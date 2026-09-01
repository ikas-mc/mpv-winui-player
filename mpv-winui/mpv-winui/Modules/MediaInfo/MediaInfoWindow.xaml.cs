using mpv_winui.Modules.Common.View;

namespace mpv_winui.Modules.MediaInfo;

public sealed partial class MediaInfoWindow : BaseWindow
{
    public MediaInfoWindow(string? path)
    {
        InitializeComponent();

        AppWindow.Title = "Media Info";
        AppWindow.SetIcon("App.ico");
        AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;

        ContentHost.Children.Add(new MediaInfoUserControl(path));

        SetupStyle();
    }
}
