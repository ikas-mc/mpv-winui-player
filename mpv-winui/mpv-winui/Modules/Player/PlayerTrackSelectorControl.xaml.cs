using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace mpv_winui.Modules.Player
{
    public class TrackItem(int index, string label)
    {
        public string Label { get; set; } = label;
        public int Index { get; set; } = index;
    }

    public sealed partial class PlayerTrackSelectorControl : UserControl
    {
        public ObservableCollection<TrackItem> VideoItems { get; } = [];
        public ObservableCollection<TrackItem> SubtitleItems { get; } = [];
        public ObservableCollection<TrackItem> AudioItems { get; } = [];
        public ObservableCollection<TrackItem> SecondSubItems { get; } = [];

        public event EventHandler<int>? VideoTrackSelected;
        public event EventHandler<int>? SubtitleTrackSelected;
        public event EventHandler<int>? AudioTrackSelected;
        public event EventHandler<int>? SecondSubTrackSelected;

        public PlayerTrackSelectorControl()
        {
            InitializeComponent();
        }

        public void LoadVideoTracks(IList<IPlayerTrackItem> tracks)
        {
            VideoItems.Clear();
            var selectedIndex = -1;
            if (tracks?.Count > 0)
            {
                for (var i = 0; i < tracks.Count; i++)
                {
                    var track = tracks[i];
                    if (track.Checked)
                    {
                        selectedIndex = i;
                    }

                    VideoItems.Add(new TrackItem(track.Id, track.Label));
                }
            }

            VideoListView.SelectedIndex = selectedIndex;
        }

        public void LoadSubtitleTracks(IList<IPlayerTrackItem> tracks, string offLabel)
        {
            SubtitleItems.Clear();
            SubtitleItems.Add(new TrackItem(-1, offLabel));
            var selectedIndex = 0;
            if (tracks?.Count > 0)
            {
                for (var i = 0; i < tracks.Count; i++)
                {
                    var track = tracks[i];
                    if (track.Checked)
                    {
                        selectedIndex = i + 1;
                    }

                    SubtitleItems.Add(new TrackItem(track.Id, track.Label));
                }
            }

            SubtitleListView.SelectedIndex = selectedIndex;
        }

        public void LoadAudioTracks(IList<IPlayerTrackItem> tracks)
        {
            AudioItems.Clear();
            var selectedIndex = -1;
            if (tracks?.Count > 0)
            {
                for (var i = 0; i < tracks.Count; i++)
                {
                    var track = tracks[i];
                    if (track.Checked)
                    {
                        selectedIndex = i;
                    }

                    AudioItems.Add(new TrackItem(track.Id, track.Label));
                }
            }

            AudioListView.SelectedIndex = selectedIndex;
        }

        public void LoadSecondSubtitleTracks(IList<IPlayerTrackItem> tracks, string offLabel)
        {
            SecondSubItems.Clear();
            SecondSubItems.Add(new TrackItem(-1, offLabel));

            var selectedIndex = 0;
            if (tracks?.Count > 0)
            {
                for (var i = 0; i < tracks.Count; i++)
                {
                    var track = tracks[i];
                    if (track.Checked)
                    {
                        selectedIndex = i + 1;
                    }

                    SecondSubItems.Add(new TrackItem(track.Id, track.Label));
                }
            }

            SecondSubListView.SelectedIndex = selectedIndex;
        }

        public void SetSecondSubVisibility(bool visible)
        {
            if (visible)
            {
                VisualStateManager.GoToState(this, "NormalState", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "SecondSubState", false);
            }
        }

        private void VideoListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is TrackItem item)
            {
                VideoTrackSelected?.Invoke(this, item.Index);
            }
        }

        private void SubtitleListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is TrackItem item)
            {
                SubtitleTrackSelected?.Invoke(this, item.Index);
            }
        }

        private void AudioListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is TrackItem item)
            {
                AudioTrackSelected?.Invoke(this, item.Index);
            }
        }

        private void SecondSubListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is TrackItem item)
            {
                SecondSubTrackSelected?.Invoke(this, item.Index);
            }
        }
    }
}