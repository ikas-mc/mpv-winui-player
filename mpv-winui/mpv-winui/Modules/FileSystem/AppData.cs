using Microsoft.Windows.Storage;
using mpv_winui.Modules.AppModel;
using System;
using System.IO;
using System.Threading.Tasks;

namespace mpv_winui.Modules.FileSystem
{
    public class AppData
    {
        private static readonly Lazy<AppData> _lazy = new(() =>
        {
            return new AppData();
        }, true);

        public static AppData Current => _lazy.Value;

        private const string AppDataId = "mpv-winui";

        public string ResolveLocalData(string path)
        {
            if (PackageHelper.IsPackaged)
            {
                var application = ApplicationData.GetDefault();
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
            else
            {
                var folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppDataId, path);
                Directory.CreateDirectory(folderPath);
                return await Windows.Storage.StorageFolder.GetFolderFromPathAsync(folderPath);
            }
        }
    }
}
