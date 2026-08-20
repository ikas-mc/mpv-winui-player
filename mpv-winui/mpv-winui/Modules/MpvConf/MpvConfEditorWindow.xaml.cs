using Microsoft.UI.Xaml;
using mpv_winui.Modules.Common.View;
using mpv_winui.Modules.FileSystem;
using Windows.Graphics;

namespace mpv_winui.Modules.MpvConf;

public sealed partial class MpvConfEditorWindow : Window
{
    private WindowStyleManager? _styleManager;

    public MpvConfEditorWindow()
    {
        InitializeComponent();

        Closed += MpvConfigWindow_Closed;

        AppWindow.Title = "mpv Config";
        AppWindow.SetIcon("App.ico");
        AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;

        _styleManager = new WindowStyleManager(this);
        _styleManager?.Setup();
    }

    private void PageFrame_Loaded(object sender, RoutedEventArgs e)
    {
        //TODO
        var configPath = AppData.Current.ResolveLocalData("mpv\\mpvw.conf");

        PageFrame.Navigate(typeof(MpvConfEditorPage), configPath);
    }

    private void MpvConfigWindow_Closed(object sender, WindowEventArgs args)
    {
        Closed -= MpvConfigWindow_Closed;
        _styleManager?.Dispose();
        _styleManager = null;
    }

    public void MoveAndResize(RectInt32 rect)
    {
        AppWindow?.MoveAndResize(rect);
    }

    public void UpdateCurrentTheme()
    {
        var theme = _styleManager?.GetThemeType();
        if (theme is not null)
        {
            _styleManager?.UpdateTheme(theme.Value);
        }
    }

    public void UpdateCurrentBackdrop()
    {
        var backdropType = _styleManager?.GetBackdropType();
        if (!string.IsNullOrEmpty(backdropType))
        {
            _styleManager?.UpdateBackdrop(backdropType);
        }
    }
}
