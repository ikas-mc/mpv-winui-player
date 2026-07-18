using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using mpv_winrt;
using mpv_winui.Modules.Common.Utils;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace mpv_winui.Modules.Player
{
    public struct PlaylistItem(MpvPlaylistItem item, int index)
    {
        public int Index { get; set; } = index;

        //TODO add option
        public readonly string? Title => string.IsNullOrEmpty(item.Title) ? Path.GetFileName(item.Filename) : item.Title;
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
            PlaylistItems.Clear();
            var items = await Task.Run(_mediaPlayer.Playlist);
            var selectedIndex = -1;
            if (items?.Count > 0)
            {
                for (var i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    if (item.IsCurrent || item.IsPlaying)
                    {
                        selectedIndex = i;
                    }
                    PlaylistItems.Add(new PlaylistItem(item, i));
                }
            }
            PlaylistView.SelectedIndex = selectedIndex;
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
                _mediaPlayer.RunCommandAsync(["playlist-play-index", playlistItem.Index.ToString()]).FireAndForget(OnException);
            }
        }

        private bool NeedUpdatePlaylist()
        {
            return PlaylistContainer.Visibility == Visibility.Visible;
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
    }
}