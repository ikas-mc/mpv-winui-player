using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace mpv_winui.Modules.AppConfData;

public static class AppConfDataImportDialog
{
    public static async Task ShowAsync(XamlRoot xamlRoot, double minWidth = 600, double minHeight = 500)
    {
        var control = new AppConfDataImportDialogControl()
        {
            MinWidth = minWidth,
            MinHeight = minHeight
        };

        var dialog = new ContentDialog
        {
            Title = "Import App Configuration Data",
            Content = control,
            CloseButtonText = "Close",
            XamlRoot = xamlRoot,
        };
        dialog.Resources["ContentDialogMaxWidth"] = 900;
        dialog.Resources["ContentDialogPadding"] = new Thickness(12);

        try
        {
            await dialog.ShowAsync();
        }
        catch (Exception)
        {
        }
    }
}