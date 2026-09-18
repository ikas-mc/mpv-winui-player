using mpv_winui.Modules.FileSystem;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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

                var entries = new List<(string Path, bool IsDirectory)>();
                using (var archive = ZipFile.OpenRead(archivePath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        var fullName = entry.FullName;
                        if (string.IsNullOrEmpty(fullName))
                        {
                            continue;
                        }

                        if (fullName.Contains(".."))
                        {
                            continue;
                        }

                        if (Path.EndsInDirectorySeparator(fullName))
                        {
                            entries.Add((Path.TrimEndingDirectorySeparator(fullName).Replace("/", "\\"), true));
                            continue;
                        }

                        entries.Add((fullName.Replace("/", "\\"), false));
                    }
                }

                var nodes = new Dictionary<string, AppConfDataNode>(StringComparer.Ordinal);
                foreach (var (path, isDirectory) in entries)
                {
                    var exists = isDirectory ? Directory.Exists(Path.Combine(targetRoot.Path, path)) : File.Exists(Path.Combine(targetRoot.Path, path));
                    nodes[path] = new AppConfDataNode(Path.GetFileName(path), path, isDirectory, exists);
                }

                var roots = new List<AppConfDataNode>();
                foreach (var node in nodes.Values)
                {
                    var parentPath = Path.GetDirectoryName(node.RelativePath) ?? string.Empty;
                    if (parentPath.Length > 0 && nodes.TryGetValue(parentPath, out var parent))
                    {
                        parent.Children.Add(node);
                    }
                    else
                    {
                        roots.Add(node);
                    }
                }

                return roots;
            }).ConfigureAwait(false);
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