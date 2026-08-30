using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using mpv_winui.Modules.About;
using mpv_winui.Modules.FileSystem;
using mpv_winui.Modules.MediaInfo;
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
                        case "open-dvda":
                            await OpenDvdaAsync();
                            break;
                        case "open-cdda":
                            await OpenCddaAsync();
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
                        case "open-mpv-conf":
                            await OpenConfigFileAsync("mpv.conf", true);
                            break;
                        case "open-mpvw-conf":
                            await OpenConfigFileAsync("mpvw.conf", true);
                            break;
                        case "open-input-conf":
                            await OpenConfigFileAsync("input.conf", true);
                            break;
                        case "open-menu-conf":
                            await OpenConfigFileAsync("menu.conf", true);
                            break;
                        case "open-mpv-log":
                            await OpenConfigFileAsync("mpv.log", true);
                            break;
                        case "open-menu-json":
                            await OpenConfigFileAsync("menu.json", false);
                            break;
                        case "link-mpv-wiki":
                            await Launcher.LaunchUriAsync(new Uri("https://github.com/mpv-player/mpv/wiki"));
                            break;
                        case "link-mpv-manual-stable":
                            await Launcher.LaunchUriAsync(new Uri("https://mpv.io/manual/stable/"));
                            break;
                        case "link-mpv-manual":
                            await Launcher.LaunchUriAsync(new Uri("https://mpv.io/manual/master/"));
                            break;
                        case "link-mpvw-wiki":
                            await Launcher.LaunchUriAsync(new Uri("https://github.com/ikas-mc/mpv-winui-player/wiki"));
                            break;
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
                            AppInstance.Restart(string.Empty);
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
                        case "conf-edit":
                            ShowMpvConfigWindow();
                            break;
                        case "media-info":
                            await ShowMediaInfoDialogAsync();
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

        private void ShowMpvConfigWindow()
        {
            if (App.Window is MainWindow window)
            {
                window.OpenMpvConfigWindow();
            }
        }

        private async Task OpenConfigFileAsync(string fileName, bool inMpvFolder)
        {
            var folder = inMpvFolder
                ? await AppData.Current.OpenOrCreateLocalDataFolderAsync(MpvConfigFolderName)
                : await AppData.Current.OpenLocalDataFolderAsync();
            var file = await folder.CreateFileAsync(fileName, Windows.Storage.CreationCollisionOption.OpenIfExists);
            await Launcher.LaunchFileAsync(file);
        }

        private async Task ShowAboutDialogAsync()
        {
            var dialog = new ContentDialog
            {
                Title = AppContext.AppLang.About,
                Content = AboutControl.Create(_mediaPlayer?.GetVersion()),
                CloseButtonText = AppContext.AppLang.Close,
                XamlRoot = XamlRoot
            };
            await dialog.ShowAsync();
        }

        private async Task ShowMediaInfoDialogAsync()
        {
            var dialog = new ContentDialog
            {
                Title = AppContext.AppLang.MediaInfo,
                Content = new MediaInfoUserControl(_mediaPlayer?.GetCurrentPath())
                {
                    MinWidth = 400
                },
                CloseButtonText = AppContext.AppLang.Close,
                XamlRoot = XamlRoot,
                Padding = new Thickness(0),
            };
            dialog.Resources["ContentDialogMaxWidth"] = 900;
            dialog.Resources["ContentDialogPadding"] = new Thickness(12);
            await dialog.ShowAsync();
        }
    }
}