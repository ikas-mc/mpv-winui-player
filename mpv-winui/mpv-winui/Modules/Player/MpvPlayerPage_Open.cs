using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Storage.Pickers;
using mpv_winui.Modules.Common.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace mpv_winui.Modules.Player
{
    public sealed partial class MpvPlayerPage
    {
        private async Task PlayFileAsync(string path)
        {
            await _mediaPlayer.PlayUrlAsync(path, null);
        }

        private async Task OpenFileAsync()
        {
            var picker = new FileOpenPicker(_appWindow.Id);
            var file = await picker.PickSingleFileAsync();
            if (file?.Path is string path && !string.IsNullOrEmpty(path))
            {
                await PlayFileAsync(file.Path);
            }
        }

        private async Task OpenFolderAsync()
        {
            var picker = new FolderPicker(_appWindow.Id);
            var folder = await picker.PickSingleFolderAsync();
            if (folder?.Path is string path && !string.IsNullOrEmpty(path))
            {
                await PlayFileAsync(folder.Path);
            }
        }

        private async Task OpenUrlAsync()
        {
            var urlBox = new TextBox { PlaceholderText = "http://..." };
            var urlDialog = new ContentDialog
            {
                Title = "Open URL",
                Content = urlBox,
                PrimaryButtonText = "Open",
                CloseButtonText = "Cancel",
                XamlRoot = XamlRoot
            };
            if (await urlDialog.ShowAsync() == ContentDialogResult.Primary)
            {
                if (urlBox.Text?.Trim() is string path && !string.IsNullOrEmpty(path))
                {
                    await PlayFileAsync(path);
                }
            }
        }

        private async Task OpenClipboardAsync()
        {
            var package = Clipboard.GetContent();
            if (package.Contains(StandardDataFormats.Text))
            {
                //TODO 
                var text = await package.GetTextAsync();
                if (text?.Trim() is string path && !string.IsNullOrEmpty(path))
                {
                    await PlayFileAsync(path);
                }
            }
            else if (package.Contains(StandardDataFormats.Uri))
            {
                var uri = await package.GetUriAsync();
                if (uri?.ToString() is string path && !string.IsNullOrEmpty(path))
                {
                    await PlayFileAsync(path);
                }
            }
            else if (package.Contains(StandardDataFormats.StorageItems))
            {
                var storageItems = await package.GetStorageItemsAsync();
                foreach (var item in storageItems)
                {
                    PlayFileAsync(item.Path).FireAndForget(OnException);
                    //TODO
                    break;
                }
            }
        }

        private async Task OpenDvdAsync()
        {
            //TODO
            var picker = new FolderPicker(_appWindow.Id);
            var folder = await picker.PickSingleFolderAsync();
            if (folder?.Path is string path && !string.IsNullOrEmpty(path))
            {
                await PlayFileAsync(path);
            }
        }

        private async Task OpenBdAsync()
        {
            //TODO check bd
            var picker = new FolderPicker(_appWindow.Id);
            var folder = await picker.PickSingleFolderAsync();
            if (folder?.Path is string path && !string.IsNullOrEmpty(path))
            {
                await PlayFileAsync(path);
            }
        }

        private async Task LoadSubtitleAsync()
        {
            var subPicker = new FileOpenPicker(_appWindow.Id);
            var subFile = await subPicker.PickSingleFileAsync();
            if (!string.IsNullOrEmpty(subFile?.Path))
            {
                await _mediaPlayer.RunCommandAsync(["sub-add", subFile.Path]);
            }
        }

        //TODO list
        private IReadOnlyList<string>? _pendingPaths;
        private void OpenPedingPath()
        {
            if (_pendingPaths is { } paths && paths.Count > 0)
            {
                foreach (var path in paths)
                {
                    //TODO
                    PlayFileAsync(path).FireAndForget(OnException);
                    break;
                }
            }
            _pendingPaths = null;
        }
    }
}