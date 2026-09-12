using mpv_winui.Modules.AppModel;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using ApplicationData = Microsoft.Windows.Storage.ApplicationData;

namespace mpv_winui.Modules.FileSystem
{
    public class AppData
    {
        private static readonly Lazy<AppData> _lazy = new(() => new AppData(), true);

        public static AppData Current => _lazy.Value;

        public const string AppDataId = "mpvw";

        public const string AppDataPublisher = "ikas-mc";

        private static readonly Lazy<StorageFolder> _localFolder = new(EnsureLocalFolder, true);

        public string ResolveLocalData(string path)
        {
            return Path.Combine(_localFolder.Value.Path, path);
        }

        public async Task<StorageFolder> OpenOrCreateLocalDataFolderAsync(string path)
        {
            return await _localFolder.Value.CreateFolderAsync(path, CreationCollisionOption.OpenIfExists);
        }

        public Task<StorageFolder> OpenLocalDataFolderAsync()
        {
            return Task.FromResult(_localFolder.Value);
        }

        private static StorageFolder EnsureLocalFolder()
        {
            if (PackageHelper.IsPackaged)
            {
                return ApplicationData.GetDefault().LocalFolder;
            }
            else
            {
                //https://learn.microsoft.com/zh-cn/windows/windows-app-sdk/api/winrt/microsoft.windows.storage.applicationdata.getforunpackaged
                var applicationData = ApplicationData.GetForUnpackaged(AppDataPublisher, AppDataId);
                Directory.CreateDirectory(applicationData.LocalPath);
                return applicationData.LocalFolder;
            }
        }
    }
}
