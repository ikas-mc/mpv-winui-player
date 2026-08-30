using Microsoft.Windows.Storage.Pickers;
using System;
using System.Threading.Tasks;

namespace mpv_winui.Modules.Player
{
    public sealed partial class MpvPlayerPage
    {
        private volatile bool _discMenuActive = false;

        private void MpvPlayerPage_DiscMenuActiveChanged(bool isActive)
        {
            _discMenuActive = isActive;

            if (_logger.IsDebugEnabled)
            {
                _logger.Debug("Disc Menu Active Changed, isActive={}", isActive);
            }
        }

        private async Task OpenDvdAsync()
        {
            var picker = new FolderPicker(_appWindow.Id);
            var folder = await picker.PickSingleFolderAsync();
            if (folder?.Path is string path && !string.IsNullOrEmpty(path))
            {
                await _mediaPlayer.OpenDvdAsync(path);
            }
        }

        private async Task OpenBdAsync()
        {
            var picker = new FolderPicker(_appWindow.Id);
            var folder = await picker.PickSingleFolderAsync();
            if (folder?.Path is string path && !string.IsNullOrEmpty(path))
            {
                await _mediaPlayer.OpenBdAsync(path);
            }
        }

        private async Task OpenDvdaAsync()
        {
            var picker = new FolderPicker(_appWindow.Id);
            var folder = await picker.PickSingleFolderAsync();
            if (folder?.Path is string path && !string.IsNullOrEmpty(path))
            {
                await _mediaPlayer.OpenDvdaAsync(path);
            }
        }

        private async Task OpenCddaAsync()
        {
            var picker = new FolderPicker(_appWindow.Id);
            var folder = await picker.PickSingleFolderAsync();
            if (folder?.Path is string path && !string.IsNullOrEmpty(path))
            {
                await _mediaPlayer.OpenCddaAsync(path);
            }
        }

        private async Task OpenCdImgAsync()
        {
            var picker = new FileOpenPicker(_appWindow.Id);
            picker.FileTypeFilter.Add(".bin");
            var file = await picker.PickSingleFileAsync();
            if (file?.Path is string path && !string.IsNullOrEmpty(path))
            {
                // TODO [cdda] Can't open CDDA device.
                path = path.Replace('\\', '/');

                await _mediaPlayer.OpenCddaAsync(path);
            }
        }

        private async Task OpenDvdImgAsync()
        {
            var picker = new FileOpenPicker(_appWindow.Id);
            picker.FileTypeFilter.Add(".img");
            picker.FileTypeFilter.Add(".iso");
            var file = await picker.PickSingleFileAsync();
            if (file?.Path is string path && !string.IsNullOrEmpty(path))
            {
                await _mediaPlayer.OpenDvdAsync(path);
            }
        }

        private async Task OpenDvdaImgAsync()
        {
            var picker = new FileOpenPicker(_appWindow.Id);
            picker.FileTypeFilter.Add(".img");
            picker.FileTypeFilter.Add(".iso");
            var file = await picker.PickSingleFileAsync();
            if (file?.Path is string path && !string.IsNullOrEmpty(path))
            {
                await _mediaPlayer.OpenDvdaAsync(path);
            }
        }

        private async Task OpenBdImgAsync()
        {
            var picker = new FileOpenPicker(_appWindow.Id);
            picker.FileTypeFilter.Add(".img");
            picker.FileTypeFilter.Add(".iso");
            var file = await picker.PickSingleFileAsync();
            if (file?.Path is string path && !string.IsNullOrEmpty(path))
            {
                await _mediaPlayer.OpenBdAsync(path);
            }
        }
    }
}