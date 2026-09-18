using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using mpv_winui.Modules.AppConfData;
using System;
using System.Threading.Tasks;

namespace mpv_winui.Modules.Welcome
{
    public static class WelcomeWizardDialog
    {
        public static async Task ShowAsync(XamlRoot xamlRoot)
        {
            var control = WelcomeWizardControl.Create();
            var dialog = new ContentDialog
            {
                Title = "Welcome",
                Content = control,
                CloseButtonText = "Close",
                XamlRoot = xamlRoot,
            };

            dialog.Resources["ContentDialogMaxWidth"] = 460;
            dialog.Resources["ContentDialogPadding"] = new Thickness(12);

            control.ImportRequested += async (_, _) =>
            {
                dialog.Hide();
                await ShowConfDataImportPromptIfNeededAsync(xamlRoot);
            };

            try
            {
                await dialog.ShowAsync();
            }
            catch (Exception)
            {
            }
        }

        private static async Task ShowConfDataImportPromptIfNeededAsync(XamlRoot xamlRoot)
        {
            await AppConfDataImportDialog.ShowAsync(xamlRoot);
        }
    }
}