using Microsoft.UI.Xaml;
using mpv_winui.Modules.Common.View;
using mpv_winui.Modules.Language;

namespace mpv_winui.Modules.Settings;

public sealed partial class SettingsWindow : BaseWindow
{
    public SettingsWindow()
    {
        InitializeComponent();

        AppWindow.Title = AppContext.AppLang.Settings;
        AppWindow.SetIcon("App.ico");
        AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;

        SetupStyle();
    }

    private void PageFrame_Loaded(object sender, RoutedEventArgs e)
    {
        PageFrame.Navigate(typeof(SettingsPage));
    }
}
