using mpv_winrt;
using mpv_winui.Modules.AppModel;
using mpv_winui.Modules.Common.Utils;

namespace mpv_winui.Modules.Player
{
    public sealed partial class MpvPlayerPage
    {
        private void MpvPlayerPage_MediaInfoChanged(MediaInfoChangedEventArgs args)
        {
            DispatcherQueue.RunAsync(() =>
            {
                if (!string.IsNullOrEmpty(args.MediaTitle))
                {
                    UpdatePageTitle(args.MediaTitle);
                }
                else if (!string.IsNullOrEmpty(args.Filename))
                {
                    UpdatePageTitle(args.Filename);
                }
                else
                {
                    UpdatePageTitle(PackageHelper.AppName);
                }
            });
        }

        private void UpdatePageTitle(string title)
        {
            if (App.Window is MainWindow window)
            {
                window.UpdateTitle(title);
            }
        }
    }
}
