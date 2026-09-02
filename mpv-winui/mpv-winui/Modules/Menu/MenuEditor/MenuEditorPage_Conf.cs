using Microsoft.UI.Xaml;
using mpv_winui.Modules.Menu.MpvMenu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mpv_winui.Modules.Menu.MenuEditor;

public sealed partial class MenuEditorPage
{
    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        SaveButton.IsEnabled = false;
        try
        {
            await SaveAsync();
        }
        catch (Exception ex)
        {
            _logger.Debug(ex, "Save failed, file={}", _filePath);
            ShowMessage($"Save failed: {ex.Message}");
        }
        finally
        {
            SaveButton.IsEnabled = true;
        }
    }

    private async void ReloadButton_Click(object sender, RoutedEventArgs e)
    {
        ReloadButton.IsEnabled = false;
        try
        {
            await LoadAsync();
        }
        catch (Exception ex)
        {
            _logger.Debug(ex, "Load failed, file={}", _filePath);
            ShowMessage($"Load failed: {ex.Message}");
        }
        finally
        {
            ReloadButton.IsEnabled = true;
        }
    }

    private async Task LoadAsync()
    {
        if (_logger.IsDebugEnabled)
        {
            _logger.Debug("Load conf, file={}", _filePath);
        }

        var items = await Task.Run(() => MenuConfParser.Parse(_filePath));

        Nodes.Clear();
        if (items is not null)
        {
            foreach (var item in items)
            {
                Nodes.Add(CreateMenuTreeItem(item, null));
            }
        }

        SelectNode(null);
        RebuildPreview();
        ShowMessage($"{(_filePath.Length == 0 ? "Open" : "Loaded")} {_filePath}");
    }

    private async Task SaveAsync()
    {
        if (_logger.IsDebugEnabled)
        {
            _logger.Debug("Save conf, file={}", _filePath);
        }

        if (!CheckName(Nodes))
        {
            ShowMessage("Cannot save: items with an empty name exist.");
            return;
        }

        var items = Nodes.Select(CreateMpvMenuItem).ToList();
        await Task.Run(() => MenuConfWriter.Save(_filePath, items));
        ShowMessage($"Saved {_filePath}");
    }

    private static bool CheckName(IEnumerable<MenuTreeItem> nodes)
    {
        foreach (var node in nodes)
        {
            if (node.Kind != MenuTreeItemKind.Separator && string.IsNullOrWhiteSpace(node.Name))
            {
                return false;
            }

            if (!CheckName(node.Children))
            {
                return false;
            }
        }

        return true;
    }

    private static MenuTreeItem CreateMenuTreeItem(MpvMenuItem item, MenuTreeItem? parent)
    {
        var node = new MenuTreeItem
        {
            Parent = parent,
            Name = item.Name ?? string.Empty,
            Command = item.CommandString ?? string.Empty,
            Hidden = item.Hidden,
            Disabled = item.Disabled,
            Checked = item.Checked,
            IsSeparator = item.IsSeparator,
        };

        if (item.IsSeparator)
        {
            node.Kind = MenuTreeItemKind.Separator;
        }
        else if (item.Children is { Count: > 0 } children)
        {
            node.Kind = MenuTreeItemKind.Menu;
            foreach (var child in children)
            {
                node.Children.Add(CreateMenuTreeItem(child, node));
            }
        }
        else
        {
            node.Kind = MenuTreeItemKind.CmdMenu;
        }

        return node;
    }

    private MpvMenuItem CreateMpvMenuItem(MenuTreeItem node)
    {
        var item = new MpvMenuItem();

        switch (node.Kind)
        {
            case MenuTreeItemKind.Separator:
            {
                item.IsSeparator = true;
                break;
            }
            case MenuTreeItemKind.Menu:
            {
                item.Name = node.Name;
                if (node.Children?.Count > 0)
                {
                    item.Children = node.Children.Select(CreateMpvMenuItem).ToList();
                }
                break;
            }
            case MenuTreeItemKind.CmdMenu:
            {
                item.Name = node.Name;
                item.CommandString = string.IsNullOrWhiteSpace(node.Command) ? null : node.Command;

                if (_type == MenuType.ContextMenu)
                {
                    item.Hidden = string.IsNullOrWhiteSpace(node.Hidden) ? null : node.Hidden;
                    item.Disabled = string.IsNullOrWhiteSpace(node.Disabled) ? null : node.Disabled;
                    item.Checked = string.IsNullOrWhiteSpace(node.Checked) ? null : node.Checked;
                }
                break;
            }
        }

        return item;
    }

}