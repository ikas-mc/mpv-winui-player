using Microsoft.UI.Xaml.Controls;
using mpv_winui.Modules.Common.Utils;
using mpv_winui.Modules.Player.History;
using System;
using System.Threading.Tasks;

namespace mpv_winui.Modules.Player
{
    public sealed partial class MpvPlayerPage
    {
        private WeakReference<ContentDialog>? _watchHistoryDialog;
        private WeakReference<ContentDialog>? _watchLaterDialog;

        private async Task ShowWatchHistoryDialogAsync()
        {
            var control = new WatchHistoryControl();
            control.Initialize(_mediaPlayer.WatchHistoryPath, _mediaPlayer.SaveWatchHistory, OnException, _logger);
            control.ItemClick += WatchHistoryControl_ItemClick;

            var dialog = new ContentDialog
            {
                Title = "Watch History",
                Content = control,
                CloseButtonText = "Close",
                XamlRoot = XamlRoot
            };
            _watchHistoryDialog = new WeakReference<ContentDialog>(dialog);
            await dialog.ShowAsync();

            control.ItemClick -= WatchHistoryControl_ItemClick;
        }

        private async Task ShowWatchLaterDialogAsync()
        {
            var control = new WatchLaterControl();
            control.Initialize(_mediaPlayer.WatchLaterFolderPath, OnException, _logger);
            control.ItemClick += WatchLaterControl_ItemClick;
            var dialog = new ContentDialog
            {
                Title = "Watch Later",
                Content = control,
                CloseButtonText = "Close",
                XamlRoot = XamlRoot
            };
            _watchLaterDialog = new WeakReference<ContentDialog>(dialog);
            await dialog.ShowAsync();

            control.ItemClick -= WatchLaterControl_ItemClick;
        }

        private void WatchHistoryControl_ItemClick(object? sender, string path)
        {
            if (_watchHistoryDialog?.TryGetTarget(out var dialog) == true)
            {
                dialog.Hide();
                _watchHistoryDialog = null;
            }
            _mediaPlayer.OpenAsync(new FileItem(path)).FireAndForget(OnException);
        }

        private void WatchLaterControl_ItemClick(object? sender, string path)
        {
            if (_watchLaterDialog?.TryGetTarget(out var dialog) == true)
            {
                dialog.Hide();
                _watchLaterDialog = null;
            }
            _mediaPlayer.OpenAsync(new FileItem(path)).FireAndForget(OnException);
        }
    }
}
