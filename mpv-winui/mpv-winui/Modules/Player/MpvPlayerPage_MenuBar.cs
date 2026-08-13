using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using mpv_winui.Modules.AppModel;
using mpv_winui.Modules.FileSystem;
using mpv_winui.Modules.Player.Menu;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.System;
using AppInstance = Microsoft.Windows.AppLifecycle.AppInstance;

namespace mpv_winui.Modules.Player
{
    public sealed partial class MpvPlayerPage
    {
        private async void SetupCustomMenuBarItems()
        {
            List<CustomMenuItem>? menuItems = null;
            try
            {
                menuItems = await MenuService.Instance.TryLoadAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Custom menu bar load error");
            }

            if (menuItems is null || menuItems.Count == 0 || MainMenuBar.Items.Count < 3)
            {
                return;
            }

            var insertIndex = MainMenuBar.Items.Count - 1;
            foreach (var menuItem in menuItems)
            {
                if (menuItem.Children?.Count > 0)
                {
                    var menuBarItem = new MenuBarItem
                    {
                        Title = menuItem.Name ?? string.Empty,
                        IsTabStop = false
                    };
                    AddCustomMenuDataItems(menuBarItem.Items, menuItem.Children);
                    MainMenuBar.Items.Insert(insertIndex++, menuBarItem);
                }
            }
        }

        private void AddCustomMenuDataItems(IList<MenuFlyoutItemBase> target, IReadOnlyList<CustomMenuItem>? items)
        {
            if (items == null)
            {
                return;
            }

            foreach (var entry in items)
            {
                if (entry.Children?.Count > 0)
                {
                    var subItem = new MenuFlyoutSubItem
                    {
                        Text = entry.Name ?? string.Empty
                    };
                    AddCustomMenuDataItems(subItem.Items, entry.Children);
                    if (subItem.Items.Count > 0)
                    {
                        target.Add(subItem);
                    }
                    continue;
                }

                var item = new MenuFlyoutItem
                {
                    Text = entry.Name ?? string.Empty,
                    Tag = entry
                };

                item.Click += CustomMenuItem_Click;
                target.Add(item);
            }
        }

        private async void CustomMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem { Tag: CustomMenuItem data })
            {
                try
                {
                    if (data.Command?.Count > 0)
                    {
                        await _mediaPlayer.RunCommandAsync(data.Command);
                    }
                    else if (!string.IsNullOrEmpty(data.CommandString))
                    {
                        await _mediaPlayer.RunCommandAsync(data.CommandString);
                    }
                }
                catch (Exception ex)
                {
                    OnException(ex);
                }
            }
        }

        private async void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is MenuFlyoutItem { Tag: string tag })
                {
                    switch (tag)
                    {
                        case "open":
                            await OpenFileAsync();
                            break;
                        case "open-folder":
                            await OpenFolderAsync();
                            break;
                        case "open-url":
                            await OpenUrlAsync();
                            break;
                        case "open-clipboard":
                            await OpenClipboardAsync();
                            break;
                        case "open-dvd":
                            await OpenDvdAsync();
                            break;
                        case "open-bd":
                            await OpenBdAsync();
                            break;
                        case "load-subtitle":
                            await LoadSubtitleAsync();
                            break;
                        case "screenshot":
                            await _mediaPlayer.RunCommandAsync(["screenshot"]);
                            break;
                        case "screenshot-no-sub":
                            await _mediaPlayer.RunCommandAsync(["screenshot", "video"]);
                            break;
                        case "conf-folder":
                        {
                            var storageFolder = await AppData.Current.OpenLocalDataFolderAsync();
                            await Launcher.LaunchFolderAsync(storageFolder);
                            break;
                        }
                        case "mpv-folder":
                        {
                            var storageFolder = await AppData.Current.OpenOrCreateLocalDataFolderAsync(MpvConfigFolderName);
                            await Launcher.LaunchFolderAsync(storageFolder);
                            break;
                        }
                        case "playlist":
                        {
                            TogglePlaylist(true);
                            break;
                        }
                        case "open-watch-history":
                        {
                            await ShowWatchHistoryDialogAsync();
                            break;
                        }
                        case "open-watch-later":
                        {
                            await ShowWatchLaterDialogAsync();
                            break;
                        }
                        case "restart":
                        {
                            if (App.Window is MainWindow mainWindow)
                            {
                                mainWindow.SaveWindowPositionAndSize();
                            }
                            AppInstance.Restart("Reset");
                            break;
                        }
                        case "about":
                            await ShowAboutDialogAsync();
                            break;
                        case "quit":
                            AppQuit();
                            break;
                        case "fullwindow":
                            PlayerControl.ToggleFullWindow();
                            break;
                        case "fullscreen":
                            PlayerControl.ToggleFullScreen();
                            break;
                        case "ontop":
                            ToggleAlwaysOnTop();
                            break;
                        case "options":
                            ShowSettingsWindow();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                OnException(ex);
            }
        }

        private void ShowSettingsWindow()
        {
            if (App.Window is MainWindow window)
            {
                window.OpenSettingWindow();
            }
        }

        private async Task ShowAboutDialogAsync()
        {
            var stack = new StackPanel { Spacing = 12, MinWidth = 400 };

            stack.Children.Add(new TextBlock
            {
                Text = PackageHelper.AppName,
                FontSize = 20,
                FontWeight = new Windows.UI.Text.FontWeight(600)
            });

            stack.Children.Add(new TextBlock
            {
                Text = PackageHelper.AppVersion,
                TextWrapping = TextWrapping.Wrap
            });

            stack.Children.Add(new TextBlock
            {
                Text = "mpv",
                TextWrapping = TextWrapping.Wrap
            });
            var mpvLink = new HyperlinkButton
            {
                Content = "github.com/mpv-player/mpv",
                NavigateUri = new Uri("https://github.com/mpv-player/mpv"),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            stack.Children.Add(mpvLink);

            stack.Children.Add(new TextBlock
            {
                Text = "mpv-winui-player (mpvw)",
                TextWrapping = TextWrapping.Wrap
            });
            var projectLink = new HyperlinkButton
            {
                Content = "github.com/ikas-mc/mpv-winui-player",
                NavigateUri = new Uri("https://github.com/ikas-mc/mpv-winui-player"),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            stack.Children.Add(projectLink);

            var dialog = new ContentDialog
            {
                Title = "About",
                Content = stack,
                CloseButtonText = "Close",
                XamlRoot = XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}