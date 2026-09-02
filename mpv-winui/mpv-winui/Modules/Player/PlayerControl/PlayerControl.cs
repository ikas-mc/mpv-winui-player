using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using mpv_winrt;
using mpv_winui.Modules.Common.Utils;
using mpv_winui.Modules.Common.View.Controls;
using System;
using System.Diagnostics;
using System.Linq;
using Windows.Foundation;
using TickBar = mpv_winui.Modules.Common.View.Controls.TickBar;

namespace mpv_winui.Modules.Player.PlayerControl;

[TemplatePart(Name = "PlayPauseButton", Type = typeof(Button))]
[TemplatePart(Name = "SkipBackwardButton", Type = typeof(Button))]
[TemplatePart(Name = "SkipForwardButton", Type = typeof(Button))]
[TemplatePart(Name = "VolumeMuteButton", Type = typeof(Button))]
[TemplatePart(Name = "FullScreenButton", Type = typeof(Button))]
[TemplatePart(Name = "FullWindowButton", Type = typeof(Button))]
[TemplatePart(Name = "RepeatButton", Type = typeof(Button))]
[TemplatePart(Name = "ShuffleButton", Type = typeof(Button))]
[TemplatePart(Name = "StopButton", Type = typeof(Button))]
[TemplatePart(Name = "TrackSelectionButton", Type = typeof(Button))]
[TemplatePart(Name = "ZoomButton", Type = typeof(Button))]
[TemplatePart(Name = "PreviousTrackButton", Type = typeof(Button))]
[TemplatePart(Name = "NextTrackButton", Type = typeof(Button))]
[TemplatePart(Name = "MoreButton", Type = typeof(Button))]
[TemplatePart(Name = "ProgressSlider", Type = typeof(Slider))]
[TemplatePart(Name = "VolumeSlider", Type = typeof(SliderExt))]
[TemplatePart(Name = "VolumeSliderContainer", Type = typeof(Grid))]
[TemplatePart(Name = "ProgressTickBar", Type = typeof(TickBar))]
[TemplatePart(Name = "TimeElapsedElement", Type = typeof(TextBlock))]
[TemplatePart(Name = "TimeDurationElement", Type = typeof(TextBlock))]
[TemplatePart(Name = "ErrorTextBlock", Type = typeof(TextBlock))]
[TemplatePart(Name = "TrackSelectorControl", Type = typeof(PlayerTrackSelectorControl))]
[TemplatePart(Name = "PlaybackRateFlyout", Type = typeof(MenuFlyout))]
[TemplatePart(Name = "ZoomSelectionFlyout", Type = typeof(MenuFlyout))]
[TemplatePart(Name = "MoreSkipBackward", Type = typeof(MenuFlyoutItem))]
[TemplatePart(Name = "MoreSkipForward", Type = typeof(MenuFlyoutItem))]
[TemplatePart(Name = "MoreShuffle", Type = typeof(MenuFlyoutItem))]
[TemplatePart(Name = "MoreRepeat", Type = typeof(MenuFlyoutItem))]
[TemplatePart(Name = "MorePreviousTrack", Type = typeof(MenuFlyoutItem))]
[TemplatePart(Name = "MoreNextTrack", Type = typeof(MenuFlyoutItem))]
[TemplatePart(Name = "MoreFullWindow", Type = typeof(MenuFlyoutItem))]
[TemplatePart(Name = "MoreFullScreen", Type = typeof(MenuFlyoutItem))]
[TemplatePart(Name = "MorePlaybackRate", Type = typeof(MenuFlyoutSubItem))]
[TemplatePart(Name = "MoreZoom", Type = typeof(MenuFlyoutSubItem))]
public sealed partial class PlayerControl : Control
{
    public delegate bool FullScreenRequestHandler();
    public delegate bool FullWindowRequestHandler();
    public delegate void OnPanelVisibleChangedHandler(bool hide);
    public delegate void OnPositionChangedHandler();

    public event FullScreenRequestHandler? OnFullScreenRequest;
    public event FullWindowRequestHandler? OnFullWindowRequest;
    public event OnPanelVisibleChangedHandler? OnPanelVisibleChanged;
    public event OnPositionChangedHandler? OnPositionChanged;

    public event EventHandler<(double HoverSec, double X, double Y)>? PreviewUpdateRequested;
    public event EventHandler? PreviewClearRequested;

    private Button? _playPauseButton;
    private Button? _skipBackwardButton;
    private Button? _skipForwardButton;
    private Button? _volumeMuteButton;
    private Button? _fullScreenButton;
    private Button? _fullWindowButton;
    private Button? _repeatButton;
    private Button? _shuffleButton;
    private Button? _stopButton;
    private Button? _trackSelectionButton;
    private Button? _zoomButton;
    private Button? _previousTrackButton;
    private Button? _nextTrackButton;
    private Button? _moreButton;

    private Slider? _progressSlider;
    private SliderExt? _volumeSlider;
    private Grid? _volumeSliderContainer;
    private TickBar? _progressTickBar;

    private TextBlock? _timeElapsedElement;
    private TextBlock? _timeDurationElement;
    private TextBlock? _errorTextBlock;

    private PlayerTrackSelectorControl? _trackSelectorControl;

    private MenuFlyout? _playbackRateFlyout;
    private MenuFlyout? _zoomSelectionFlyout;

    private MenuFlyoutItem? _moreSkipBackward;
    private MenuFlyoutItem? _moreSkipForward;
    private MenuFlyoutItem? _moreShuffle;
    private MenuFlyoutItem? _moreRepeat;
    private MenuFlyoutItem? _morePreviousTrack;
    private MenuFlyoutItem? _moreNextTrack;
    private MenuFlyoutItem? _moreFullWindow;
    private MenuFlyoutItem? _moreFullScreen;
    private MenuFlyoutSubItem? _morePlaybackRate;
    private MenuFlyoutSubItem? _moreZoom;

    private bool _controlPanelIsVisible = true;

    private readonly DispatcherTimer _positionUpdateTimer;
    private bool _hasError = false;
    private bool _isBuffering = false;
    private bool _isInScrubMode = false;
    private bool _isDragging = false;

    private bool _isFullScreen;
    private bool _isFullWindow;
    private string _lastError = string.Empty;

    private MpvPlayer? _mediaPlayer;

    public static readonly DependencyProperty NextTrackButtonVisibilityProperty = DependencyProperty.Register("NextTrackButtonVisibility", typeof(Visibility), typeof(PlayerControl), new PropertyMetadata(Visibility.Collapsed, (DependencyObject d, DependencyPropertyChangedEventArgs e) =>
    {
        if (d is PlayerControl self)
        {
            self._nextTrackButton?.Visibility = (Visibility)e.NewValue;
        }
    }));

    public static readonly DependencyProperty IsNextTrackButtonEnabledProperty = DependencyProperty.Register("IsNextTrackButtonEnabled", typeof(bool), typeof(PlayerControl), new PropertyMetadata(true, (DependencyObject d, DependencyPropertyChangedEventArgs e) =>
    {
        if (d is PlayerControl self)
        {
            self._nextTrackButton?.IsEnabled = (bool)e.NewValue;
        }
    }));

    public static readonly DependencyProperty PreviousTrackButtonVisibilityProperty = DependencyProperty.Register("PreviousTrackButtonVisibility", typeof(Visibility), typeof(PlayerControl), new PropertyMetadata(Visibility.Collapsed, (DependencyObject d, DependencyPropertyChangedEventArgs e) =>
    {
        if (d is PlayerControl self)
        {
            self._previousTrackButton?.Visibility = (Visibility)e.NewValue;
        }
    }));

    public static readonly DependencyProperty IsPreviousTrackButtonEnabledProperty = DependencyProperty.Register("IsPreviousTrackButtonEnabled", typeof(bool), typeof(PlayerControl), new PropertyMetadata(true, (DependencyObject d, DependencyPropertyChangedEventArgs e) =>
    {
        if (d is PlayerControl self)
        {
            self._previousTrackButton?.IsEnabled = (bool)e.NewValue;
        }
    }));

    public PlayerControl()
    {
        DefaultStyleKey = typeof(PlayerControl);
        this.Loaded += PlayerControl_Loaded;
        this.Unloaded += PlayerControl_Unloaded;

        _positionUpdateTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
    }

    public MpvPlayer? MediaPlayer
    {
        get
        {
            return _mediaPlayer;
        }
        set
        {
            if (_mediaPlayer != value)
            {
                RemoveEventListeners();

                _mediaPlayer = value;

                _hasError = false;
                _isBuffering = false;

                if (null != value)
                {
                    AddEventListeners();

                    RefreshVolumeFromPlayer();
                    UpdateTimeUI();
                    UpdateChapters(false);
                    UpdateShuffleButtonUI();
                    UpdateRepeatButtonUI();
                    UpdatePlayPauseUI(value.IsPaused(), false);
                    RestoreErrorText();
                    UpdatePlaybackStatusUI(false);
                }
            }
        }
    }

    public Visibility NextTrackButtonVisibility
    {
        get
        {
            return (Visibility)GetValue(NextTrackButtonVisibilityProperty);
        }
        set
        {
            SetValue(NextTrackButtonVisibilityProperty, value);
        }
    }

    public bool IsNextTrackButtonEnabled
    {
        get
        {
            return (bool)GetValue(IsNextTrackButtonEnabledProperty);
        }
        set
        {
            SetValue(IsNextTrackButtonEnabledProperty, value);
        }
    }

    public Visibility PreviousTrackButtonVisibility
    {
        get
        {
            return (Visibility)GetValue(PreviousTrackButtonVisibilityProperty);
        }
        set
        {
            SetValue(PreviousTrackButtonVisibilityProperty, value);
        }
    }

    public bool IsPreviousTrackButtonEnabled
    {
        get
        {
            return (bool)GetValue(IsPreviousTrackButtonEnabledProperty);
        }
        set
        {
            SetValue(IsPreviousTrackButtonEnabledProperty, value);
        }
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        UnhookPartEvents();
        ResolveParts();
        ApplyButtonVisibilityStates();
        HookPartEvents();

        RefreshVolumeFromPlayer();
        UpdateTimeUI();
        UpdateChapters(false);
        UpdateShuffleButtonUI();
        UpdateRepeatButtonUI();
        if (_mediaPlayer is { } player)
        {
            UpdatePlayPauseUI(player.IsPaused(), false);
        }
        UpdatePlaybackStatusUI(false);
        RestoreErrorText();
        UpdateFullScreenUI(_isFullScreen);
        UpdateFullWindowUI(_isFullWindow);
        RestoreControlPanelVisibility();

        _currentSegment = -1;
        if (ActualWidth > 0)
        {
            UpdateToolbarVisibility(ActualWidth);
        }
    }

    private void ResolveParts()
    {
        _playPauseButton = GetPart<Button>("PlayPauseButton");
        _skipBackwardButton = GetPart<Button>("SkipBackwardButton");
        _skipForwardButton = GetPart<Button>("SkipForwardButton");
        _volumeMuteButton = GetPart<Button>("VolumeMuteButton");
        _fullScreenButton = GetPart<Button>("FullScreenButton");
        _fullWindowButton = GetPart<Button>("FullWindowButton");
        _repeatButton = GetPart<Button>("RepeatButton");
        _shuffleButton = GetPart<Button>("ShuffleButton");
        _stopButton = GetPart<Button>("StopButton");
        _trackSelectionButton = GetPart<Button>("TrackSelectionButton");
        _zoomButton = GetPart<Button>("ZoomButton");
        _previousTrackButton = GetPart<Button>("PreviousTrackButton");
        _nextTrackButton = GetPart<Button>("NextTrackButton");
        _moreButton = GetPart<Button>("MoreButton");

        _progressSlider = GetPart<Slider>("ProgressSlider");
        _volumeSlider = GetPart<SliderExt>("VolumeSlider");
        _volumeSliderContainer = GetPart<Grid>("VolumeSliderContainer");
        _progressTickBar = GetPart<TickBar>("ProgressTickBar");

        _timeElapsedElement = GetPart<TextBlock>("TimeElapsedElement");
        _timeDurationElement = GetPart<TextBlock>("TimeDurationElement");
        _errorTextBlock = GetPart<TextBlock>("ErrorTextBlock");

        _trackSelectorControl = GetPart<PlayerTrackSelectorControl>("TrackSelectorControl");

        _playbackRateFlyout = GetPart<MenuFlyout>("PlaybackRateFlyout");
        _zoomSelectionFlyout = GetPart<MenuFlyout>("ZoomSelectionFlyout");

        _moreSkipBackward = GetPart<MenuFlyoutItem>("MoreSkipBackward");
        _moreSkipForward = GetPart<MenuFlyoutItem>("MoreSkipForward");
        _moreShuffle = GetPart<MenuFlyoutItem>("MoreShuffle");
        _moreRepeat = GetPart<MenuFlyoutItem>("MoreRepeat");
        _morePreviousTrack = GetPart<MenuFlyoutItem>("MorePreviousTrack");
        _moreNextTrack = GetPart<MenuFlyoutItem>("MoreNextTrack");
        _moreFullWindow = GetPart<MenuFlyoutItem>("MoreFullWindow");
        _moreFullScreen = GetPart<MenuFlyoutItem>("MoreFullScreen");
        _morePlaybackRate = GetPart<MenuFlyoutSubItem>("MorePlaybackRate");
        _moreZoom = GetPart<MenuFlyoutSubItem>("MoreZoom");
    }

    private T? GetPart<T>(string name) where T : DependencyObject
    {
        return GetTemplateChild(name) as T;
    }

    private void ApplyButtonVisibilityStates()
    {
        _nextTrackButton?.Visibility = NextTrackButtonVisibility;
        _nextTrackButton?.IsEnabled = IsNextTrackButtonEnabled;

        _previousTrackButton?.Visibility = PreviousTrackButtonVisibility;
        _previousTrackButton?.IsEnabled = IsPreviousTrackButtonEnabled;
    }

    private void HookPartEvents()
    {
        _playPauseButton?.Click += OnPlayPauseClick;
        _skipBackwardButton?.Click += SkipBackwardButton_Click;
        _skipForwardButton?.Click += SkipForwardButton_Click;

        _volumeMuteButton?.Click += OnMuteClick;
        _volumeMuteButton?.PointerWheelChanged += VolumeMuteButton_PointerWheelChanged;

        _fullScreenButton?.Click += OnFullScreenClick;
        _fullWindowButton?.Click += FullWindowButton_Click;
        _repeatButton?.Click += OnRepeatClick;
        _shuffleButton?.Click += OnShuffleClick;
        _stopButton?.Click += StopButton_Click;
        _trackSelectionButton?.Click += TrackSelectionButton_Click;
        _zoomButton?.Click += ZoomButton_Click;
        _previousTrackButton?.Click += PreviousTrackButton_Click;
        _nextTrackButton?.Click += NextTrackButton_Click;

        if (_playbackRateFlyout is { } playbackRateFlyout)
        {
            foreach (var item in playbackRateFlyout.Items)
            {
                if (item is MenuFlyoutItem menuFlyoutItem)
                {
                    menuFlyoutItem.Click += PlaybackRateFlyout_MenuFlyoutItem_Click;
                }
            }
        }

        _progressSlider?.ValueChanged += OnPositionSliderValueChanged;
        if (AppContext.AppSetting.EnableVideoPreview || AppContext.AppSetting.EnableVideoBuiltInPreview)
        {
            _progressSlider?.PointerEntered += ProgressSlider_PointerEntered;
            _progressSlider?.PointerMoved += ProgressSlider_PointerMoved;
            _progressSlider?.PointerExited += ProgressSlider_PointerExited;
        }

        _volumeSlider?.ValueChanged2 += OnVolumeSliderValueChanged;

        _moreSkipBackward?.Click += SkipBackwardButton_Click;
        _moreSkipForward?.Click += SkipForwardButton_Click;
        _moreShuffle?.Click += OnShuffleClick;
        _moreRepeat?.Click += OnRepeatClick;
        _morePreviousTrack?.Click += PreviousTrackButton_Click;
        _moreNextTrack?.Click += NextTrackButton_Click;
        _moreFullWindow?.Click += FullWindowButton_Click;
        _moreFullScreen?.Click += OnFullScreenClick;

        if (_morePlaybackRate is { } morePlaybackRate)
        {
            foreach (var item in morePlaybackRate.Items)
            {
                if (item is MenuFlyoutItem menuFlyoutItem)
                {
                    menuFlyoutItem.Click += PlaybackRateFlyout_MenuFlyoutItem_Click;
                }
            }
        }

        if (_moreZoom is { } moreZoom)
        {
            foreach (var item in moreZoom.Items)
            {
                if (item is MenuFlyoutItem menuFlyoutItem)
                {
                    menuFlyoutItem.Click += ZoomSelectionMenu_Click;
                }
            }
        }
    }

    private void UnhookPartEvents()
    {
        _playPauseButton?.Click -= OnPlayPauseClick;
        _skipBackwardButton?.Click -= SkipBackwardButton_Click;
        _skipForwardButton?.Click -= SkipForwardButton_Click;

        _volumeMuteButton?.Click -= OnMuteClick;
        _volumeMuteButton?.PointerWheelChanged -= VolumeMuteButton_PointerWheelChanged;

        _fullScreenButton?.Click -= OnFullScreenClick;
        _fullWindowButton?.Click -= FullWindowButton_Click;
        _repeatButton?.Click -= OnRepeatClick;
        _shuffleButton?.Click -= OnShuffleClick;
        _stopButton?.Click -= StopButton_Click;
        _trackSelectionButton?.Click -= TrackSelectionButton_Click;
        _zoomButton?.Click -= ZoomButton_Click;
        _previousTrackButton?.Click -= PreviousTrackButton_Click;
        _nextTrackButton?.Click -= NextTrackButton_Click;

        if (_playbackRateFlyout is { } playbackRateFlyout)
        {
            foreach (var item in playbackRateFlyout.Items)
            {
                if (item is MenuFlyoutItem menuFlyoutItem)
                {
                    menuFlyoutItem.Click -= PlaybackRateFlyout_MenuFlyoutItem_Click;
                }
            }
        }

        _progressSlider?.ValueChanged -= OnPositionSliderValueChanged;
        _progressSlider?.PointerEntered -= ProgressSlider_PointerEntered;
        _progressSlider?.PointerMoved -= ProgressSlider_PointerMoved;
        _progressSlider?.PointerExited -= ProgressSlider_PointerExited;

        _volumeSlider?.ValueChanged2 -= OnVolumeSliderValueChanged;

        _moreSkipBackward?.Click -= SkipBackwardButton_Click;
        _moreSkipForward?.Click -= SkipForwardButton_Click;
        _moreShuffle?.Click -= OnShuffleClick;
        _moreRepeat?.Click -= OnRepeatClick;
        _morePreviousTrack?.Click -= PreviousTrackButton_Click;
        _moreNextTrack?.Click -= NextTrackButton_Click;
        _moreFullWindow?.Click -= FullWindowButton_Click;
        _moreFullScreen?.Click -= OnFullScreenClick;

        if (_morePlaybackRate is { } morePlaybackRate)
        {
            foreach (var item in morePlaybackRate.Items)
            {
                if (item is MenuFlyoutItem menuFlyoutItem)
                {
                    menuFlyoutItem.Click -= PlaybackRateFlyout_MenuFlyoutItem_Click;
                }
            }
        }

        if (_moreZoom is { } moreZoom)
        {
            foreach (var item in moreZoom.Items)
            {
                if (item is MenuFlyoutItem menuFlyoutItem)
                {
                    menuFlyoutItem.Click -= ZoomSelectionMenu_Click;
                }
            }
        }
    }

    private void PlayerControl_Loaded(object sender, RoutedEventArgs e)
    {
        _timeElapsedElement?.Text = "00:00";
        _timeDurationElement?.Text = "00:00";

        _positionUpdateTimer.Tick += OnPositionUpdateTimerTick;
        _positionUpdateTimer.Start();

        UpdateToolbarVisibility(ActualWidth);

        SizeChanged += PlayerControl_SizeChanged;

        RefreshVolumeFromPlayer();
        UpdateTimeUI();
        UpdateChapters(false);
        UpdateShuffleButtonUI();
        UpdateRepeatButtonUI();
        if (_mediaPlayer is { } player)
        {
            UpdatePlayPauseUI(player.IsPaused(), false);
        }
        UpdatePlaybackStatusUI(false);
        RestoreErrorText();
    }

    private void PlayerControl_Unloaded(object sender, RoutedEventArgs e)
    {
        SizeChanged -= PlayerControl_SizeChanged;

        _positionUpdateTimer.Stop();
        _positionUpdateTimer.Tick -= OnPositionUpdateTimerTick;

        UnhookPartEvents();
        RemoveEventListeners();
    }

    private void AddEventListeners()
    {
        _mediaPlayer?.FileStarted += MediaPlayer_FileStarted;
        _mediaPlayer?.FileLoaded += MediaPlayer_FileLoaded;
        _mediaPlayer?.FileFailed += MediaPlayer_FileFailed;
        _mediaPlayer?.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;
        _mediaPlayer?.SeekStarted += MediaPlayer_SeekStarted;
        _mediaPlayer?.BufferingChanged += MediaPlayer_BufferingChanged;
        _mediaPlayer?.PlaybackRestarted += MediaPlayer_PlaybackRestarted;
        _mediaPlayer?.VolumeChanged += PlaybackSession_VolumeChanged;
        _mediaPlayer?.LoopFileChanged += PlaybackSession_RepeatStateChanged;
        _mediaPlayer?.LoopPlaylistChanged += PlaybackSession_RepeatStateChanged;
        _mediaPlayer?.ShuffleChanged += PlaybackSession_ShuffleChanged;
    }

    private void RemoveEventListeners()
    {
        _mediaPlayer?.FileStarted -= MediaPlayer_FileStarted;
        _mediaPlayer?.FileLoaded -= MediaPlayer_FileLoaded;
        _mediaPlayer?.FileFailed -= MediaPlayer_FileFailed;
        _mediaPlayer?.PlaybackStateChanged -= PlaybackSession_PlaybackStateChanged;
        _mediaPlayer?.SeekStarted -= MediaPlayer_SeekStarted;
        _mediaPlayer?.BufferingChanged -= MediaPlayer_BufferingChanged;
        _mediaPlayer?.PlaybackRestarted -= MediaPlayer_PlaybackRestarted;
        _mediaPlayer?.VolumeChanged -= PlaybackSession_VolumeChanged;
        _mediaPlayer?.LoopFileChanged -= PlaybackSession_RepeatStateChanged;
        _mediaPlayer?.LoopPlaylistChanged -= PlaybackSession_RepeatStateChanged;
        _mediaPlayer?.ShuffleChanged -= PlaybackSession_ShuffleChanged;
    }

    private void NextTrackButton_Click(object sender, RoutedEventArgs e)
    {
        _mediaPlayer?.PlaylistNext();
    }

    private void PreviousTrackButton_Click(object sender, RoutedEventArgs e)
    {
        _mediaPlayer?.PlaylistPrevious();
    }

    private void ZoomButton_Click(object sender, RoutedEventArgs e)
    {
        if (_zoomSelectionFlyout is { } flyout && _moreButton is { } moreButton)
        {
            //TODO
            flyout.Items.Clear();

            var item = new MenuFlyoutItem() { Text = "Auto", Tag = "no", };
            item.Click += ZoomSelectionMenu_Click;
            flyout.Items.Add(item);

            item = new MenuFlyoutItem() { Text = "4:3", Tag = "4:3", };
            item.Click += ZoomSelectionMenu_Click;
            flyout.Items.Add(item);

            item = new MenuFlyoutItem() { Text = "16:9", Tag = "16:9", };
            item.Click += ZoomSelectionMenu_Click;
            flyout.Items.Add(item);

            item = new MenuFlyoutItem() { Text = "16:10", Tag = "16:10", };
            item.Click += ZoomSelectionMenu_Click;
            flyout.Items.Add(item);

            if (sender is MenuFlyoutItem)
            {
                flyout.ShowAt(moreButton);
            }
        }
    }

    private void ZoomSelectionMenu_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItemBase item && item.Tag is string tag)
        {
            _mediaPlayer?.SetAspectRatio(tag);
        }
    }

    private void TrackSelectionButton_Click(object sender, RoutedEventArgs e)
    {
        if (_mediaPlayer is { } player && _trackSelectorControl is { } trackSelector)
        {
            try
            {
                trackSelector.VideoTrackSelected -= TrackSelectorControl_VideoTrackSelected;
                trackSelector.LoadVideoTracks(player.GetVideoTracks() ?? []);
                trackSelector.VideoTrackSelected += TrackSelectorControl_VideoTrackSelected;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"TrackSelectionButton_Click video error: {ex.Message}");
            }

            try
            {
                trackSelector.SubtitleTrackSelected -= TrackSelectorControl_SubtitleTrackSelected;
                trackSelector.LoadSubtitleTracks(player.GetSubtitleTracks() ?? [], "Off");
                trackSelector.SubtitleTrackSelected += TrackSelectorControl_SubtitleTrackSelected;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"TrackSelectionButton_Click sub error: {ex.Message}");
            }

            try
            {
                trackSelector.AudioTrackSelected -= TrackSelectorControl_AudioTrackSelected;
                trackSelector.LoadAudioTracks(player.GetAudioTracks() ?? []);
                trackSelector.AudioTrackSelected += TrackSelectorControl_AudioTrackSelected;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"TrackSelectionButton_Click audio error: {ex.Message}");
            }

            try
            {
                trackSelector.SecondSubTrackSelected -= TrackSelectorControl_SecondSubTrackSelected;
                trackSelector.LoadSecondSubtitleTracks(player.GetSubtitleTracks() ?? [], _mediaPlayer?.CurrentSecondSubtitleTrack() ?? -1, AppContext.AppLang.Off);
                trackSelector.SecondSubTrackSelected += TrackSelectorControl_SecondSubTrackSelected;
                trackSelector.SetSecondSubVisibility(true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"TrackSelectionButton_Click second-sub error: {ex.Message}");
            }
        }
    }

    private void TrackSelectorControl_VideoTrackSelected(object? sender, int trackIndex)
    {
        _mediaPlayer?.CurrentVideoTrack(trackIndex);
    }

    private void TrackSelectorControl_SubtitleTrackSelected(object? sender, int trackIndex)
    {
        _mediaPlayer?.CurrentSubtitleTrack(trackIndex);
    }

    private void TrackSelectorControl_AudioTrackSelected(object? sender, int trackIndex)
    {
        _mediaPlayer?.CurrentAudioTrack(trackIndex);
    }

    private void TrackSelectorControl_SecondSubTrackSelected(object? sender, int trackIndex)
    {
        _mediaPlayer?.CurrentSecondSubtitleTrack(trackIndex);
    }

    private void StopButton_Click(object sender, RoutedEventArgs e)
    {
        _mediaPlayer?.Stop();
    }

    private void PlaybackRateFlyout_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        if (_mediaPlayer is { } player && sender is MenuFlyoutItem item && item.Tag is string tag)
        {
            if (double.TryParse(tag, out double speed) && speed >= 0)
            {
                player.PlaybackSpeed(speed);
            }
        }
    }


    private void SkipForwardButton_Click(object sender, RoutedEventArgs e)
    {
        Forward();
    }

    private void SkipBackwardButton_Click(object sender, RoutedEventArgs e)
    {
        Backward();
    }

    public void Forward()
    {
        if (_mediaPlayer is { } player)
        {
            player.Position(player.Position() + 30);
        }
    }

    public void Backward()
    {
        if (_mediaPlayer is { } player)
        {
            player.Position(player.Position() - 10);
        }
    }

    private void PlaybackSession_PlaybackStateChanged(PlaybackStateChangedEventArgs args)
    {
        DispatcherQueue.RunAsync(() =>
        {
            if (args.IsPaused)
            {
                _positionUpdateTimer.Stop();
            }
            else
            {
                _positionUpdateTimer.Start();
            }

            UpdatePlayPauseUI(args.IsPaused, true);
        });
    }

    private void MediaPlayer_FileStarted()
    {
        DispatcherQueue.RunAsync(() =>
        {
            _hasError = false;
            _isBuffering = true;
            UpdatePlaybackStatusUI(false);
        });
    }

    private void MediaPlayer_FileLoaded()
    {
        _hasError = false;
        var duration = _mediaPlayer?.Duration();
        var isPaused = _mediaPlayer?.IsPaused();
        if (duration != null)
        {
            DispatcherQueue.RunAsync(() =>
            {
                UpdateProgressSliderValue(0, duration);
                if (duration > 0)
                {
                    ApplyAdaptiveSliderStep(duration.Value);
                }

                UpdatePlaybackStatusUI(false);
                UpdatePlayPauseUI(isPaused ?? true, false);
                UpdateVolumeUI(false);
                UpdateChapters(false);
            });
        }
    }

    private void MediaPlayer_SeekStarted()
    {
        DispatcherQueue.RunAsync(() =>
        {
            _isBuffering = true;
            UpdatePlaybackStatusUI(true);
        });
    }

    private void MediaPlayer_BufferingChanged(bool isBuffering)
    {
        DispatcherQueue.RunAsync(() =>
        {
            _isBuffering = isBuffering;
            UpdatePlaybackStatusUI(true);
        });
    }

    private void MediaPlayer_PlaybackRestarted()
    {
        if (_mediaPlayer is { } player)
        {
            var isPaused = player.IsPaused();
            DispatcherQueue.RunAsync(() =>
            {
                _isBuffering = false;
                UpdatePlaybackStatusUI(true);

                if (isPaused)
                {
                    UpdateTimeUI();
                }
            });
        }
    }

    private void ApplyAdaptiveSliderStep(double durationSeconds)
    {
        if (durationSeconds <= 0)
        {
            return;
        }

        var small = Math.Max(10, durationSeconds / 40.0);
        var large = Math.Max(10, durationSeconds / 20.0);

        small = Math.Round(small);
        large = Math.Round(large);

        _progressSlider?.SmallChange = small;
        _progressSlider?.LargeChange = large;
        _progressSlider?.StepFrequency = 1;
    }

    private void UpdateChapters(bool hide)
    {
        if (!hide && _mediaPlayer is { } player)
        {
            double duration = player.Duration();
            var ticks = player.GetChapters()
                .Select(c => c.Time)
                .Where(t => t > 0 && (duration <= 0 || t < duration))
                .ToList();
            _progressTickBar?.Values = ticks;
            _progressTickBar?.Maximum = duration;
            _progressTickBar?.Visibility = ticks.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        else
        {
            _progressTickBar?.Values = null;
            _progressTickBar?.Maximum = 0;
            _progressTickBar?.Visibility = Visibility.Collapsed;
        }
    }

    private void MediaPlayer_FileFailed(FileFailedEventArgs args)
    {
        DispatcherQueue.RunAsync(() =>
        {
            _hasError = true;
            _lastError = args.Message;
            _errorTextBlock?.Text = _lastError;
            UpdatePlaybackStatusUI(true);
            UpdateChapters(true);
        });
    }

    private void PlaybackSession_VolumeChanged(VolumeChangedEventArgs args)
    {
        DispatcherQueue.RunAsync(() =>
        {
            _volumeSlider?.Value2 = args.Volume;
            UpdateVolumeUI(true);
        });
    }

    private void PlaybackSession_RepeatStateChanged()
    {
        DispatcherQueue.RunAsync(UpdateRepeatButtonUI);
    }

    private void PlaybackSession_ShuffleChanged()
    {
        DispatcherQueue.RunAsync(UpdateShuffleButtonUI);
    }

    private void OnPlayPauseClick(object sender, RoutedEventArgs e)
    {
        TogglePlay();
    }

    public void TogglePlay()
    {
        if (_mediaPlayer is { } player)
        {
            if (!player.IsPaused())
            {
                player.Pause();
            }
            else
            {
                player.Play();
            }
        }
    }

    private void OnMuteClick(object sender, RoutedEventArgs e)
    {
        if (_mediaPlayer is { } player && _volumeMuteButton is { } volumeMuteButton)
        {
            //TODO 
            if (_volumeSliderContainer?.Visibility == Visibility.Visible)
            {
                player.IsMuted(!player.IsMuted());
                UpdateVolumeUI(true);
            }
            else
            {
                var control = new VolumeFlyoutControl(player);
                var flyout = new Flyout { Content = control };
                flyout.ShowAt(volumeMuteButton);
            }
        }
    }

    private void FullWindowButton_Click(object sender, RoutedEventArgs e)
    {
        ToggleFullWindow();
    }

    public bool ToggleFullWindow()
    {
        var full = OnFullWindowRequest?.Invoke();
        if (null != full)
        {
            _isFullWindow = full.Value;
            UpdateFullWindowUI(full.Value);
            return full.Value;
        }

        return false;
    }

    private void OnFullScreenClick(object sender, RoutedEventArgs e)
    {
        ToggleFullScreen();
    }

    public bool ToggleFullScreen()
    {
        var full = OnFullScreenRequest?.Invoke();
        if (null != full)
        {
            _isFullScreen = full.Value;
            UpdateFullScreenUI(full.Value);
            return full.Value;
        }

        return false;
    }

    public void UpdateFullScreen(bool enabled)
    {
        _isFullScreen = enabled;
        UpdateFullScreenUI(enabled);
    }

    private void OnRepeatClick(object sender, RoutedEventArgs e)
    {
        if (_mediaPlayer is { } player)
        {
            player.SetRepeatState(player.GetRepeatState() switch
            {
                RepeatState.All => RepeatState.One,
                RepeatState.One => RepeatState.None,
                _ => RepeatState.All,
            });
            UpdateRepeatButtonUI();
        }
    }

    private void OnShuffleClick(object sender, RoutedEventArgs e)
    {
        if (_mediaPlayer is { } player)
        {
            if (player.Shuffle())
            {
                player.SetShuffle(false);
            }
            else
            {
                player.SetShuffle(true);

                //TODO 
                player.PlaylistShuffle();
            }
            UpdateShuffleButtonUI();
        }
    }

    private void OnPositionSliderValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        if (!_isInScrubMode)
        {
            _mediaPlayer?.Position(e.NewValue);
        }
    }

    private void OnVolumeSliderValueChanged(object sender, double value)
    {
        _mediaPlayer?.Volume(value);
    }

    private void OnPositionUpdateTimerTick(object? sender, object e)
    {
        UpdateTimeUI();
    }

    private void UpdateTimeUI()
    {
        if (_mediaPlayer is { } player)
        {
            var position = player.Position();
            var duration = player.Duration();
            if (position >= 0 && duration >= 0)
            {
                UpdateProgressSliderValue(position, duration);
                _timeElapsedElement?.Text = FormatTime(position);
                _timeDurationElement?.Text = FormatTime(duration);
                OnPositionChanged?.Invoke();
            }
        }
    }

    private void UpdateProgressSliderValue(double? value, double? max = null)
    {
        _isInScrubMode = true;
        if (_progressSlider is { } progressSlider)
        {
            if (null != value)
            {
                _progressSlider.Value = value ?? 0;
            }

            if (null != max && max != progressSlider.Maximum)
            {
                _progressSlider.Maximum = max ?? 0;
            }
        }

        _isInScrubMode = false;
    }

    private string FormatTime(double second)
    {
        var ts = TimeSpan.FromSeconds(second);
        return ts.TotalHours switch
        {
            >= 1 => $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}",
            _ => $"{ts.Minutes:D2}:{ts.Seconds:D2}"
        };
    }

    public bool IsVisible()
    {
        return _controlPanelIsVisible;
    }

    public void ToggleControlPanel()
    {
        if (_controlPanelIsVisible)
        {
            HideControlPanel();
        }
        else
        {
            ShowControlPanel();
        }
    }

    public void ShowControlPanel()
    {
        if (!_controlPanelIsVisible)
        {
            VisualStateManager.GoToState(this, "ControlPanelFadeIn", true);
            _controlPanelIsVisible = true;
        }

        OnPanelVisibleChanged?.Invoke(false);
    }

    public void HideControlPanel()
    {
        if (_controlPanelIsVisible)
        {
            VisualStateManager.GoToState(this, "ControlPanelFadeOut", true);
            _controlPanelIsVisible = false;
        }

        OnPanelVisibleChanged?.Invoke(true);
    }

    private void UpdateFullScreenUI(bool enabled)
    {
        if (enabled)
        {
            VisualStateManager.GoToState(this, "FullScreenState", false);
        }
        else
        {
            VisualStateManager.GoToState(this, "NonFullScreenState", false);
        }
    }
    private void UpdateFullWindowUI(bool enabled)
    {
        if (enabled)
        {
            VisualStateManager.GoToState(this, "FullWindowState", false);
        }
        else
        {
            VisualStateManager.GoToState(this, "NonFullWindowState", false);
        }
    }

    private void UpdateRepeatButtonUI()
    {
        var stateName = _mediaPlayer?.GetRepeatState() switch
        {
            RepeatState.One => "RepeatOneState",
            RepeatState.None => "RepeatNoneState",
            _ => "RepeatAllState",
        };

        VisualStateManager.GoToState(this, stateName, false);
    }

    private void UpdateShuffleButtonUI()
    {
        var stateName = _mediaPlayer?.Shuffle() switch
        {
            true => "ShuffleState",
            _ => "ShuffleNoneState",
        };

        VisualStateManager.GoToState(this, stateName, false);
    }

    private void UpdatePlaybackStatusUI(bool useTransitions)
    {
        if (_hasError)
        {
            VisualStateManager.GoToState(this, "Error", useTransitions);
        }
        else if (_isBuffering)
        {
            VisualStateManager.GoToState(this, "Buffering", useTransitions);
        }
        else
        {
            VisualStateManager.GoToState(this, "Normal", useTransitions);
        }
    }

    private void UpdatePlayPauseUI(bool isPaused, bool useTransitions)
    {
        if (isPaused)
        {
            VisualStateManager.GoToState(this, "PlayState", useTransitions);
        }
        else
        {
            VisualStateManager.GoToState(this, "PauseState", useTransitions);
        }
    }

    private void UpdateVolumeUI(bool useTransitions)
    {
        if (_mediaPlayer is { } player)
        {
            if (player.IsMuted())
            {
                VisualStateManager.GoToState(this, "MuteState", useTransitions);
            }
            else
            {
                switch (player.Volume())
                {
                    case < 0.01:
                        VisualStateManager.GoToState(this, "VolumeState0", useTransitions);
                        break;
                    case < 34:
                        VisualStateManager.GoToState(this, "VolumeState1", useTransitions);
                        break;
                    case < 67:
                        VisualStateManager.GoToState(this, "VolumeState2", useTransitions);
                        break;
                    default:
                        VisualStateManager.GoToState(this, "VolumeState3", useTransitions);
                        break;
                }
            }
        }
    }

    private void RefreshVolumeFromPlayer()
    {
        if (_mediaPlayer is { } player)
        {
            _volumeSlider?.Value2 = player.Volume();
            UpdateVolumeUI(false);
        }
    }

    // Re-pushes recorded error state onto a freshly applied template.
    private void RestoreErrorText()
    {
        if (_hasError)
        {
            _errorTextBlock?.Text = _lastError;
        }
    }

    private void RestoreControlPanelVisibility()
    {
        if (!_controlPanelIsVisible)
        {
            VisualStateManager.GoToState(this, "ControlPanelFadeOut", false);
        }
    }

    private int _currentSegment = -1; // 0=wide, 1=medium, 2=compact, 3=narrow
    private void PlayerControl_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateToolbarVisibility(e.NewSize.Width);
    }

    private void UpdateToolbarVisibility(double w)
    {
        int newSegment = w >= 700 ? 0 : w >= 500 ? 1 : w >= 280 ? 2 : 3;
        if (newSegment == _currentSegment)
        {
            return;
        }

        _currentSegment = newSegment;

        string name = newSegment switch
        {
            0 => "Wide",
            1 => "Medium",
            2 => "Compact",
            _ => "Narrow"
        };
        VisualStateManager.GoToState(this, name, false);
    }

    private void ProgressSlider_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        _isDragging = true;
    }

    private void ProgressSlider_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (_isDragging)
        {
            UpdatePreview(e);
        }
    }

    private void ProgressSlider_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        _isDragging = false;
        ClearPreview();
    }

    private void UpdatePreview(PointerRoutedEventArgs e)
    {
        if (_mediaPlayer is { } player && _progressSlider is { } progressSlider)
        {
            var width = progressSlider.ActualWidth;
            if (width > 0)
            {
                var point = e.GetCurrentPoint(progressSlider);
                var fraction = point.Position.X / width;
                var hoverSec = Math.Max(0, fraction * player.Duration());

                PreviewUpdateRequested?.Invoke(this, (hoverSec, point.Position.X, 0D));
            }
        }
    }

    public Point TransformSliderPoint(UIElement element, double x, double y)
    {
        return _progressSlider?.TransformToVisual(element)?.TransformPoint(new Point(x, y)) ?? default;
    }

    private void ClearPreview()
    {
        PreviewClearRequested?.Invoke(this, EventArgs.Empty);
    }

    private void VolumeMuteButton_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        if (sender is UIElement element)
        {
            if (_mediaPlayer is { } player)
            {
                var volume = player.Volume();
                var delta = e.GetCurrentPoint(element).Properties.MouseWheelDelta;

                if (delta > 0)
                {
                    player.Volume(Math.Min(volume + 2, 100));
                }
                else if (delta < 0)
                {
                    player.Volume(Math.Max(volume - 2, 0));
                }
            }
        }
    }
}