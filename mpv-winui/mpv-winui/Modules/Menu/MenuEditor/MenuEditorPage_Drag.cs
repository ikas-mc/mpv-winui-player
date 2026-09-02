using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Linq;

namespace mpv_winui.Modules.Menu.MenuEditor;

public sealed partial class MenuEditorPage
{
    private MenuTreeItem? _dragged;

    private void MenuTree_DragItemsStarting(TreeView sender, TreeViewDragItemsStartingEventArgs args)
    {
        _dragged = args.Items.FirstOrDefault() as MenuTreeItem;
    }

    private void MenuTree_DragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Move;
    }

    private void MenuTree_Drop(object sender, DragEventArgs e)
    {
        if (_dragged is null || e.OriginalSource is not FrameworkElement { DataContext: MenuTreeItem target })
        {
            return;
        }

        if (ReferenceEquals(_dragged, target) || IsDescendant(target, _dragged))
        {
            return;
        }

        if (ReferenceEquals(_dragged.Parent, target.Parent))
        {
            var items = GetSiblings(target);
            var targetIndex = items.IndexOf(target);
            items.Remove(_dragged);
            items.Insert(targetIndex > items.Count ? items.Count : targetIndex, _dragged);
        }
        else
        {
            RemoveNode(_dragged);
            _dragged.Parent = target;
            target.Children.Add(_dragged);
            target.Kind = MenuTreeItemKind.Menu;
            target.IsSeparator = false;
        }

        SelectNode(_dragged);
        _dragged = null;
    }

    private static bool IsDescendant(MenuTreeItem node, MenuTreeItem ancestor)
    {
        for (var current = node.Parent; current is not null; current = current.Parent)
        {
            if (ReferenceEquals(current, ancestor))
            {
                return true;
            }
        }

        return false;
    }
}