using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using mpv_winui.Modules.Common.View;
using mpv_winui.Modules.FileSystem;

namespace mpv_winui.Modules.MpvConf;

public sealed partial class MpvConfEditorWindow : BaseWindow
{
    public MpvConfEditorWindow()
    {
        InitializeComponent();

        AppWindow.Title = "Mpv Conf Editor";
        AppWindow.SetIcon("App.ico");
        AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;

        SetupStyle();
    }

    private void PageFrame_Loaded(object sender, RoutedEventArgs e)
    {
        var configPath = AppData.Current.ResolveLocalData("mpv\\mpvw.conf");
        SubTitleText.Text = configPath;
        ToolTipService.SetToolTip(SubTitleText, configPath);
        PageFrame.Navigate(typeof(MpvConfEditorPage), configPath);
    }
}
