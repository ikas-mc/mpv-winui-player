using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using mpv_winrt;
using mpv_winui.Modules.Common.Utils;
using NLog.Targets;
using System.Collections.Generic;

namespace mpv_winui.Modules.Player
{
    public sealed partial class MpvPlayerPage
    {
        private MenuFlyout BuildMenuFlyoutFromData(IReadOnlyList<MpvMenuItem>? items)
        {
            var flyout = new MenuFlyout();
            if (items != null)
            {
                AddMenuDataItems(flyout.Items, items);
            }

            var item = new MenuFlyoutItem
            {
                Text = "Toggle Playlist",
                Tag = "playlist"
            };
            item.Click += Item_Click;
            flyout.Items.Add(item);

            var subItem = new MenuFlyoutSubItem
            {
                Text = "Window",
                MinWidth = 200 //TODO 
            };
            flyout.Items.Add(subItem);

            //TODO 
            item = new MenuFlyoutItem
            {
                Text = "Toggle Full Screen",
                Tag = "fullscreen"
            };
            item.Click += Item_Click;
            subItem.Items.Add(item);

            item = new MenuFlyoutItem
            {
                Text = "Toggle Full Window",
                Tag = "fullwindow"
            };
            item.Click += Item_Click;
            subItem.Items.Add(item);

            item = new MenuFlyoutItem
            {
                Text = "Quit",
                Tag = "quit"
            };
            item.Click += Item_Click;
            flyout.Items.Add(item);

            return flyout;
        }

        private void AddMenuDataItems(IList<MenuFlyoutItemBase> target, IReadOnlyList<MpvMenuItem> items)
        {

            foreach (var entry in items)
            {
                //TODO
                if (!CheckMpvMenu(entry))
                {
                    continue;
                }

                if (entry.Type == "separator")
                {
                    target.Add(new MenuFlyoutSeparator());
                    continue;
                }

                if (entry.Type == "submenu" && entry.Items.Count > 0)
                {
                    var subItem = new MenuFlyoutSubItem { Text = entry.Title?.Replace("&", "") ?? string.Empty, IsEnabled = !entry.IsDisabled };
                    AddMenuDataItems(subItem.Items, entry.Items);
                    if (subItem.Items.Count > 0)
                    {
                        target.Add(subItem);
                    }
                }
                else if (!string.IsNullOrEmpty(entry.Command))
                {
                    var cmd = entry.Command;
                    MenuFlyoutItem item;
                    if (entry.IsChecked)
                    {
                        item = new ToggleMenuFlyoutItem { Text = entry.Title?.Replace("&", "") ?? string.Empty, IsEnabled = !entry.IsDisabled, IsChecked = true, };
                    }
                    else
                    {
                        item = new MenuFlyoutItem { Text = entry.Title?.Replace("&", "") ?? string.Empty, IsEnabled = !entry.IsDisabled, };
                    }

                    item.Click += (_, _) => MpvMenuItemClick(cmd);
                    target.Add(item);
                }
            }
        }

        private void Item_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem { Tag: string tag })
            {
                switch (tag)
                {
                    case "quit":
                    {
                        AppQuit();
                        break;
                    }
                    case "fullscreen":
                    {
                        PlayerControl.ToggleFullScreen();
                        break;
                    }
                    case "fullwindow":
                    {
                        PlayerControl.ToggleFullWindow();
                        break;
                    }
                    case "playlist":
                    {
                        TogglePlaylist(true);
                        break;
                    }
                }
            }
        }

        private bool CheckMpvMenu(MpvMenuItem mpvMenuItem)
        {
            //TODO remove&  check cmd ??
            if (mpvMenuItem.Title == "&Stop" || mpvMenuItem.Title == "&Quit" || mpvMenuItem.Title == "Quit an&d save position")
            {
                return true;
            }

            return true;
        }

        private void MpvMenuItemClick(string cmd)
        {
            if (_logger.IsDebugEnabled)
            {
                _logger.Debug("mpv menu item click, cmd={}", cmd);
            }

            var args = TokenizeCommand(cmd);
            if (_logger.IsDebugEnabled)
            {
                _logger.Debug("mpv menu item click, args={}", args);
            }

            _mediaPlayer?.RunCommandAsync(args).FireAndForget(OnException);
        }

        private List<string> TokenizeCommand(string cmd)
        {
            var args = new List<string>();
            var i = 0;
            while (i < cmd.Length)
            {
                if (char.IsWhiteSpace(cmd[i]))
                {
                    i++;
                    continue;
                }

                if (cmd[i] == '"')
                {
                    i++;
                    var end = cmd.IndexOf('"', i);
                    args.Add(end < 0 ? cmd[i..] : cmd[i..end]);
                    i = end < 0 ? cmd.Length : end + 1;
                }
                else
                {
                    var end = cmd.IndexOfAny([' ', '\t'], i);
                    args.Add(end < 0 ? cmd[i..] : cmd[i..end]);
                    i = end < 0 ? cmd.Length : end + 1;
                }
            }

            return args;
        }

        private void PlayerView_ContextRequested(UIElement sender, ContextRequestedEventArgs args)
        {
            var menuItems = _mediaPlayer.MenuData();
            if (menuItems.Count > 0)
            {
                var flyout = BuildMenuFlyoutFromData(menuItems);
                if (args.TryGetPosition(PlayerView, out var point))
                {
                    flyout.ShowAt(PlayerView, point);
                }
                else
                {
                    flyout.ShowAt(PlayerView);
                }

                args.Handled = true;
            }
        }
    }
}