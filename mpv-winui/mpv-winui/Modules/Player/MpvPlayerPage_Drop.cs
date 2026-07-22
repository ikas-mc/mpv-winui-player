using Microsoft.UI.Xaml;
using mpv_winui.Modules.Common.Utils;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace mpv_winui.Modules.Player
{
    public sealed partial class MpvPlayerPage
    {
        private void OnDragEnter(object sender, DragEventArgs e)
        {
            var defer = e.GetDeferral();
            try
            {
                var dataPackageView = e.DataView;
                if (dataPackageView.Contains(StandardDataFormats.StorageItems))
                {
                    e.AcceptedOperation = DataPackageOperation.Link;
                }
                else
                {
                    e.AcceptedOperation = DataPackageOperation.None;
                }
            }
            finally
            {
                defer.Complete();
            }
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            e.DragUIOverride.IsCaptionVisible = false;
            e.DragUIOverride.IsContentVisible = true;
            e.DragUIOverride.IsGlyphVisible = true;
            e.Handled = true;
        }

        private async void OnDrop(object sender, DragEventArgs e)
        {
            var items = new List<IStorageItem>();
            var defer = e.GetDeferral();
            try
            {
                var dataPackageView = e.DataView;
                if (dataPackageView.Contains(StandardDataFormats.StorageItems))
                {
                    var storageItems = await dataPackageView.GetStorageItemsAsync();

                    foreach (var item in storageItems)
                    {
                        items.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                OnException(ex);
            }
            finally
            {
                defer.Complete();
            }

            if (items.Count > 0)
            {
                await PlayStorageItems(items, OpenMode.Replace);
            }
        }

    }
}