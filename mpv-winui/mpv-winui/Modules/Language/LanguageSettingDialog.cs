using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace mpv_winui.Modules.Language
{
    public static class LanguageSettingDialog
    {
        public static async Task ShowAsync(XamlRoot xamlRoot)
        {
            var dialog = new ContentDialog
            {
                Title = AppContext.AppLang.Language,
                Content = new LanguageSettingControl(),
                CloseButtonText = AppContext.AppLang.Close,
                XamlRoot = xamlRoot,
            };

            dialog.Resources["ContentDialogMaxWidth"] = 460;
            dialog.Resources["ContentDialogPadding"] = new Thickness(12);

            await dialog.ShowAsync();
        }
    }
}
