using NLog;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mpv_winui.Modules.FileSystem
{
    public class FileService
    {
        private static readonly Logger _logger = LogManager.GetLogger(nameof(FileService));

        private static readonly Lazy<FileService> _lazy = new(() => new FileService(), true);

        public static FileService Instance => _lazy.Value;

        private FileService()
        {
        }

        public Task<string> ReadAsync(string path)
        {
            return File.ReadAllTextAsync(path, Encoding.UTF8);
        }

        public Task<string[]> ReadAllLinesAsync(string path)
        {
            return File.ReadAllLinesAsync(path, Encoding.UTF8);
        }

        public async Task BackAndSaveAsync(string path, string content, bool backup, int limit = 50)
        {
            if (backup && File.Exists(path))
            {
                await Task.Run(() => DoBackup(path, limit)).ConfigureAwait(false);
            }

            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            await File.WriteAllTextAsync(path, content);
        }

        public async Task BackAndSaveAsync(string path, byte[] content, bool backup, int limit = 50)
        {
            if (backup && File.Exists(path))
            {
                await Task.Run(() => DoBackup(path, limit)).ConfigureAwait(false);
            }

            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            await File.WriteAllBytesAsync(path, content).ConfigureAwait(false);
        }

        private void DoBackup(string path, int limit)
        {
            var fileName = Path.GetFileName(path);
            var ext = Path.GetExtension(fileName);
            var nameWithoutExt = string.IsNullOrEmpty(ext) ? fileName : fileName[..^ext.Length];

            var folder = Path.Combine(AppData.Current.ResolveLocalData("backup"), fileName);
            Directory.CreateDirectory(folder);

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff");
            var backupPath = Path.Combine(folder, nameWithoutExt + "." + timestamp + ext);

            File.Copy(path, backupPath, overwrite: true);
            EnforceLimit(folder, nameWithoutExt, limit);
        }

        private static void EnforceLimit(string folder, string fileBase, int limit)
        {
            if (limit <= 0)
            {
                return;
            }

            var prefix = fileBase + ".";
            var backups = Directory.GetFiles(folder)
                .Where(f => Path.GetFileName(f).StartsWith(prefix, StringComparison.Ordinal))
                .OrderByDescending(File.GetCreationTime)
                .ToList();

            for (var i = limit; i < backups.Count; i++)
            {
                try
                {
                    File.Delete(backups[i]);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error deleting backup file: {0}", backups[i]);
                }
            }
        }
    }
}