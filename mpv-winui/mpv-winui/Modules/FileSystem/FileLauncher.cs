using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;

namespace mpv_winui.Modules.FileSystem
{
    internal class FileLauncher
    {
        public static async ValueTask LaunchFolderAsync(StorageFolder? storageFolder)
        {
            if (storageFolder is null)
            {
                return;
            }

            await Launcher.LaunchFolderAsync(storageFolder);
        }

        public static async ValueTask LaunchFolderAsync(StorageFile? storageFile)
        {
            if (storageFile is null)
            {
                return;
            }

            await Launcher.LaunchFileAsync(storageFile);
        }

        public static async ValueTask ShellLaunchFileAsync(string? path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            await Task.Run(async () =>
            {
                if (Uri.TryCreate(path, UriKind.Absolute, out var uri) && !uri.IsFile)
                {
                    await Launcher.LaunchUriAsync(uri);
                    return;
                }

                Process.Start("explorer.exe", "/select,\"" + path + "\"");
            });
        }
    }
}
