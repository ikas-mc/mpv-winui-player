using mpv_winui.Modules.Common.Utils;
using mpv_winui.Modules.Player.History;
using System.Threading.Tasks;

namespace mpv_winui.Modules.Player
{
    public sealed partial class MpvPlayerPage
    {
        private async Task ShowWatchHistoryDialogAsync()
        {
            await WatchHistoryDialog.ShowAsync(XamlRoot, _mediaPlayer.GetWatchHistoryPath(), _mediaPlayer.SaveWatchHistory(), OnException, _logger, WatchHistoryControl_ItemClick);
        }

        private async Task ShowWatchLaterDialogAsync()
        {
            await WatchLaterDialog.ShowAsync(XamlRoot, _mediaPlayer.GetWatchLaterFolderPath(), OnException, _logger, WatchLaterControl_ItemClick);
        }

        private void WatchHistoryControl_ItemClick(string path)
        {
            _mediaPlayer.OpenAsync(new FileItem(path)).FireAndForget(OnException);
        }

        private void WatchLaterControl_ItemClick(string path)
        {
            _mediaPlayer.OpenAsync(new FileItem(path)).FireAndForget(OnException);
        }
    }
}
