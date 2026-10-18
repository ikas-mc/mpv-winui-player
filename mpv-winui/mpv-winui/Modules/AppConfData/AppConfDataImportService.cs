using mpv_winui.Modules.FileSystem;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace mpv_winui.Modules.AppConfData
{
    public class AppConfDataImportService
    {
        private static readonly Logger _logger = LogManager.GetLogger("AppConfData");

        private static readonly Lazy<AppConfDataImportService> _lazy = new(() => new AppConfDataImportService(), true);

        public static AppConfDataImportService Instance => _lazy.Value;

        private AppConfDataImportService()
        {
        }

        public Task<StorageFolder> GetTargetFolderAsync()
        {
            return AppData.Current.OpenLocalDataFolderAsync();
        }

        public async Task<List<AppConfDataNode>> ReadArchiveAsync(string archivePath)
        {
            if (string.IsNullOrEmpty(archivePath))
            {
                return [];
            }

            return await Task.Run(async () =>
            {
                var targetRoot = await AppData.Current.OpenLocalDataFolderAsync();

                var entries = ReadArchiveEntries(archivePath);
                return BuildTree(entries, targetRoot.Path);
            }).ConfigureAwait(false);
        }

        private static List<(string Path, bool IsDirectory)> ReadArchiveEntries(string archivePath)
        {
            var entries = new List<(string Path, bool IsDirectory)>();

            using (var archive = ZipFile.OpenRead(archivePath))
            {
                foreach (var entry in archive.Entries)
                {
                    var fullName = entry.FullName;
                    if (string.IsNullOrEmpty(fullName) || fullName.Contains(".."))
                    {
                        continue;
                    }

                    var isDirectory = Path.EndsInDirectorySeparator(fullName);
                    var path = (isDirectory ? Path.TrimEndingDirectorySeparator(fullName) : fullName).Replace('/', '\\');
                    if (path.Length == 0)
                    {
                        continue;
                    }

                    entries.Add((path, isDirectory));
                }
            }

            return entries;
        }

        private static List<AppConfDataNode> BuildTree(List<(string Path, bool IsDirectory)> entries, string targetRootPath)
        {
            var nodes = new Dictionary<string, AppConfDataNode>(StringComparer.Ordinal);
            var roots = new List<AppConfDataNode>();

            AppConfDataNode GetOrAddNode(string path, bool isDirectory)
            {
                if (nodes.TryGetValue(path, out var existing))
                {
                    return existing;
                }

                var missing = new List<(string Path, bool IsDirectory)>();
                var current = path;
                var currentIsDirectory = isDirectory;
                while (!nodes.ContainsKey(current))
                {
                    missing.Add((current, currentIsDirectory));
                    currentIsDirectory = true;

                    var parentPath = Path.GetDirectoryName(current);
                    if (string.IsNullOrEmpty(parentPath))
                    {
                        break;
                    }

                    current = parentPath;
                }

                for (var i = missing.Count - 1; i >= 0; i--)
                {
                    var (nodePath, nodeIsDirectory) = missing[i];
                    var node = new AppConfDataNode(Path.GetFileName(nodePath), nodePath, nodeIsDirectory, TargetExists(targetRootPath, nodePath, nodeIsDirectory));
                    nodes.Add(nodePath, node);

                    var parentPath = Path.GetDirectoryName(nodePath);
                    if (!string.IsNullOrEmpty(parentPath) && nodes.TryGetValue(parentPath, out var parent))
                    {
                        parent.Children.Add(node);
                    }
                    else
                    {
                        roots.Add(node);
                    }
                }

                return nodes[path];
            }

            foreach (var (path, isDirectory) in entries)
            {
                GetOrAddNode(path, isDirectory);
            }

            return roots;
        }

        private static bool TargetExists(string targetRootPath, string relativePath, bool isDirectory)
        {
            var fullPath = Path.Combine(targetRootPath, relativePath);
            return isDirectory ? Directory.Exists(fullPath) : File.Exists(fullPath);
        }

        public async Task<List<string>> CollectSelectedPathsAsync(IReadOnlyList<AppConfDataNode> selectedNodes)
        {
            return await Task.Run(() =>
            {
                var paths = new List<string>();
                var pending = new Stack<AppConfDataNode>();
                for (var i = selectedNodes.Count - 1; i >= 0; i--)
                {
                    pending.Push(selectedNodes[i]);
                }

                while (pending.Count > 0)
                {
                    var node = pending.Pop();
                    if (!node.IsDirectory)
                    {
                        paths.Add(node.RelativePath);
                        continue;
                    }

                    for (var i = node.Children.Count - 1; i >= 0; i--)
                    {
                        pending.Push(node.Children[i]);
                    }
                }

                return paths.Distinct().ToList();
            });
        }

        public async Task<int> ImportAsync(string archivePath, IReadOnlyList<string> selectedPaths, bool overwrite)
        {
            if (string.IsNullOrEmpty(archivePath) || selectedPaths is null || selectedPaths.Count == 0)
            {
                return 0;
            }

            return await Task.Run(async () =>
            {
                var appDataFolder = await AppData.Current.OpenLocalDataFolderAsync();
                var appDataPath = appDataFolder.Path + "\\";

                var imported = 0;

                using (var archive = ZipFile.OpenRead(archivePath))
                {
                    foreach (var path in selectedPaths)
                    {
                        if (string.IsNullOrEmpty(path))
                        {
                            continue;
                        }

                        if (path.Contains(".."))
                        {
                            continue;
                        }

                        var entry = archive.GetEntry(path.Replace("\\", "/"));
                        if (entry is null)
                        {
                            continue;
                        }

                        var targetPath = Path.Combine(appDataFolder.Path, path);
                        if (!targetPath.StartsWith(appDataPath, StringComparison.Ordinal))
                        {
                            _logger.Warn("skip entry outside of app data root, entry={}", path);
                            continue;
                        }

                        if (File.Exists(targetPath) && !overwrite)
                        {
                            continue;
                        }

                        var dir = Path.GetDirectoryName(targetPath);
                        if (!string.IsNullOrEmpty(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }

                        try
                        {
                            entry.ExtractToFile(targetPath, overwrite);
                            imported++;
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "extract failed, path={}", targetPath);
                        }
                    }
                }

                return imported;
            });
        }
    }
}