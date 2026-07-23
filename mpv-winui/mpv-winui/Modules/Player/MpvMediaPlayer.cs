using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using mpv_winrt;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace mpv_winui.Modules.Player
{
    public interface IPlayerTrackItem
    {
        int Id
        {
            get;
        }
        string Label
        {
            get;
        }

        bool Checked
        {
            get;
        }
    }

    public record PlayerTrackItem(int Id, string Label, bool Checked) : IPlayerTrackItem
    {
    }

    public partial class MpvMediaPlayer
    {
        private readonly MpvPlayer _mpvPlayer;

        private readonly Lazy<HashSet<string>> _subtitleExtensions;

        private MpvPlayer GetMpvPlayer()
        {
            return _mpvPlayer;
        }

        public MpvMediaPlayer()
        {
            this._mpvPlayer = new MpvPlayer();
            _subtitleExtensions = new(() =>
            {
                var exts = _mpvPlayer.GetSubtitleExtensions();
                var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                if (!string.IsNullOrEmpty(exts))
                {
                    foreach (var ext in exts.Split(','))
                    {
                        set.Add(ext.Trim());
                    }
                }
                return set;
            });
        }

        public Action<MpvMediaPlayer, object?>? MediaOpened
        {
            get; set;
        }
        public Action<MpvMediaPlayer, string?>? MediaFailed
        {
            get; set;
        }
        public Action<MpvMediaPlayer, object?>? MediaEnded
        {
            get; set;
        }
        public Action<MpvMediaPlayer, bool>? PlaybackStateChanged
        {
            get; set;
        }
        public Action<MpvMediaPlayer, object?>? BufferingStarted
        {
            get; set;
        }
        public Action<MpvMediaPlayer, object?>? BufferingEnded
        {
            get; set;
        }
        public Action<MpvMediaPlayer, object?>? NaturalDurationChanged
        {
            get; set;
        }
        public Action<MpvMediaPlayer, int>? VolumeChangedChanged
        {
            get; set;
        }
        public Action<MpvMediaPlayer, RepeatState>? RepeatStateChanged
        {
            get; set;
        }
        public Action<MpvMediaPlayer, bool>? ShuffleEnabledChanged
        {
            get; set;
        }
        public Action<MpvMediaPlayer, object?>? Seeked
        {
            get; set;
        }

        public Action<MpvMediaPlayer, object?>? SwapChainChanged
        {
            get; set;
        }

        public Action<MpvMediaPlayer, WindowChangedEventArgs>? WindowChanged
        {
            get; set;
        }

        public Action<MpvMediaPlayer, object?>? SeekingStarted
        {
            get; set;
        }

        public Action<MpvMediaPlayer, MediaInfoChangedEventArgs>? MediaInfoChanged
        {
            get; set;
        }

        public double Volume
        {
            get => _mpvPlayer.Volume();
            set => _mpvPlayer.Volume(value);
        }

        public bool Playing => !_mpvPlayer.IsPaused();
        public double Duration => _mpvPlayer.Duration();

        public bool IsMuted
        {
            get => _mpvPlayer.IsMuted();
            set => _mpvPlayer.IsMuted(value);
        }

        public double Position
        {
            get => _mpvPlayer.Position();
            set
            {
                _mpvPlayer.Position(value);
                SeekingStarted?.Invoke(this, value);
            }
        }

        public TimeSpan PositionTimeSpan
        {
            get => TimeSpan.FromSeconds(_mpvPlayer.Position());
            set
            {
                _mpvPlayer.Position(value.TotalSeconds);
                SeekingStarted?.Invoke(this, value.TotalSeconds);
            }
        }

        public double PlaybackRate
        {
            get => _mpvPlayer.PlaybackSpeed();
            set => _mpvPlayer.PlaybackSpeed(value);
        }

        public int CurrentSubtitleTrack
        {
            get => _mpvPlayer.CurrentSubtitleTrack();
            set => _mpvPlayer.CurrentSubtitleTrack(value);
        }

        public int CurrentAudioTrack
        {
            get => _mpvPlayer.CurrentAudioTrack();
            set => _mpvPlayer.CurrentAudioTrack(value);
        }

        public int CurrentVideoTrack
        {
            get => _mpvPlayer.CurrentVideoTrack();
            set => _mpvPlayer.CurrentVideoTrack(value);
        }

        public int CurrentSecondSubtitleTrack
        {
            get => _mpvPlayer.CurrentSecondSubtitleTrack();
            set => _mpvPlayer.CurrentSecondSubtitleTrack(value);
        }

        public string AspectRatio
        {
            set => _mpvPlayer.SetAspectRatio(value);
        }

        public int CurrentChapter => _mpvPlayer.CurrentChapter();
        public int CurrentEdition => _mpvPlayer.CurrentEdition();

        public RepeatState RepeatState
        {
            get
            {
                var loopFile = _mpvPlayer.LoopFile();
                if (loopFile)
                {
                    return RepeatState.One;
                }

                var loopPlaylist = _mpvPlayer.LoopPlaylist();
                if (loopPlaylist)
                {
                    return RepeatState.All;
                }

                return RepeatState.None;
            }
            set
            {
                _mpvPlayer.LoopFile(value == RepeatState.One);
                _mpvPlayer.SetLoopPlaylist(value == RepeatState.All);
                //RepeatStateChanged?.Invoke(this, null);
            }
        }

        public bool ShuffleEnabled
        {
            get => _mpvPlayer?.Shuffle() ?? false;
            set
            {
                _mpvPlayer.SetShuffle(value);
                //ShuffleEnabledChanged?.Invoke(this, null);
            }
        }

        public async Task InitializeAsync(string configFolder, int volume, mpv_winrt.DisplayColorKind colorKind, int refreshRate)
        {
            _mpvPlayer.VoConfigured += MpvPlayer_VoConfigured;
            await Task.Run(() => { _mpvPlayer.Initialize(configFolder, 1, 1, volume, colorKind, refreshRate); });
        }

        public void UpdatePanel(object panel)
        {
            if (panel is SwapChainPanel scp)
            {
                _mpvPlayer.AttachSwapChain(scp);
            }
        }

        public void UpdatePanelScale(float scaleX, float scaleY)
        {
            _mpvPlayer.UpdateSwapChainScale(scaleX, scaleY);
        }

        public void UpdateDisplayColorInfo(mpv_winrt.DisplayColorKind colorKind)
        {
            _mpvPlayer.UpdateDisplayColorInfo(colorKind);
        }

        public void UpdateDisplayRefreshRate(uint refreshRate)
        {
            _mpvPlayer.UpdateDisplayRefreshRate((int)refreshRate);
        }

        public void StartListen()
        {
            _mpvPlayer.MediaLoaded += MpvPlayer_MediaLoaded;
            _mpvPlayer.PlaybackEnded += MpvPlayer_PlaybackEnded;
            _mpvPlayer.PlaybackFailed += MpvPlayer_PlaybackFailed;
            _mpvPlayer.FileLoaded += MpvPlayer_FileLoaded;
            _mpvPlayer.PlaybackStateChanged += MpvPlayer_PlaybackStateChanged;
            _mpvPlayer.PositionChanged += MpvPlayer_PositionChanged;
            _mpvPlayer.SpeedChanged += MpvPlayer_SpeedChanged;
            _mpvPlayer.VolumeChanged += MpvPlayer_VolumeChanged;
            _mpvPlayer.Seeked += MpvPlayer_Seeked;
            _mpvPlayer.MediaInfoChanged += MpvPlayer_MediaInfoChanged;
            _mpvPlayer.LoopFileChanged += MpvPlayer_LoopFileChanged;
            _mpvPlayer.LoopPlaylistChanged += MpvPlayer_LoopPlaylistChanged;
            _mpvPlayer.ShuffleChanged += MpvPlayer_ShuffleChanged;
            _mpvPlayer.WindowChanged += MpvPlayer_WindowChanged;
        }

        public void StopListen()
        {
            _mpvPlayer.MediaLoaded -= MpvPlayer_MediaLoaded;
            _mpvPlayer.PlaybackEnded -= MpvPlayer_PlaybackEnded;
            _mpvPlayer.PlaybackFailed -= MpvPlayer_PlaybackFailed;
            _mpvPlayer.FileLoaded -= MpvPlayer_FileLoaded;
            _mpvPlayer.PlaybackStateChanged -= MpvPlayer_PlaybackStateChanged;
            _mpvPlayer.PositionChanged -= MpvPlayer_PositionChanged;
            _mpvPlayer.SpeedChanged -= MpvPlayer_SpeedChanged;
            _mpvPlayer.VolumeChanged -= MpvPlayer_VolumeChanged;
            _mpvPlayer.Seeked -= MpvPlayer_Seeked;
            _mpvPlayer.MediaInfoChanged -= MpvPlayer_MediaInfoChanged;
            _mpvPlayer.LoopFileChanged -= MpvPlayer_LoopFileChanged;
            _mpvPlayer.LoopPlaylistChanged -= MpvPlayer_LoopPlaylistChanged;
            _mpvPlayer.ShuffleChanged -= MpvPlayer_ShuffleChanged;
            _mpvPlayer.WindowChanged -= MpvPlayer_WindowChanged;
        }

        private void MpvPlayer_VoConfigured()
        {
            SwapChainChanged?.Invoke(this, null);
        }

        private void MpvPlayer_WindowChanged(WindowChangedEventArgs args)
        {
            WindowChanged?.Invoke(this, args);
        }

        private void MpvPlayer_MediaInfoChanged(MediaInfoChangedEventArgs args)
        {
            MediaInfoChanged?.Invoke(this, args);
        }

        private void MpvPlayer_LoopFileChanged()
        {
            RepeatStateChanged?.Invoke(this, RepeatState);
        }

        private void MpvPlayer_LoopPlaylistChanged()
        {
            RepeatStateChanged?.Invoke(this, RepeatState);
        }

        private void MpvPlayer_ShuffleChanged()
        {
            ShuffleEnabledChanged?.Invoke(this, ShuffleEnabled);
        }

        private void MpvPlayer_Seeked()
        {
            Seeked?.Invoke(this, null);
            BufferingStarted?.Invoke(this, null);
        }

        private void MpvPlayer_VolumeChanged(VolumeChangedEventArgs args)
        {
            VolumeChangedChanged?.Invoke(this, (int)args.Volume);
        }

        private void MpvPlayer_SpeedChanged(SpeedChangedEventArgs args)
        {
        }

        private void MpvPlayer_PositionChanged(PositionChangedEventArgs args)
        {
        }

        private void MpvPlayer_PlaybackStateChanged(PlaybackStateChangedEventArgs args)
        {
            PlaybackStateChanged?.Invoke(this, args.IsPaused);
        }

        private void MpvPlayer_FileLoaded()
        {
            var isPaused = _mpvPlayer.IsPaused();
            PlaybackStateChanged?.Invoke(this, isPaused);
            MediaOpened?.Invoke(this, null);
        }

        private void MpvPlayer_PlaybackEnded()
        {
            MediaEnded?.Invoke(this, null);
        }

        private void MpvPlayer_PlaybackFailed(PlaybackFailedEventArgs args)
        {
            MediaFailed?.Invoke(this, args.Message);
        }

        private void MpvPlayer_MediaLoaded()
        {
            BufferingEnded?.Invoke(this, null);
        }

        public void Pause() => _mpvPlayer.Pause();
        public void Play() => _mpvPlayer.Play();

        public void Stop() => _mpvPlayer.Stop();

        public void Command(IList<string> args)
        {
            if (args?.Count > 0)
            {
                _mpvPlayer.Command(args);
            }
        }

        public async ValueTask RunCommandAsync(IList<string> args)
        {
            if (args?.Count > 0)
            {
                await Task.Run(() => _mpvPlayer.Command(args));
            }
        }

        public void UpdateSize(uint width, uint height)
        {
            _mpvPlayer?.UpdateSize(width, height);
        }

        public IList<IPlayerTrackItem> SubtitleTracks()
        {
            var tracks = _mpvPlayer?.GetSubtitleTracks();
            if (!(tracks?.Count > 0))
            {
                return [];
            }

            var result = new List<IPlayerTrackItem>();
            for (var i = 0; i < tracks.Count; i++)
            {
                var track = tracks[i];
                result.Add(new PlayerTrackItem(i + 1, $"{i + 1} {track.Title} {track.Lang}", track.Selected));
            }

            return result;
        }

        public IList<IPlayerTrackItem> AudioTracks()
        {
            var tracks = _mpvPlayer?.GetAudioTracks();
            if (!(tracks?.Count > 0))
            {
                return [];
            }

            var result = new List<IPlayerTrackItem>();
            for (var i = 0; i < tracks.Count; i++)
            {
                var track = tracks[i];
                result.Add(new PlayerTrackItem(i + 1, $"{i + 1} {track.Title} {track.Codec}", track.Selected));
            }

            return result;
        }

        public IList<IPlayerTrackItem> VideoTracks()
        {
            var tracks = _mpvPlayer?.GetVideoTracks();
            if (!(tracks?.Count > 0))
            {
                return [];
            }

            var result = new List<IPlayerTrackItem>();
            for (var i = 0; i < tracks.Count; i++)
            {
                var track = tracks[i];
                result.Add(new PlayerTrackItem(i + 1, $"{i + 1} {track.Title} {track.Codec}", track.Selected));
            }

            return result;
        }

        public IList<IPlayerTrackItem> SecondSubtitleTracks()
        {
            var tracks = _mpvPlayer?.GetSubtitleTracks();
            if (!(tracks?.Count > 0))
            {
                return [];
            }

            var current = _mpvPlayer?.CurrentSecondSubtitleTrack();
            var result = new List<IPlayerTrackItem>();
            for (var i = 0; i < tracks.Count; i++)
            {
                var track = tracks[i];
                var id = i + 1;
                result.Add(new PlayerTrackItem(id, $"{id} {track.Title} {track.Lang}", id == current));
            }

            return result;
        }

        public IReadOnlyList<MpvPlaylistItem> Playlist()
        {
            return _mpvPlayer.GetPlaylist();
        }

        public IReadOnlyList<MpvMenuItem> MenuData()
        {
            return _mpvPlayer.GetMenu();
        }

        public IReadOnlyList<MpvChapter> Chapters()
        {
            return _mpvPlayer.GetChapters();
        }

        public IReadOnlyList<MpvEdition> Editions()
        {
            return _mpvPlayer.GetEditions();
        }

        public IReadOnlyList<MpvAudioDevice> AudioDevices()
        {
            return _mpvPlayer.GetAudioDevices();
        }

        public IReadOnlyList<MpvProfile> Profiles()
        {
            return _mpvPlayer.GetProfiles();
        }

        public void AddSubtitle(string path, bool selected = true, string? title = null)
        {
            _mpvPlayer.AddSubtitle(path, selected, title ?? "");
        }

        public void AddSubtitles(IReadOnlyList<string> paths)
        {
            foreach (var path in paths)
            {
                _mpvPlayer.AddSubtitle(path, false, string.Empty);
            }
        }

        public void Close()
        {
            _mpvPlayer.VoConfigured -= MpvPlayer_VoConfigured;
            _mpvPlayer?.Destroy();
        }
    }

    public enum RepeatState
    {
        All,
        One,
        None
    }
}