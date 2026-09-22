using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NLog;
using System;
using System.Threading.Tasks;

namespace mpv_winui.Modules.Player.History;

public static class WatchLaterDialog
{
    public static async Task ShowAsync(XamlRoot xamlRoot, string? directory, Action<Exception>? onException, Logger logger, Action<string>? onItemClick, double minWidth = 600, double minHeight = 500)
    {
        var control = new WatchLaterControl()
        {
            MinWidth = minWidth,
            MinHeight = minHeight
        };
        control.Initialize(directory, onException, logger);

        var dialog = new ContentDialog
        {
            Title = "Watch Later",
            Content = control,
            CloseButtonText = "Close",
            XamlRoot = xamlRoot,
        };
        dialog.Resources["ContentDialogMaxWidth"] = 900;
        dialog.Resources["ContentDialogPadding"] = new Thickness(12);

        control.ItemClick += (_, p) =>
        {
            dialog.Hide();
            onItemClick?.Invoke(p);
        };

        try
        {
            await dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            onException?.Invoke(ex);
        }
    }
}