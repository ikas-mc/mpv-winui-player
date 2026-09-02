using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;

namespace mpv_winui.Modules.Menu.MenuEditor;

public sealed partial class MenuEditorPage
{
    private MenuFlyout? _previewFlyout;

    private void RebuildPreview()
    {
        if (_type == MenuType.Menubar)
        {
            RebuildMenuBarPreview();
        }
        else
        {
            RebuildFlyoutPreview();
        }
    }

    private void RebuildMenuBarPreview()
    {
        PreviewBar.Items.Clear();
        foreach (var root in Nodes)
        {
            if (root.Kind != MenuTreeItemKind.Menu || root.Children.Count == 0)
            {
                continue;
            }

            var barItem = new MenuBarItem
            {
                Title = root.DisplayName,
                IsTabStop = false,
            };
            AddMenuItems(MenuType.Menubar, barItem.Items, root.Children);
            PreviewBar.Items.Add(barItem);
        }
    }

    private void RebuildFlyoutPreview()
    {
        var flyout = new MenuFlyout();
        AddMenuItems(MenuType.ContextMenu, flyout.Items, Nodes);
        _previewFlyout = flyout;
    }

    private void PreviewButton_Click(object sender, RoutedEventArgs e)
    {
        RebuildPreview();
        ShowMessage($"Preview updated from {Nodes.Count} root node{(Nodes.Count == 1 ? "" : "s")}.");
    }

    private void PreviewMenuButton_Click(object sender, RoutedEventArgs e)
    {
        RebuildFlyoutPreview();
        _previewFlyout!.ShowAt(PreviewMenuButton);
    }

    private static void AddMenuItems(MenuType menuType, IList<MenuFlyoutItemBase> target, IReadOnlyList<MenuTreeItem> nodes)
    {
        foreach (var node in nodes)
        {
            if (node.Kind == MenuTreeItemKind.Separator)
            {
                target.Add(new MenuFlyoutSeparator());
                continue;
            }

            if (node.Kind == MenuTreeItemKind.Menu)
            {
                var sub = new MenuFlyoutSubItem { Text = node.DisplayName };
                AddMenuItems(menuType, sub.Items, node.Children);
                if (sub.Items.Count > 0)
                {
                    target.Add(sub);
                }

                continue;
            }

            if (menuType == MenuType.ContextMenu)
            {
                if (string.IsNullOrEmpty(node.Command))
                {
                    continue;
                }
            }

            target.Add(new MenuFlyoutItem { Text = node.DisplayName });
        }
    }
}