using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using mpv_winui.Modules.Common.View;

namespace mpv_winui.Modules.Menu.MenuEditor;

public sealed partial class MenuEditorWindow : BaseWindow
{
    private readonly string _filePath;
    private readonly MenuType _type;

    public MenuEditorWindow(string filePath, MenuType type)
    {
        InitializeComponent();

        _filePath = filePath;
        _type = type;

        AppWindow.Title = "Menu Editor";
        AppWindow.SetIcon("App.ico");
        AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;

        SetupStyle();
    }

    private void PageFrame_Loaded(object sender, RoutedEventArgs e)
    {
        TitleText.Text = _type == MenuType.ContextMenu ? "Mpv Context Menu" : "Menu Bar";
        SubTitleText.Text = _filePath;
        ToolTipService.SetToolTip(SubTitleText, _filePath);

        PageFrame.Navigate(typeof(MenuEditorPage), new MenuEditorParam(_filePath, _type));
    }
}
