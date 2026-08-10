using System.Runtime.InteropServices;
using Microsoft.UI.Xaml.Controls;
using Mpv;
using WinRT;
using Windows.Win32.Foundation;

namespace mpv_winrt;

public sealed unsafe class MpvPlayer
{
    private enum ObserveId : ulong
    {
        CoreIdle = 1,
        Pause = 2,
        Duration = 3,
        PlaybackTime = 4,
        TimePos = 5,
        CacheSpeed = 6,
        Speed = 7,
        Volume = 8,
        Mute = 9,
        LoopFile = 10,
        LoopPlaylist = 11,
        Shuffle = 12,
        Filename = 20,
        MediaTitle = 21,
        TrackList = 30,
        TrackListCount = 31,
        Aid = 32,
        Sid = 33,
        MenuData = 41,
        Playlist = 42,
        VoConfigured = 50,
        Fullscreen = 201,
        Ontop = 202,
        WindowMinimized = 203,
        WindowMaximized = 204,
        TitleBar = 205,
        Border = 206,
    }

    private unsafe mpv_handle* _ctx;
    private volatile bool _eventThreadRunning;
    private Thread? _eventThread;
    private nint _swapChain;

    public event Action? MediaLoaded;
    public event Action? PlaybackEnded;
    public event Action<PlaybackFailedEventArgs>? PlaybackFailed;
    public event Action? Seeked;
    public event Action? FileLoaded;
    public event Action? TrackChanged;

    public event Action<PlaybackStateChangedEventArgs>? PlaybackStateChanged;
    public event Action<VolumeChangedEventArgs>? VolumeChanged;
    public event Action<PositionChangedEventArgs>? PositionChanged;
    public event Action<SpeedChangedEventArgs>? SpeedChanged;
    public event Action<MediaInfoChangedEventArgs>? MediaInfoChanged;
    public event Action<NetworkInfoChangedEventArgs>? NetworkInfoChanged;
    public event Action<TrackListChangedEventArgs>? TrackListChanged;
    public event Action<TrackListCountChangedEventArgs>? TrackListCountChanged;
    public event Action? VoConfigured;
    public event Action<WindowChangedEventArgs>? WindowChanged;
    public event Action? LoopFileChanged;
    public event Action? LoopPlaylistChanged;
    public event Action? ShuffleChanged;
    public event Action? PlaylistChanged;

    public void Initialize(string configPath, uint width, uint height, int volume, DisplayColorKind colorKind, int refreshRate)
    {
        CreateContext();
        SetOption("config", "yes");
        SetOption("config-dir", configPath);

        SetOption("gpu-shader-cache-dir", "~~/cache/shaders_cache");
        SetOption("screenshot-dir", "~~/screenshots");
        SetOption("osc", "no");
        SetOption("idle", "yes");

        SetOption("script-opts", "select-populate_menu_data=yes");
        SetOption("load-select", "yes");
        SetOption("input-default-bindings", "yes");
        SetOption("input-media-keys", "yes");
        SetOption("media-controls", "yes");

        SetOption("reset-on-next-file", "pause,ab-loop-a,ab-loop-b");
        SetOption("volume", volume.ToString());

        SetOption("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
        SetOption("gpu-api", "d3d11");
        SetOption("d3d11-output-mode", "composition");
        SetOption("auto-window-resize", "no");
        SetOption("force-window", "yes");
        SetOption("d3d11-composition-size", $"{width}x{height}");

        if (PInvoke.mpv_initialize(ref *_ctx) < 0)
        {
            throw new InvalidOperationException("Failed to initialize mpv");
        }

        UpdateDisplayColorInfo(colorKind);
        UpdateDisplayRefreshRate(refreshRate);

        Observe(ObserveId.CoreIdle, "core-idle", mpv_format.MPV_FORMAT_FLAG);
        Observe(ObserveId.Pause, "pause", mpv_format.MPV_FORMAT_FLAG);
        Observe(ObserveId.Duration, "duration", mpv_format.MPV_FORMAT_DOUBLE);

        Observe(ObserveId.LoopFile, "loop-file", mpv_format.MPV_FORMAT_STRING);
        Observe(ObserveId.LoopPlaylist, "loop-playlist", mpv_format.MPV_FORMAT_STRING);
        Observe(ObserveId.Shuffle, "shuffle", mpv_format.MPV_FORMAT_STRING);
        Observe(ObserveId.Playlist, "playlist", mpv_format.MPV_FORMAT_NODE);

        Observe(ObserveId.Speed, "speed", mpv_format.MPV_FORMAT_DOUBLE);

        Observe(ObserveId.Volume, "volume", mpv_format.MPV_FORMAT_DOUBLE);
        Observe(ObserveId.Mute, "mute", mpv_format.MPV_FORMAT_FLAG);

        Observe(ObserveId.MediaTitle, "media-title", mpv_format.MPV_FORMAT_STRING);

        Observe(ObserveId.Fullscreen, "fullscreen", mpv_format.MPV_FORMAT_FLAG);
        Observe(ObserveId.Ontop, "ontop", mpv_format.MPV_FORMAT_FLAG);
        Observe(ObserveId.WindowMinimized, "window-minimized", mpv_format.MPV_FORMAT_FLAG);
        Observe(ObserveId.WindowMaximized, "window-maximized", mpv_format.MPV_FORMAT_FLAG);
        Observe(ObserveId.TitleBar, "title-bar", mpv_format.MPV_FORMAT_FLAG);
        Observe(ObserveId.Border, "border", mpv_format.MPV_FORMAT_FLAG);

        StartEventThread();
    }

    public void Destroy()
    {
        StopEventThread();
        unsafe
        {
            if (_ctx != null)
            {
                PInvoke.mpv_terminate_destroy(ref *_ctx);
                _ctx = null;
            }
        }
        _swapChain = 0;
    }

    private unsafe void CreateContext()
    {
        _ctx = PInvoke.mpv_create();
        if (_ctx == null)
        {
            throw new InvalidOperationException("Failed to create mpv context");
        }
    }

    private unsafe void SetOption(string name, string value)
    {
        if (_ctx == null)
        {
            return;
        }
        PInvoke.mpv_set_option_string(ref *_ctx, name, value);
    }

    private unsafe void Observe(ObserveId id, string name, mpv_format format)
    {
        if (_ctx == null)
        {
            return;
        }
        PInvoke.mpv_observe_property(ref *_ctx, (ulong)id, name, format);
    }

    private void StartEventThread()
    {
        if (_eventThreadRunning)
        {
            return;
        }

        _eventThreadRunning = true;
        _eventThread = new Thread(ProcessEvents)
        {
            IsBackground = true
        };
        _eventThread.Start();
    }

    private void StopEventThread()
    {
        if (!_eventThreadRunning)
        {
            return;
        }

        _eventThreadRunning = false;

        unsafe
        {
            if (_ctx != null)
            {
                PInvoke.mpv_wakeup(_ctx);
            }
        }

        _eventThread?.Join();
        _eventThread = null;
    }

    private unsafe void ProcessEvents()
    {
        while (_eventThreadRunning)
        {
            if (_ctx == null)
            {
                break;
            }

            mpv_event* ev = PInvoke.mpv_wait_event(ref *_ctx, 1.0);
            if (ev->event_id == mpv_event_id.MPV_EVENT_NONE)
            {
                continue;
            }

            if (ev->event_id == mpv_event_id.MPV_EVENT_SHUTDOWN)
            {
                break;
            }

            HandleMpvEvent(ev);
        }
    }

    private unsafe void HandleMpvEvent(mpv_event* ev)
    {
        switch (ev->event_id)
        {
            case mpv_event_id.MPV_EVENT_FILE_LOADED:
                Raise(FileLoaded);
                break;

            case mpv_event_id.MPV_EVENT_START_FILE:
                break;

            case mpv_event_id.MPV_EVENT_PLAYBACK_RESTART:
                Raise(MediaLoaded);
                break;

            case mpv_event_id.MPV_EVENT_END_FILE:
            {
                var endFile = (mpv_event_end_file*)ev->data;
                if (endFile->reason == mpv_end_file_reason.MPV_END_FILE_REASON_EOF)
                {
                    Raise(PlaybackEnded);
                }
                else if (endFile->reason == mpv_end_file_reason.MPV_END_FILE_REASON_ERROR)
                {
                    string message = Utf8ToString(PInvoke.mpv_error_string(endFile->error)) ?? "";
                    Raise(PlaybackFailed, new PlaybackFailedEventArgs(message));
                }
                break;
            }

            case mpv_event_id.MPV_EVENT_SEEK:
                Raise(Seeked);
                break;

            case mpv_event_id.MPV_EVENT_VIDEO_RECONFIG:
            {
                nint swapChain = 0;
                PInvoke.mpv_get_property(ref *_ctx, "display-swapchain", mpv_format.MPV_FORMAT_INT64, &swapChain);
                if (swapChain != _swapChain)
                {
                    _swapChain = swapChain;
                    Raise(VoConfigured);
                }
                break;
            }

            case mpv_event_id.MPV_EVENT_PROPERTY_CHANGE:
            {
                var prop = (mpv_event_property*)ev->data;
                if (prop == null)
                {
                    break;
                }

                switch ((ObserveId)ev->reply_userdata)
                {
                    case ObserveId.CoreIdle:
                        break;

                    case ObserveId.Pause:
                    {
                        int paused = prop->data != null ? *(int*)prop->data : 0;
                        Raise(PlaybackStateChanged, new PlaybackStateChangedEventArgs(paused != 0, false));
                        break;
                    }

                    case ObserveId.Volume:
                    case ObserveId.Mute:
                    {
                        double volume = GetDoubleProperty("volume");
                        bool isMuted = IsStringPropertyEqual("mute", "yes");
                        Raise(VolumeChanged, new VolumeChangedEventArgs(volume, isMuted));
                        break;
                    }

                    case ObserveId.PlaybackTime:
                    case ObserveId.TimePos:
                    case ObserveId.Duration:
                    {
                        double position = GetDoubleProperty("time-pos");
                        double duration = GetDoubleProperty("duration");
                        double percentPos = GetDoubleProperty("percent-pos");
                        Raise(PositionChanged, new PositionChangedEventArgs(position, duration, percentPos));
                        break;
                    }

                    case ObserveId.Speed:
                    {
                        double speed = GetDoubleProperty("speed");
                        Raise(SpeedChanged, new SpeedChangedEventArgs(speed));
                        break;
                    }

                    case ObserveId.CacheSpeed:
                    {
                        long cacheSpeed = GetInt64Property("cache-speed");
                        Raise(NetworkInfoChanged, new NetworkInfoChangedEventArgs(cacheSpeed));
                        break;
                    }

                    case ObserveId.Filename:
                    case ObserveId.MediaTitle:
                    {
                        string filename = GetHStringProperty("filename");
                        string mediaTitle = GetHStringProperty("media-title");
                        Raise(MediaInfoChanged, new MediaInfoChangedEventArgs(filename, mediaTitle));
                        break;
                    }

                    case ObserveId.LoopFile:
                        Raise(LoopFileChanged);
                        break;

                    case ObserveId.LoopPlaylist:
                        Raise(LoopPlaylistChanged);
                        break;

                    case ObserveId.Shuffle:
                        Raise(ShuffleChanged);
                        break;

                    case ObserveId.Playlist:
                        Raise(PlaylistChanged);
                        break;

                    case ObserveId.Aid:
                    case ObserveId.Sid:
                        Raise(TrackChanged);
                        break;

                    case ObserveId.MenuData:
                        break;

                    case ObserveId.TrackListCount:
                    {
                        if (prop->format == mpv_format.MPV_FORMAT_INT64 && prop->data != null)
                        {
                            var count = *(int*)prop->data;
                            Raise(TrackListCountChanged, new TrackListCountChangedEventArgs(count));
                        }
                        break;
                    }

                    case ObserveId.TrackList:
                    {
                        if (prop->format == mpv_format.MPV_FORMAT_NODE && prop->data != null)
                        {
                            Raise(TrackListChanged, new TrackListChangedEventArgs(Array.Empty<MpvTrack>()));
                        }
                        break;
                    }

                    case ObserveId.Fullscreen:
                    case ObserveId.Ontop:
                    case ObserveId.WindowMinimized:
                    case ObserveId.WindowMaximized:
                    case ObserveId.TitleBar:
                    case ObserveId.Border:
                    {
                        bool value = prop->data != null && *(int*)prop->data != 0;
                        string name = Utf8ToString(prop->name) ?? "";
                        Raise(WindowChanged, new WindowChangedEventArgs(name, (int)ev->reply_userdata, value));
                        break;
                    }
                }
                break;
            }
        }
    }

    private static void Raise(Action? handler)
    {
        handler?.Invoke();
    }

    private static void Raise<T>(Action<T>? handler, T args)
    {
        handler?.Invoke(args);
    }

    public unsafe void UpdateSize(uint width, uint height)
    {
        if (_ctx == null)
        {
            return;
        }
        PInvoke.mpv_set_option_string(ref *_ctx, "d3d11-composition-size", $"{width}x{height}");
    }

    public void LoadFile(string url, double position)
    {
        if (_ctx == null)
        {
            return;
        }

        long startSeconds = (long)position;
        if (startSeconds > 0)
        {
            string formatted = string.Format("start={0:00}:{1:00}:{2:00}",
                startSeconds / 3600, startSeconds / 60 % 60, startSeconds % 60);
            RunCommand(["loadfile", url, "replace", "0", formatted]);
        }
        else
        {
            RunCommand(["loadfile", url]);
        }
    }

    public void LoadList(string url)
    {
        if (_ctx == null)
        {
            return;
        }
        RunCommand(["loadlist", url]);
    }

    public void Play()
    {
        if (_ctx == null)
        {
            return;
        }
        RunCommand(["set", "pause", "no"]);
    }

    public void Pause()
    {
        if (_ctx == null)
        {
            return;
        }
        RunCommand(["set", "pause", "yes"]);
    }

    public void Stop()
    {
        if (_ctx == null)
        {
            return;
        }
        RunCommand(["stop"]);
    }

    public void TogglePlayPause()
    {
        if (_ctx == null)
        {
            return;
        }
        RunCommand(["cycle", "pause"]);
    }

    public void Command(IList<string> args)
    {
        if (_ctx == null || args == null)
        {
            return;
        }
        RunCommand(args.ToArray());
    }

    public unsafe void CommandString(string cmd)
    {
        if (_ctx == null)
        {
            return;
        }
        PInvoke.mpv_command_string(ref *_ctx, cmd);
    }

    public string GetWatchHistoryPath()
    {
        if (_ctx == null)
        {
            return "";
        }

        unsafe
        {
            sbyte* raw = null;
            if (PInvoke.mpv_get_property(ref *_ctx, "watch-history-path", mpv_format.MPV_FORMAT_STRING, &raw) < 0 || raw == null)
            {
                return "";
            }

            try
            {
                sbyte* cmd = ToUtf8("expand-path");
                try
                {
                    mpv_node result = default;
                    var argv = new sbyte*[2];
                    argv[0] = cmd;
                    argv[1] = raw;
                    try
                    {
                        fixed (sbyte** p = argv)
                        {
                            if (PInvoke.mpv_command_ret(_ctx, p, &result) >= 0 &&
                                result.format == mpv_format.MPV_FORMAT_STRING &&
                                result.u.@string.Value != null)
                            {
                                return result.u.@string.ToString() ?? "";
                            }
                        }
                    }
                    finally
                    {
                        if (result.format != mpv_format.MPV_FORMAT_NONE)
                        {
                            PInvoke.mpv_free_node_contents(ref result);
                        }
                    }
                    return "";
                }
                finally
                {
                    Marshal.FreeHGlobal((nint)cmd);
                }
            }
            finally
            {
                PInvoke.mpv_free(raw);
            }
        }
    }

    public string GetWatchLaterFolderPath()
    {
        return GetHStringProperty("current-watch-later-dir");
    }

    public bool SaveWatchHistory()
    {
        return GetFlagProperty("save-watch-history");
    }

    public bool IsPaused()
    {
        if (_ctx == null)
        {
            return true;
        }
        return IsStringPropertyEqual("pause", "yes");
    }

    public double Volume()
    {
        if (_ctx == null)
        {
            return 0.0;
        }
        return GetDoubleProperty("volume");
    }

    public void Volume(double value)
    {
        if (_ctx == null)
        {
            return;
        }

        if (value < 0)
        {
            value = 0;
        }
        if (value > 100)
        {
            value = 100;
        }
        SetDoubleProperty("volume", value);
    }

    public bool IsMuted()
    {
        if (_ctx == null)
        {
            return false;
        }
        return IsStringPropertyEqual("mute", "yes");
    }

    public void IsMuted(bool value)
    {
        if (_ctx == null)
        {
            return;
        }
        SetStringProperty("mute", value ? "yes" : "no");
    }

    public double Position()
    {
        if (_ctx == null)
        {
            return 0.0;
        }
        return GetDoubleProperty("time-pos");
    }

    public void Position(double value)
    {
        if (_ctx == null)
        {
            return;
        }
        SetDoubleProperty("time-pos", value);
    }

    public double Duration()
    {
        if (_ctx == null)
        {
            return 0.0;
        }
        return GetDoubleProperty("duration");
    }

    public int CurrentVideoTrack()
    {
        if (_ctx == null)
        {
            return -1;
        }
        return (int)GetInt64Property("vid");
    }

    public void CurrentVideoTrack(int value)
    {
        if (_ctx == null)
        {
            return;
        }
        SetInt64Property("vid", value);
    }

    public int CurrentAudioTrack()
    {
        if (_ctx == null)
        {
            return -1;
        }
        return (int)GetInt64Property("aid");
    }

    public void CurrentAudioTrack(int value)
    {
        if (_ctx == null)
        {
            return;
        }
        SetInt64Property("aid", value);
    }

    public int CurrentSubtitleTrack()
    {
        if (_ctx == null)
        {
            return -1;
        }
        return (int)GetInt64Property("sid");
    }

    public void CurrentSubtitleTrack(int value)
    {
        if (_ctx == null)
        {
            return;
        }
        if (value <= 0)
        {
            SetStringProperty("sid", "no");
        }
        else
        {
            SetInt64Property("sid", value);
        }
    }

    public int CurrentSecondSubtitleTrack()
    {
        if (_ctx == null)
        {
            return -1;
        }
        return (int)GetInt64Property("secondary-sid");
    }

    public void CurrentSecondSubtitleTrack(int value)
    {
        if (_ctx == null)
        {
            return;
        }
        if (value <= 0)
        {
            SetStringProperty("secondary-sid", "no");
        }
        else
        {
            SetInt64Property("secondary-sid", value);
        }
    }

    public void AddSubtitle(string url, bool selected, string title)
    {
        if (_ctx == null)
        {
            return;
        }
        RunCommand(["sub-add", url, selected ? "select" : "auto", title]);
    }

    public double PlaybackSpeed()
    {
        if (_ctx == null)
        {
            return 1.0;
        }
        return GetDoubleProperty("speed");
    }

    public void PlaybackSpeed(double value)
    {
        if (_ctx == null)
        {
            return;
        }
        SetDoubleProperty("speed", value);
    }

    public bool LoopFile()
    {
        if (_ctx == null)
        {
            return false;
        }
        return !IsStringPropertyEqual("loop-file", "no");
    }

    public void LoopFile(bool enabled)
    {
        if (_ctx == null)
        {
            return;
        }
        SetStringProperty("loop-file", enabled ? "inf" : "no");
    }

    public void SetLoopPlaylist(bool enabled)
    {
        if (_ctx == null)
        {
            return;
        }
        SetStringProperty("loop-playlist", enabled ? "inf" : "no");
    }

    public bool LoopPlaylist()
    {
        if (_ctx == null)
        {
            return false;
        }
        return !IsStringPropertyEqual("loop-playlist", "no");
    }

    public void SetShuffle(bool enabled)
    {
        if (_ctx == null)
        {
            return;
        }
        SetStringProperty("shuffle", enabled ? "yes" : "no");
    }

    public bool Shuffle()
    {
        if (_ctx == null)
        {
            return false;
        }
        return !IsStringPropertyEqual("shuffle", "no");
    }

    public void SetAspectRatio(string ratio)
    {
        if (_ctx == null)
        {
            return;
        }
        SetStringProperty("video-aspect-override", ratio);
    }

    public void SetHoverSec(double sec)
    {
        SetDoubleProperty("user-data/osc/hover-sec", sec);
    }

    public unsafe void SetDrawPreview(int x, int y, int w, int h)
    {
        if (_ctx == null)
        {
            return;
        }

        mpv_node node = default;
        mpv_node_list list = default;
        var values = new mpv_node[4];
        var keyPtrs = new sbyte*[4];

        values[0].format = mpv_format.MPV_FORMAT_INT64;
        values[0].u.int64 = x;
        values[1].format = mpv_format.MPV_FORMAT_INT64;
        values[1].u.int64 = y;
        values[2].format = mpv_format.MPV_FORMAT_INT64;
        values[2].u.int64 = w;
        values[3].format = mpv_format.MPV_FORMAT_INT64;
        values[3].u.int64 = h;

        try
        {
            keyPtrs[0] = ToUtf8("x");
            keyPtrs[1] = ToUtf8("y");
            keyPtrs[2] = ToUtf8("w");
            keyPtrs[3] = ToUtf8("h");

            fixed (sbyte** kp = keyPtrs)
            {
                fixed (mpv_node* vp = values)
                {
                    list.num = 4;
                    list.keys = kp;
                    list.values = vp;

                    node.format = mpv_format.MPV_FORMAT_NODE_MAP;
                    node.u.list = &list;

                    PInvoke.mpv_set_property(ref *_ctx, "user-data/osc/draw-preview", mpv_format.MPV_FORMAT_NODE, &node);
                }
            }
        }
        finally
        {
            for (int i = 0; i < 4; i++)
            {
                if (keyPtrs[i] != null)
                {
                    Marshal.FreeHGlobal((nint)keyPtrs[i]);
                }
            }
        }
    }

    public unsafe void ClearPreview()
    {
        if (_ctx == null)
        {
            return;
        }

        mpv_node nullNode = default;
        PInvoke.mpv_set_property(ref *_ctx, "user-data/osc/draw-preview", mpv_format.MPV_FORMAT_NODE, &nullNode);
        PInvoke.mpv_del_property(ref *_ctx, "user-data/osc/hover-sec");
    }

    public void AttachSwapChain(SwapChainPanel panel)
    {
        if (_ctx == null)
        {
            return;
        }

        unsafe
        {
            nint swapChain = 0;
            PInvoke.mpv_get_property(ref *_ctx, "display-swapchain", mpv_format.MPV_FORMAT_INT64, &swapChain);

            if (swapChain != 0)
            {
                NativeInterop.SetInverseScaleMatrix(swapChain, panel.CompositionScaleX, panel.CompositionScaleY);
            }

            NativeInterop.SetSwapChainPanel(panel, swapChain);
        }
    }

    public void UpdateSwapChainScale(float scaleX, float scaleY)
    {
        if (_ctx == null)
        {
            return;
        }

        unsafe
        {
            nint swapChain = 0;
            PInvoke.mpv_get_property(ref *_ctx, "display-swapchain", mpv_format.MPV_FORMAT_INT64, &swapChain);

            if (swapChain != 0 && scaleX > 0 && scaleY > 0)
            {
                NativeInterop.SetInverseScaleMatrix(swapChain, scaleX, scaleY);
            }
        }
    }

    public void UpdateDisplayColorInfo(DisplayColorKind colorKind)
    {
        if (_ctx == null)
        {
            return;
        }

        string cs = colorKind switch
        {
            DisplayColorKind.HDR => "HDR",
            DisplayColorKind.WCG => "WCG",
            _ => "SDR",
        };
        SetStringProperty("user-data/mpvw/color-kind", cs);
    }

    public void UpdateDisplayRefreshRate(int refreshRate)
    {
        if (_ctx == null)
        {
            return;
        }

        SetOption("override-display-fps", refreshRate.ToString());
        SetInt64Property("user-data/mpvw/refresh-rate", refreshRate);
    }

    public IReadOnlyList<MpvPlaylistItem> GetPlaylist()
    {
        var items = new List<MpvPlaylistItem>();
        if (_ctx == null)
        {
            return items;
        }

        unsafe
        {
            mpv_node node = default;
            if (PInvoke.mpv_get_property(ref *_ctx, "playlist", mpv_format.MPV_FORMAT_NODE, &node) < 0)
            {
                return items;
            }

            try
            {
                if (node.format == mpv_format.MPV_FORMAT_NODE_ARRAY && node.u.list != null)
                {
                    var list = node.u.list;
                    for (int i = 0; i < list->num; i++)
                    {
                        mpv_node* entry = &list->values[i];
                        if (entry->format != mpv_format.MPV_FORMAT_NODE_MAP || entry->u.list == null)
                        {
                            continue;
                        }

                        int id = -1;
                        string filename = "";
                        string title = "";
                        bool isCurrent = false;
                        bool isPlaying = false;

                        var entryList = entry->u.list;
                        for (int j = 0; j < entryList->num; j++)
                        {
                            sbyte* key = entryList->keys[j];
                            mpv_node value = entryList->values[j];
                            string? keyName = key == null ? null : Utf8ToString(key);

                            switch (keyName)
                            {
                                case "id" when value.format == mpv_format.MPV_FORMAT_INT64:
                                    id = (int)value.u.int64;
                                    break;
                                case "filename" when value.format == mpv_format.MPV_FORMAT_STRING:
                                    filename = value.u.@string.ToString() ?? "";
                                    break;
                                case "title" when value.format == mpv_format.MPV_FORMAT_STRING:
                                    title = value.u.@string.ToString() ?? "";
                                    break;
                                case "current" when value.format == mpv_format.MPV_FORMAT_FLAG:
                                    isCurrent = value.u.flag != 0;
                                    break;
                                case "playing" when value.format == mpv_format.MPV_FORMAT_FLAG:
                                    isPlaying = value.u.flag != 0;
                                    break;
                            }
                        }

                        items.Add(new MpvPlaylistItem(id, i, filename, title, isCurrent, isPlaying));
                    }
                }
            }
            finally
            {
                PInvoke.mpv_free_node_contents(ref node);
            }
        }

        return items;
    }

    private static unsafe mpv_node* FindMapField(mpv_node* map, string key)
    {
        if (map == null || map->format != mpv_format.MPV_FORMAT_NODE_MAP || map->u.list == null)
        {
            return null;
        }

        var list = map->u.list;
        for (int i = 0; i < list->num; i++)
        {
            sbyte* k = list->keys[i];
            if (k != null && Utf8ToString(k) == key)
            {
                return &list->values[i];
            }
        }
        return null;
    }

    private IReadOnlyList<MpvTrack> GetTracks(string type)
    {
        var tracks = new List<MpvTrack>();
        if (_ctx == null)
        {
            return tracks;
        }

        unsafe
        {
            mpv_node node = default;
            if (PInvoke.mpv_get_property(ref *_ctx, "track-list", mpv_format.MPV_FORMAT_NODE, &node) < 0)
            {
                return tracks;
            }

            try
            {
                if (node.format == mpv_format.MPV_FORMAT_NODE_ARRAY && node.u.list != null)
                {
                    var list = node.u.list;
                    int index = 0;
                    for (int i = 0; i < list->num; i++)
                    {
                        var entry = &list->values[i];
                        if (entry->format != mpv_format.MPV_FORMAT_NODE_MAP)
                        {
                            continue;
                        }

                        var typeField = FindMapField(entry, "type");
                        if (typeField == null || typeField->format != mpv_format.MPV_FORMAT_STRING || typeField->u.@string.Value == null)
                        {
                            continue;
                        }
                        if ((typeField->u.@string.ToString() ?? "") != type)
                        {
                            continue;
                        }

                        index++;
                        int id = -1;
                        string title = "";
                        string lang = "";
                        string codec = "";
                        bool selected = false;
                        bool isDefault = false;

                        var idField = FindMapField(entry, "id");
                        if (idField != null && idField->format == mpv_format.MPV_FORMAT_INT64)
                        {
                            id = (int)idField->u.int64;
                        }

                        var titleField = FindMapField(entry, "title");
                        if (titleField != null && titleField->format == mpv_format.MPV_FORMAT_STRING && titleField->u.@string.Value != null)
                        {
                            title = titleField->u.@string.ToString() ?? "";
                        }

                        var langField = FindMapField(entry, "lang");
                        if (langField != null && langField->format == mpv_format.MPV_FORMAT_STRING && langField->u.@string.Value != null)
                        {
                            lang = langField->u.@string.ToString() ?? "";
                        }

                        var codecField = FindMapField(entry, "codec");
                        if (codecField != null && codecField->format == mpv_format.MPV_FORMAT_STRING && codecField->u.@string.Value != null)
                        {
                            codec = codecField->u.@string.ToString() ?? "";
                        }

                        var selectedField = FindMapField(entry, "selected");
                        if (selectedField != null && selectedField->format == mpv_format.MPV_FORMAT_FLAG)
                        {
                            selected = selectedField->u.flag != 0;
                        }

                        var defaultField = FindMapField(entry, "default");
                        if (defaultField != null && defaultField->format == mpv_format.MPV_FORMAT_FLAG)
                        {
                            isDefault = defaultField->u.flag != 0;
                        }

                        if (type == "audio")
                        {
                            int demuxChannelCount = 0;
                            int demuxSamplerate = 0;
                            var ccField = FindMapField(entry, "demux-channel-count");
                            if (ccField != null && ccField->format == mpv_format.MPV_FORMAT_INT64)
                            {
                                demuxChannelCount = (int)ccField->u.int64;
                            }
                            var srField = FindMapField(entry, "demux-samplerate");
                            if (srField != null && srField->format == mpv_format.MPV_FORMAT_INT64)
                            {
                                demuxSamplerate = (int)srField->u.int64;
                            }

                            tracks.Add(new MpvTrack(index, id, TrackType.Audio, title, lang, selected, codec, isDefault,
                                demuxChannelCount: demuxChannelCount, demuxSamplerate: demuxSamplerate));
                        }
                        else if (type == "video")
                        {
                            int demuxW = 0;
                            int demuxH = 0;
                            double demuxFps = 0;
                            var wField = FindMapField(entry, "demux-w");
                            if (wField != null && wField->format == mpv_format.MPV_FORMAT_INT64)
                            {
                                demuxW = (int)wField->u.int64;
                            }
                            var hField = FindMapField(entry, "demux-h");
                            if (hField != null && hField->format == mpv_format.MPV_FORMAT_INT64)
                            {
                                demuxH = (int)hField->u.int64;
                            }
                            var fpsField = FindMapField(entry, "demux-fps");
                            if (fpsField != null && fpsField->format == mpv_format.MPV_FORMAT_DOUBLE)
                            {
                                demuxFps = fpsField->u.double_;
                            }

                            tracks.Add(new MpvTrack(index, id, TrackType.Video, title, lang, selected, codec, isDefault,
                                demuxW: demuxW, demuxH: demuxH, demuxFps: demuxFps));
                        }
                        else if (type == "sub")
                        {
                            bool isForced = false;
                            bool isExternal = false;
                            var forcedField = FindMapField(entry, "forced");
                            if (forcedField != null && forcedField->format == mpv_format.MPV_FORMAT_FLAG)
                            {
                                isForced = forcedField->u.flag != 0;
                            }
                            var extField = FindMapField(entry, "external");
                            if (extField != null && extField->format == mpv_format.MPV_FORMAT_FLAG)
                            {
                                isExternal = extField->u.flag != 0;
                            }

                            tracks.Add(new MpvTrack(index, id, TrackType.Subtitle, title, lang, selected, codec, isDefault,
                                isForced: isForced, isExternal: isExternal));
                        }
                    }
                }
            }
            finally
            {
                PInvoke.mpv_free_node_contents(ref node);
            }
        }

        return tracks;
    }

    public IReadOnlyList<MpvTrack> GetAudioTracks()
    {
        return GetTracks("audio");
    }

    public IReadOnlyList<MpvTrack> GetVideoTracks()
    {
        return GetTracks("video");
    }

    public IReadOnlyList<MpvTrack> GetSubtitleTracks()
    {
        return GetTracks("sub");
    }

    public IReadOnlyList<MpvChapter> GetChapters()
    {
        var chapters = new List<MpvChapter>();
        if (_ctx == null)
        {
            return chapters;
        }

        unsafe
        {
            mpv_node node = default;
            if (PInvoke.mpv_get_property(ref *_ctx, "chapter-list", mpv_format.MPV_FORMAT_NODE, &node) < 0)
            {
                return chapters;
            }

            try
            {
                if (node.format == mpv_format.MPV_FORMAT_NODE_ARRAY && node.u.list != null)
                {
                    var list = node.u.list;
                    for (int i = 0; i < list->num; i++)
                    {
                        mpv_node* entry = &list->values[i];
                        if (entry->format != mpv_format.MPV_FORMAT_NODE_MAP || entry->u.list == null)
                        {
                            continue;
                        }

                        int id = i;
                        string title = "";
                        double time = 0;

                        var entryList = entry->u.list;
                        for (int j = 0; j < entryList->num; j++)
                        {
                            sbyte* key = entryList->keys[j];
                            mpv_node value = entryList->values[j];
                            string? keyName = key == null ? null : Utf8ToString(key);

                            if (keyName == "title" && value.format == mpv_format.MPV_FORMAT_STRING)
                            {
                                title = value.u.@string.ToString() ?? "";
                            }
                            else if (keyName == "time" && value.format == mpv_format.MPV_FORMAT_DOUBLE)
                            {
                                time = value.u.double_;
                            }
                        }

                        chapters.Add(new MpvChapter(id, title, time));
                    }
                }
            }
            finally
            {
                PInvoke.mpv_free_node_contents(ref node);
            }
        }

        return chapters;
    }

    public IReadOnlyList<MpvEdition> GetEditions()
    {
        var editions = new List<MpvEdition>();
        if (_ctx == null)
        {
            return editions;
        }

        unsafe
        {
            mpv_node node = default;
            if (PInvoke.mpv_get_property(ref *_ctx, "edition-list", mpv_format.MPV_FORMAT_NODE, &node) < 0)
            {
                return editions;
            }

            try
            {
                if (node.format == mpv_format.MPV_FORMAT_NODE_ARRAY && node.u.list != null)
                {
                    var list = node.u.list;
                    for (int i = 0; i < list->num; i++)
                    {
                        mpv_node* entry = &list->values[i];
                        if (entry->format != mpv_format.MPV_FORMAT_NODE_MAP || entry->u.list == null)
                        {
                            continue;
                        }

                        int id = i;
                        string title = "";

                        var entryList = entry->u.list;
                        for (int j = 0; j < entryList->num; j++)
                        {
                            sbyte* key = entryList->keys[j];
                            mpv_node value = entryList->values[j];
                            string? keyName = key == null ? null : Utf8ToString(key);

                            if (keyName == "title" && value.format == mpv_format.MPV_FORMAT_STRING)
                            {
                                title = value.u.@string.ToString() ?? "";
                            }
                        }

                        editions.Add(new MpvEdition(id, title));
                    }
                }
            }
            finally
            {
                PInvoke.mpv_free_node_contents(ref node);
            }
        }

        return editions;
    }

    public IReadOnlyList<MpvAudioDevice> GetAudioDevices()
    {
        var devices = new List<MpvAudioDevice>();
        if (_ctx == null)
        {
            return devices;
        }

        unsafe
        {
            mpv_node node = default;
            if (PInvoke.mpv_get_property(ref *_ctx, "audio-device-list", mpv_format.MPV_FORMAT_NODE, &node) < 0)
            {
                return devices;
            }

            try
            {
                if (node.format == mpv_format.MPV_FORMAT_NODE_ARRAY && node.u.list != null)
                {
                    var list = node.u.list;
                    for (int i = 0; i < list->num; i++)
                    {
                        mpv_node* entry = &list->values[i];
                        if (entry->format != mpv_format.MPV_FORMAT_NODE_MAP || entry->u.list == null)
                        {
                            continue;
                        }

                        string name = "";
                        string description = "";

                        var entryList = entry->u.list;
                        for (int j = 0; j < entryList->num; j++)
                        {
                            sbyte* key = entryList->keys[j];
                            mpv_node value = entryList->values[j];
                            string? keyName = key == null ? null : Utf8ToString(key);

                            if (keyName == "name" && value.format == mpv_format.MPV_FORMAT_STRING)
                            {
                                name = value.u.@string.ToString() ?? "";
                            }
                            else if (keyName == "description" && value.format == mpv_format.MPV_FORMAT_STRING)
                            {
                                description = value.u.@string.ToString() ?? "";
                            }
                        }

                        devices.Add(new MpvAudioDevice(name, description));
                    }
                }
            }
            finally
            {
                PInvoke.mpv_free_node_contents(ref node);
            }
        }

        return devices;
    }

    public IReadOnlyList<MpvProfile> GetProfiles()
    {
        var profiles = new List<MpvProfile>();
        if (_ctx == null)
        {
            return profiles;
        }

        unsafe
        {
            mpv_node node = default;
            if (PInvoke.mpv_get_property(ref *_ctx, "profile-list", mpv_format.MPV_FORMAT_NODE, &node) < 0)
            {
                return profiles;
            }

            try
            {
                if (node.format == mpv_format.MPV_FORMAT_NODE_ARRAY && node.u.list != null)
                {
                    var list = node.u.list;
                    for (int i = 0; i < list->num; i++)
                    {
                        mpv_node* entry = &list->values[i];
                        if (entry->format != mpv_format.MPV_FORMAT_NODE_MAP || entry->u.list == null)
                        {
                            continue;
                        }

                        string name = "";

                        var entryList = entry->u.list;
                        for (int j = 0; j < entryList->num; j++)
                        {
                            sbyte* key = entryList->keys[j];
                            mpv_node value = entryList->values[j];
                            string? keyName = key == null ? null : Utf8ToString(key);

                            if (keyName == "name" && value.format == mpv_format.MPV_FORMAT_STRING)
                            {
                                name = value.u.@string.ToString() ?? "";
                            }
                        }

                        profiles.Add(new MpvProfile(name));
                    }
                }
            }
            finally
            {
                PInvoke.mpv_free_node_contents(ref node);
            }
        }

        return profiles;
    }

    public int CurrentChapter()
    {
        return (int)GetInt64Property("chapter");
    }

    public int CurrentEdition()
    {
        return (int)GetInt64Property("edition");
    }

    private static unsafe IReadOnlyList<MpvMenuItem> ParseMenuNode(mpv_node* node)
    {
        var items = new List<MpvMenuItem>();
        if (node == null || node->format != mpv_format.MPV_FORMAT_NODE_ARRAY || node->u.list == null)
        {
            return items;
        }

        var list = node->u.list;
        for (int i = 0; i < list->num; i++)
        {
            var itemNode = &list->values[i];
            if (itemNode->format != mpv_format.MPV_FORMAT_NODE_MAP || itemNode->u.list == null)
            {
                continue;
            }

            string title = "";
            string command = "";
            string type = "command";
            bool isChecked = false;
            bool isDisabled = false;
            bool isHidden = false;
            mpv_node* subItemsNode = null;

            var itemList = itemNode->u.list;
            for (int j = 0; j < itemList->num; j++)
            {
                sbyte* key = itemList->keys[j];
                mpv_node* val = &itemList->values[j];
                string? keyName = key == null ? null : Utf8ToString(key);

                if (keyName == "title" && val->format == mpv_format.MPV_FORMAT_STRING)
                {
                    title = val->u.@string.ToString() ?? "";
                }
                else if (keyName == "cmd" && val->format == mpv_format.MPV_FORMAT_STRING)
                {
                    command = val->u.@string.ToString() ?? "";
                }
                else if (keyName == "type" && val->format == mpv_format.MPV_FORMAT_STRING)
                {
                    type = val->u.@string.ToString() ?? "";
                }
                else if (keyName == "state" && val->format == mpv_format.MPV_FORMAT_NODE_ARRAY && val->u.list != null)
                {
                    var stateList = val->u.list;
                    for (int k = 0; k < stateList->num; k++)
                    {
                        if (stateList->values[k].format == mpv_format.MPV_FORMAT_STRING)
                        {
                            string s = stateList->values[k].u.@string.ToString() ?? "";
                            if (s == "checked")
                            {
                                isChecked = true;
                            }
                            if (s == "disabled")
                            {
                                isDisabled = true;
                            }
                            if (s == "hidden")
                            {
                                isHidden = true;
                            }
                        }
                    }
                }
                else if (keyName == "submenu")
                {
                    subItemsNode = val;
                }
            }

            var subItems = subItemsNode != null
                ? ParseMenuNode(subItemsNode)
                : Array.Empty<MpvMenuItem>();

            items.Add(new MpvMenuItem(title, command, type, isChecked, isDisabled, isHidden, subItems));
        }

        return items;
    }

    public string GetSubtitleExtensions()
    {
        return GetHStringProperty("sub-auto-exts");
    }

    public IReadOnlyList<MpvMenuItem> GetMenu()
    {
        if (_ctx == null)
        {
            return Array.Empty<MpvMenuItem>();
        }

        unsafe
        {
            mpv_node node = default;
            if (PInvoke.mpv_get_property(ref *_ctx, "menu-data", mpv_format.MPV_FORMAT_NODE, &node) < 0)
            {
                return Array.Empty<MpvMenuItem>();
            }

            var result = ParseMenuNode(&node);
            PInvoke.mpv_free_node_contents(ref node);
            return result;
        }
    }

    private unsafe double GetDoubleProperty(string name)
    {
        if (_ctx == null)
        {
            return 0.0;
        }

        double value = 0.0;
        PInvoke.mpv_get_property(ref *_ctx, name, mpv_format.MPV_FORMAT_DOUBLE, &value);
        return value;
    }

    private unsafe long GetInt64Property(string name)
    {
        if (_ctx == null)
        {
            return 0;
        }

        long value = 0;
        PInvoke.mpv_get_property(ref *_ctx, name, mpv_format.MPV_FORMAT_INT64, &value);
        return value;
    }

    private unsafe string GetHStringProperty(string name)
    {
        if (_ctx == null)
        {
            return "";
        }

        sbyte* value = null;
        if (PInvoke.mpv_get_property(ref *_ctx, name, mpv_format.MPV_FORMAT_STRING, &value) >= 0 && value != null)
        {
            string result = Utf8ToString(value) ?? "";
            PInvoke.mpv_free(value);
            return result;
        }
        return "";
    }

    private unsafe bool GetFlagProperty(string name)
    {
        if (_ctx == null)
        {
            return false;
        }

        int flag = 0;
        return PInvoke.mpv_get_property(ref *_ctx, name, mpv_format.MPV_FORMAT_FLAG, &flag) >= 0 && flag != 0;
    }

    private unsafe bool IsStringPropertyEqual(string name, string expected)
    {
        if (_ctx == null)
        {
            return false;
        }

        sbyte* value = null;
        if (PInvoke.mpv_get_property(ref *_ctx, name, mpv_format.MPV_FORMAT_STRING, &value) < 0 || value == null)
        {
            return false;
        }

        bool isEqual = Utf8ToString(value) == expected;
        PInvoke.mpv_free(value);
        return isEqual;
    }

    private unsafe void SetDoubleProperty(string name, double value)
    {
        if (_ctx == null)
        {
            return;
        }

        double v = value;
        PInvoke.mpv_set_property(ref *_ctx, name, mpv_format.MPV_FORMAT_DOUBLE, &v);
    }

    private unsafe void SetInt64Property(string name, long value)
    {
        if (_ctx == null)
        {
            return;
        }

        long v = value;
        PInvoke.mpv_set_property(ref *_ctx, name, mpv_format.MPV_FORMAT_INT64, &v);
    }

    private unsafe void SetStringProperty(string name, string value)
    {
        if (_ctx == null)
        {
            return;
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(value + "\0");
        fixed (byte* p = bytes)
        {
            sbyte* str = (sbyte*)p;
            PInvoke.mpv_set_property(ref *_ctx, name, mpv_format.MPV_FORMAT_STRING, &str);
        }
    }

    private unsafe void RunCommand(string[] args)
    {
        if (_ctx == null || args == null || args.Length == 0)
        {
            return;
        }

        var argv = new sbyte*[args.Length + 1];
        try
        {
            for (int i = 0; i < args.Length; i++)
            {
                argv[i] = ToUtf8(args[i]);
            }
            argv[args.Length] = null;

            fixed (sbyte** p = argv)
            {
                PInvoke.mpv_command(_ctx, p);
            }
        }
        finally
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (argv[i] != null)
                {
                    Marshal.FreeHGlobal((nint)argv[i]);
                }
            }
        }
    }

    private static unsafe sbyte* ToUtf8(string s)
    {
        nint p = Marshal.StringToCoTaskMemUTF8(s);
        return (sbyte*)p;
    }

    private static unsafe string? Utf8ToString(sbyte* p)
    {
        if (p == null)
        {
            return null;
        }
        return new string(p);
    }

    private static unsafe string? Utf8ToString(PCSTR p)
    {
        if (p.Value == null)
        {
            return null;
        }
        return p.ToString();
    }
}
