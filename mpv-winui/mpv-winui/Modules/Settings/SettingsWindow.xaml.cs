using Microsoft.UI.Xaml;
using mpv_winui.Modules.Common.View;
using Windows.Graphics;

namespace mpv_winui.Modules.Settings;

public sealed partial class SettingsWindow : Window
{
    private WindowStyleManager? _styleManager;

    public SettingsWindow()
    {
        InitializeComponent();

        Closed += SettingsWindow_Closed;

        AppWindow.Title = "Settings";
        AppWindow.SetIcon("App.ico");
        AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;

        _styleManager = new WindowStyleManager(this);
        _styleManager?.Setup();
    }

    private void SettingsWindow_Closed(object sender, WindowEventArgs args)
    {
        Closed -= SettingsWindow_Closed;
        _styleManager?.Dispose();
        _styleManager = null;
    }

    private void PageFrame_Loaded(object sender, RoutedEventArgs e)
    {
        PageFrame.Navigate(typeof(SettingsPage));
    }

    public void MoveAndResize(RectInt32 rect)
    {
        AppWindow?.MoveAndResize(rect);
    }

    public void UpdateCurrentTheme()
    {
        _styleManager?.UpdateTheme(_styleManager.GetThemeType());
    }
}