using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using mpv_winrt;
using mpv_winui.Modules.Common.Utils;
using mpv_winui.Modules.FileSystem;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace mpv_winui.Modules.Player
{
    public struct PlaylistItem(MpvPlaylistItem item)
    {
        public readonly int Index => item.Index;
        public readonly int Id => item.Id;
        public readonly bool IsCurrent => item.IsCurrent || item.IsPlaying;
        public readonly string Title => item.Title;
        public readonly string Path => item.Filename;
        public readonly string Filename => System.IO.Path.GetFileName(item.Filename);
    };

    public sealed partial class MpvPlayerPage
    {
        public ObservableCollection<PlaylistItem> PlaylistItems { get; } = [];

        private void RefreshPlaylistAsync()
        {
            GetPlaylistAsync().FireAndForget(OnException);
        }

        private async Task GetPlaylistAsync()
        {
            var items = await Task.Run(() => _mediaPlayer.GetPlaylist().Select(x => new PlaylistItem(x)).ToList());
            PlaylistItems.Clear();
            if (items?.Count > 0)
            {
                foreach (var item in items)
                {
                    PlaylistItems.Add(item);
                }
            }

            SelectCurrentPlayListItem();
        }

        private void SelectCurrentPlayListItem()
        {
            for (int i = 0; i < PlaylistItems.Count; i++)
            {
                if (PlaylistItems[i].IsCurrent)
                {
                    PlaylistView.SelectedIndex = i;
                    PlaylistView.ScrollIntoView(PlaylistItems[i], ScrollIntoViewAlignment.Leading);
                    break;
                }
            }
        }

        private void ClosePlaylist_Click(object sender, RoutedEventArgs e)
        {
            TogglePlaylist();
        }

        private async void RefreshPlaylist_Click(object sender, RoutedEventArgs e)
        {
            RefreshPlaylistAsync();
        }

        private void PlaylistView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is PlaylistItem playlistItem)
            {
                _mediaPlayer.PlaylistPlayIndex(playlistItem.Index);
            }
        }

        private bool NeedUpdatePlaylist()
        {
            return PlaylistContainer.Visibility == Visibility.Visible;
        }

        private void MpvPlayerPage_PlaylistChanged()
        {
            DispatcherQueue.RunAsync(() =>
            {
                if (NeedUpdatePlaylist())
                {
                    RefreshPlaylistAsync();
                }
            });
        }

        private void TogglePlaylist(bool refresh = false)
        {
            if (PlaylistContainer.Visibility == Visibility.Collapsed)
            {
                VisualStateManager.GoToState(this, "ShowPlaylist", true);
                if (refresh)
                {
                    RefreshPlaylistAsync();
                }
            }
            else
            {
                VisualStateManager.GoToState(this, "HidePlaylist", true);
            }
        }

        private void PlaylistView_ContextRequested(UIElement sender, ContextRequestedEventArgs args)
        {
            if (args.OriginalSource is FrameworkElement fe && fe.DataContext is PlaylistItem item)
            {
                PlaylistView.SelectedItem = item;
                var count = PlaylistItems.Count;
                var index = item.Index;

                if (Resources["PlaylistContextMenu"] is MenuFlyout flyout)
                {
                    foreach (var flyoutItem in flyout.Items)
                    {
                        if (flyoutItem is MenuFlyoutItem menuItem && menuItem.Tag is string tag)
                        {
                            menuItem.IsEnabled = tag switch
                            {
                                "move-up" or "move-top" => index > 0,
                                "move-down" or "move-bottom" => index < count - 1,
                                _ => true,
                            };
                        }
                    }

                    if (args.TryGetPosition(PlaylistView, out var point))
                    {
                        flyout.ShowAt(PlaylistView, point);
                    }
                    else
                    {
                        flyout.ShowAt(PlaylistView);
                    }
                    args.Handled = true;
                }
            }
        }

        private void PlaylistMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem { Tag: string action } && PlaylistView.SelectedItem is PlaylistItem item)
            {
                try
                {
                    switch (action)
                    {
                        case "play":
                            _mediaPlayer.PlaylistPlayIndex(item.Index);
                            break;
                        case "move-up":
                            MovePlaylistItem(item.Index, item.Index - 1);
                            break;
                        case "move-down":
                            MovePlaylistItem(item.Index, item.Index + 1);
                            break;
                        case "move-top":
                            MovePlaylistItem(item.Index, 0);
                            break;
                        case "move-bottom":
                            MovePlaylistItem(item.Index, PlaylistItems.Count - 1);
                            break;
                        case "remove":
                            _mediaPlayer.PlaylistRemove(item.Index);
                            RefreshPlaylistAsync();
                            break;
                        case "copy-title":
                            ClipboardHelper.SetCopyText(item.Title);
                            break;
                        case "copy-path":
                            ClipboardHelper.SetCopyText(item.Path);
                            break;
                        case "open-location":
                            _ = FileLauncher.ShellLaunchFileAsync(item.Path);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    OnException(ex);
                }
            }
        }

        private void MovePlaylistItem(int from, int to)
        {
            _mediaPlayer.PlaylistMove(from, to);
            RefreshPlaylistAsync();
        }
    }
}