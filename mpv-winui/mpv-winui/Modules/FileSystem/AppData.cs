using Microsoft.Windows.Storage;
using mpv_winui.Modules.AppModel;
using System;
using System.IO;
using System.Threading.Tasks;

namespace mpv_winui.Modules.FileSystem
{
    public class AppData
    {
        private static readonly Lazy<AppData> _lazy = new(() => new AppData(), true);

        public static AppData Current => _lazy.Value;

        public const string AppDataId = "mpvw";

        public const string AppDataPublisher = "ikas-mc";

        //https://learn.microsoft.com/zh-cn/windows/windows-app-sdk/api/winrt/microsoft.windows.storage.applicationdata.getforunpackaged
        private readonly bool _useUnpackagedAppData = true;

        public string ResolveLocalData(string path)
        {
            if (PackageHelper.IsPackaged)
            {
                var application = ApplicationData.GetDefault();
                return Path.Combine(application.LocalPath, path);
            }
            else if (_useUnpackagedAppData)
            {
                var application = ApplicationData.GetForUnpackaged(AppDataPublisher, AppDataId);
                return Path.Combine(application.LocalPath, path);
            }
            else
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppDataId, path);
            }
        }

        public async Task<Windows.Storage.StorageFolder> OpenOrCreateLocalDataFolderAsync(string path)
        {
            if (PackageHelper.IsPackaged)
            {
                return await ApplicationData.GetDefault().LocalFolder.CreateFolderAsync(path, Windows.Storage.CreationCollisionOption.OpenIfExists);
            }
            else if (_useUnpackagedAppData)
            {
                return await ApplicationData.GetForUnpackaged(AppDataPublisher, AppDataId).LocalFolder.CreateFolderAsync(path, Windows.Storage.CreationCollisionOption.OpenIfExists);
            }
            else
            {
                return await Task.Run(async () =>
                {
                    var folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppDataId, path);
                    Directory.CreateDirectory(folderPath);
                    return await Windows.Storage.StorageFolder.GetFolderFromPathAsync(folderPath);
                });
            }
        }

        public async Task<Windows.Storage.StorageFolder> OpenLocalDataFolderAsync()
        {
            if (PackageHelper.IsPackaged)
            {
                return ApplicationData.GetDefault().LocalFolder;
            }
            else if (_useUnpackagedAppData)
            {
                return ApplicationData.GetForUnpackaged(AppDataPublisher, AppDataId).LocalFolder;
            }
            else
            {
                return await Task.Run(async () =>
                 {
                     var folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppDataId);
                     Directory.CreateDirectory(folderPath);
                     return await Windows.Storage.StorageFolder.GetFolderFromPathAsync(folderPath);
                 });
            }
        }
    }
}
