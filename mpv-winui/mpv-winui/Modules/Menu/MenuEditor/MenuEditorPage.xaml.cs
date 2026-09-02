using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using NLog;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace mpv_winui.Modules.Menu.MenuEditor;

public sealed record MenuEditorParam(string FilePath, MenuType Type);

public sealed partial class MenuEditorPage : Page
{
    private static readonly Logger _logger = LogManager.GetLogger("MenuEditor");

    private string _filePath = string.Empty;
    private MenuType _type = MenuType.Menubar;

    public ObservableCollection<MenuTreeItem> Nodes { get; } = [];
    public ObservableCollection<string> BreadcrumbItems { get; } = [];

    private MenuTreeItem? _selected;
    private bool _suppressEditor;

    public MenuEditorPage()
    {
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is not MenuEditorParam param || string.IsNullOrEmpty(param.FilePath))
        {
            return;
        }

        _filePath = param.FilePath;
        _type = param.Type;
        await LoadAsync();
    }

    private void SelectNode(MenuTreeItem? node)
    {
        _selected = node;
        _suppressEditor = true;

        if (node is not null)
        {
            KindMenuItemRadio.IsChecked = node.Kind == MenuTreeItemKind.CmdMenu;
            KindSubmenuRadio.IsChecked = node.Kind == MenuTreeItemKind.Menu;
            KindSeparatorRadio.IsChecked = node.Kind == MenuTreeItemKind.Separator;

            NameBox.Text = node.Name;
            CommandBox.Text = node.Command;
            HiddenBox.Text = node.Hidden ?? string.Empty;
            DisabledBox.Text = node.Disabled ?? string.Empty;
            CheckedBox.Text = node.Checked ?? string.Empty;
        }
        else
        {
            KindSubmenuRadio.IsChecked = true;

            NameBox.Text = string.Empty;
            CommandBox.Text = string.Empty;
            HiddenBox.Text = string.Empty;
            DisabledBox.Text = string.Empty;
            CheckedBox.Text = string.Empty;
        }

        _suppressEditor = false;
        UpdateVisualState();
        UpdateBreadcrumb();
    }

    private void KindRadio_Checked(object sender, RoutedEventArgs e)
    {
        if (_suppressEditor)
        {
            return;
        }

        if (_selected is { } node)
        {
            if (KindSeparatorRadio.IsChecked == true)
            {
                node.Kind = MenuTreeItemKind.Separator;
                node.IsSeparator = true;
            }
            else if (KindSubmenuRadio.IsChecked == true)
            {
                node.Kind = MenuTreeItemKind.Menu;
                node.IsSeparator = false;
            }
            else
            {
                node.Kind = MenuTreeItemKind.CmdMenu;
                node.IsSeparator = false;
            }
        }

        UpdateVisualState();
        UpdateBreadcrumb();
    }

    private void NameBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_suppressEditor || _selected is null)
        {
            return;
        }

        _selected.Name = NameBox.Text;
        UpdateBreadcrumb();
    }

    private void CommandBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_suppressEditor || _selected is null)
        {
            return;
        }

        _selected.Command = CommandBox.Text;
    }

    private void StateBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_suppressEditor || _selected is null)
        {
            return;
        }

        _selected.Hidden = HiddenBox.Text;
        _selected.Disabled = DisabledBox.Text;
        _selected.Checked = CheckedBox.Text;
    }

    private void UpdateVisualState()
    {
        var kind = _selected?.Kind ?? MenuTreeItemKind.Menu;

        if (_type == MenuType.Menubar)
        {
            VisualStateManager.GoToState(this, "PreviewMenubar", true);
            if (_selected is null)
            {
                VisualStateManager.GoToState(this, "EditorNone", true);
            }
            else if (kind == MenuTreeItemKind.Separator)
            {
                VisualStateManager.GoToState(this, "EditorSeparator", true);
            }
            else if (kind == MenuTreeItemKind.Menu)
            {
                VisualStateManager.GoToState(this, "EditorSubmenu", true);
            }
            else
            {
                VisualStateManager.GoToState(this, "EditorMenuItemMenubar", true);
            }
        }
        else
        {
            VisualStateManager.GoToState(this, "PreviewMpvMenu", true);
            if (_selected is null)
            {
                VisualStateManager.GoToState(this, "EditorNone", true);
            }
            else if (kind == MenuTreeItemKind.Separator)
            {
                VisualStateManager.GoToState(this, "EditorSeparator", true);
            }
            else if (kind == MenuTreeItemKind.Menu)
            {
                VisualStateManager.GoToState(this, "EditorSubmenu", true);
            }
            else
            {
                VisualStateManager.GoToState(this, "EditorMenuItemMpv", true);
            }
        }
    }

    private void UpdateBreadcrumb()
    {
        BreadcrumbItems.Clear();
        var chain = new List<MenuTreeItem>();
        for (var n = _selected; n is not null; n = n.Parent)
        {
            chain.Insert(0, n);
        }

        foreach (var n in chain)
        {
            BreadcrumbItems.Add(n.DisplayName);
        }
    }

    private void ShowMessage(string message)
    {
        MessageBar.Text = message;
    }
}
