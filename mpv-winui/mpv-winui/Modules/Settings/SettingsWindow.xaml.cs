using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Graphics;

namespace mpv_winui.Modules.Settings;

public sealed partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();

        var appWindow = AppWindow;
        appWindow.Title = "Settings";

        //appWindow.SetIcon("\\App.ico");
        appWindow.TitleBar.ExtendsContentIntoTitleBar = true;

        appWindow.TitleBar.ForegroundColor = Colors.White;
        appWindow.TitleBar.ButtonForegroundColor = Colors.White;
        appWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
        appWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

        if (appWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsMaximizable = true;
            presenter.IsResizable = true;
        }
    }

    private void PageFrame_Loaded(object sender, RoutedEventArgs e)
    {
        PageFrame.Navigate(typeof(SettingsPage));
    }

    public void MoveAndResize(RectInt32 rect)
    {
        AppWindow?.MoveAndResize(rect);
    }
}