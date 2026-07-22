using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Storage.Pickers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace mpv_winui.Modules.Player
{
    public sealed partial class MpvPlayerPage
    {
        private async Task OpenFileAsync()
        {
            var picker = new FileOpenPicker(_appWindow.Id);
            var files = await picker.PickMultipleFilesAsync();
            if (files?.Count > 0)
            {
                await PlayFiles([.. files.Select(x => x.Path)], OpenMode.Replace);
            }
        }

        private async Task OpenFolderAsync()
        {
            var picker = new FolderPicker(_appWindow.Id);
            var folders = await picker.PickMultipleFoldersAsync();
            if (folders?.Count > 0)
            {
                await PlayFolders([.. folders.Select(x => x.Path)], OpenMode.Replace);
            }
        }

        private async Task OpenUrlAsync()
        {
            var urlBox = new TextBox
            {
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                PlaceholderText = "http://...",
                MinHeight = 80,
                MaxHeight = 200,
                MinWidth = 400
            };
            var urlDialog = new ContentDialog
            {
                Title = "Open URL",
                Content = urlBox,
                PrimaryButtonText = "Open",
                CloseButtonText = "Cancel",
                XamlRoot = XamlRoot
            };

            _suppressKeyboard = true;
            try
            {
                if (await urlDialog.ShowAsync() == ContentDialogResult.Primary)
                {
                    if (urlBox.Text?.Trim() is string path && !string.IsNullOrEmpty(path))
                    {
                        await PlayUrl(path, OpenMode.Replace);
                    }
                }
            }
            finally
            {
                _suppressKeyboard = false;
            }
        }

        private async Task OpenClipboardAsync()
        {
            var package = Clipboard.GetContent();
            if (package.Contains(StandardDataFormats.Text))
            {
                var text = await package.GetTextAsync();
                if (text?.Trim() is string path && !string.IsNullOrEmpty(path))
                {
                    await PlayUrl(path, OpenMode.Replace);
                }
            }
            else if (package.Contains(StandardDataFormats.Uri))
            {
                var uri = await package.GetUriAsync();
                if (uri?.ToString() is string path && !string.IsNullOrEmpty(path))
                {
                    await PlayUrl(path, OpenMode.Replace);
                }
            }
            else if (package.Contains(StandardDataFormats.StorageItems))
            {
                var storageItems = await package.GetStorageItemsAsync();
                if (storageItems?.Count > 0)
                {
                    await PlayStorageItems(storageItems, OpenMode.Replace);
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
                await PlayFolder(path, OpenMode.Replace);
            }
        }

        private async Task OpenBdAsync()
        {
            //TODO check bd
            var picker = new FolderPicker(_appWindow.Id);
            var folder = await picker.PickSingleFolderAsync();
            if (folder?.Path is string path && !string.IsNullOrEmpty(path))
            {
                await PlayFolder(path, OpenMode.Replace);
            }
        }

        private async Task LoadSubtitleAsync()
        {
            var subPicker = new FileOpenPicker(_appWindow.Id);
            var subFile = await subPicker.PickSingleFileAsync();
            if (!string.IsNullOrEmpty(subFile?.Path))
            {
                _mediaPlayer.AddSubtitle(subFile.Path, true);
            }
        }

        //TODO list
        private IReadOnlyList<FileItem>? _pendingPaths;
        private async ValueTask OpenPendingPath()
        {
            if (_pendingPaths is { } paths && paths.Count > 0)
            {
                await _mediaPlayer.OpenAsync(paths, OpenMode.Replace);
            }
            _pendingPaths = null;
        }

        // move
        private async ValueTask PlayStorageItems(IReadOnlyList<IStorageItem> storageItems, OpenMode openMode)
        {
            var items = storageItems
                .Where(x => !x.IsOfType(StorageItemTypes.None))
                .Select(x => new FileItem(x.Path, x.IsOfType(Windows.Storage.StorageItemTypes.File) ? FileType.File : FileType.Folder))
                .ToList();

            if (items?.Count > 0)
            {
                await _mediaPlayer.OpenAsync(items, openMode);
            }
        }

        private async ValueTask PlayFiles(IReadOnlyList<string> files, OpenMode openMode)
        {
            var items = files.Select(file => new FileItem(file, FileType.File)).ToList();
            await _mediaPlayer.OpenAsync(items, OpenMode.Replace);
        }

        private async ValueTask PlayFolders(IReadOnlyList<string> folders, OpenMode openMode)
        {
            var items = folders.Select(file => new FileItem(file, FileType.Folder)).ToList();
            await _mediaPlayer.OpenAsync(items, OpenMode.Replace);
        }

        private async ValueTask PlayUrl(string url, OpenMode openMode)
        {
            await _mediaPlayer.OpenAsync([new FileItem(url, FileType.Url)], openMode);
        }

        private async ValueTask PlayFile(string file, OpenMode openMode)
        {
            await _mediaPlayer.OpenAsync([new FileItem(file, FileType.File)], openMode);
        }

        private async ValueTask PlayFolder(string folder, OpenMode openMode)
        {
            await _mediaPlayer.OpenAsync([new FileItem(folder, FileType.Folder)], openMode);
        }
    }
}