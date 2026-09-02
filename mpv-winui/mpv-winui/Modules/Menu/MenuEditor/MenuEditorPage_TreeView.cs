using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;

namespace mpv_winui.Modules.Menu.MenuEditor;

public sealed partial class MenuEditorPage
{
    private void AddRootButton_Click(object sender, RoutedEventArgs e)
    {
        var node = new MenuTreeItem
        {
            Name = "New menu",
            Kind = MenuTreeItemKind.Menu,
        };
        Nodes.Add(node);
        SelectNode(node);
    }


    private void MenuTree_SelectionChanged(TreeView sender, TreeViewSelectionChangedEventArgs args)
    {
        SelectNode(sender.SelectedNode?.Content as MenuTreeItem);
    }

    private void MenuTree_ContextRequested(UIElement sender, ContextRequestedEventArgs args)
    {
        if (args.OriginalSource is not FrameworkElement { DataContext: MenuTreeItem node })
        {
            return;
        }

        MenuTree.SelectedItem = node;

        if (Resources["TreeContextMenu"] is MenuFlyout flyout)
        {
            foreach (var flyoutItem in flyout.Items)
            {
                if (flyoutItem is MenuFlyoutItem menuItem && menuItem.Tag is string tag)
                {
                    menuItem.IsEnabled = tag switch
                    {
                        "delete-children" => node.Children.Count > 0,
                        "execute" => node.Kind == MenuTreeItemKind.CmdMenu,
                        _ => true,
                    };
                }
            }

            if (args.TryGetPosition(MenuTree, out var point))
            {
                flyout.ShowAt(MenuTree, point);
            }
            else
            {
                flyout.ShowAt(MenuTree);
            }

            args.Handled = true;
        }
    }

    private void TreeContextMenu_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem { Tag: string action } || MenuTree.SelectedItem is not MenuTreeItem node)
        {
            return;
        }

        switch (action)
        {
            case "edit":
                EditNode(node);
                break;
            case "execute":
                ExecuteNode(node);
                break;
            case "delete":
                DeleteNode(node);
                break;
            case "delete-children":
                DeleteChildren(node);
                break;
            case "add-child":
                AddChild(node);
                break;
        }
    }

    private void EditNode(MenuTreeItem node)
    {
        NameBox.Focus(FocusState.Programmatic);
        NameBox.SelectAll();
    }

    private void DeleteNode(MenuTreeItem node)
    {
        RemoveNode(node);
        SelectNode(null);
    }

    private void ExecuteNode(MenuTreeItem node)
    {
        string command = node.Command;
        if (string.IsNullOrEmpty(command))
        {
            ShowMessage($"No command to execute for '{node.Name}'.");
            return;
        }

        try
        {
            if (App.Window is MainWindow window)
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug("menu command executed on player, command={}", command);
                }

                window.RunMpvCommand(command);
                ShowMessage($"Executed: {command}");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "execute menu command on player failed, command={}", command);
            ShowMessage($"Execute failed: {ex.Message}");
        }
    }

    private void DeleteChildren(MenuTreeItem node)
    {
        node.Children.Clear();
        SelectNode(node);
    }

    private void AddChild(MenuTreeItem node)
    {
        var child = new MenuTreeItem { Name = "New item", Parent = node };
        node.Children.Add(child);
        node.Kind = MenuTreeItemKind.Menu;
        SelectNode(child);
    }

    private void RemoveNode(MenuTreeItem node)
    {
        GetSiblings(node).Remove(node);
        node.Parent = null;
    }

    private IList<MenuTreeItem> GetSiblings(MenuTreeItem node)
    {
        return node.Parent?.Children ?? Nodes;
    }

}