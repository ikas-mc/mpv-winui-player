namespace mpv_winrt;

public enum DisplayColorKind
{
    SDR = 0,
    WCG = 1,
    HDR = 2
}

public enum TrackType
{
    Unknown = 0,
    Video = 1,
    Audio = 2,
    Subtitle = 3
}

public sealed class MpvPlaylistItem
{
    public MpvPlaylistItem(int id, int index, string filename, string title, bool isCurrent, bool isPlaying)
    {
        Id = id;
        Index = index;
        Filename = filename;
        Title = title;
        IsCurrent = isCurrent;
        IsPlaying = isPlaying;
    }

    public int Id { get; }
    public int Index { get; }
    public string Filename { get; }
    public string Title { get; }
    public bool IsCurrent { get; }
    public bool IsPlaying { get; }
}

public sealed class MpvTrack
{
    public MpvTrack(int index, int id, TrackType type, string title, string lang, bool selected, string codec, bool isDefault,
        bool isForced = false, bool isExternal = false,
        int demuxW = 0, int demuxH = 0, double demuxFps = 0,
        int demuxChannelCount = 0, int demuxSamplerate = 0)
    {
        Index = index;
        Id = id;
        Type = type;
        Title = title;
        Lang = lang;
        Selected = selected;
        Codec = codec;
        IsDefault = isDefault;
        IsForced = isForced;
        IsExternal = isExternal;
        DemuxW = demuxW;
        DemuxH = demuxH;
        DemuxFps = demuxFps;
        DemuxChannelCount = demuxChannelCount;
        DemuxSamplerate = demuxSamplerate;
    }

    public int Index { get; }
    public int Id { get; }
    public TrackType Type { get; }
    public string Title { get; }
    public string Lang { get; }
    public bool Selected { get; }
    public string Codec { get; }
    public bool IsDefault { get; }
    public bool IsForced { get; }
    public bool IsExternal { get; }
    public int DemuxW { get; }
    public int DemuxH { get; }
    public double DemuxFps { get; }
    public int DemuxChannelCount { get; }
    public int DemuxSamplerate { get; }
}

public sealed class MpvChapter
{
    public MpvChapter(int id, string title, double time)
    {
        Id = id;
        Title = title;
        Time = time;
    }

    public int Id { get; }
    public string Title { get; }
    public double Time { get; }
}

public sealed class MpvEdition
{
    public MpvEdition(int id, string title)
    {
        Id = id;
        Title = title;
    }

    public int Id { get; }
    public string Title { get; }
}

public sealed class MpvAudioDevice
{
    public MpvAudioDevice(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public string Name { get; }
    public string Description { get; }
}

public sealed class MpvProfile
{
    public MpvProfile(string name)
    {
        Name = name;
    }

    public string Name { get; }
}

public sealed class MpvMenuItem
{
    public MpvMenuItem(string title, string command, string type, bool isChecked, bool isDisabled, bool isHidden,
        IReadOnlyList<MpvMenuItem> items)
    {
        Title = title;
        Command = command;
        Type = type;
        IsChecked = isChecked;
        IsDisabled = isDisabled;
        IsHidden = isHidden;
        Items = items;
    }

    public string Title { get; }
    public string Command { get; }
    public string Type { get; }
    public bool IsChecked { get; }
    public bool IsDisabled { get; }
    public bool IsHidden { get; }
    public IReadOnlyList<MpvMenuItem> Items { get; }
}

public sealed class PlaybackStateChangedEventArgs
{
    public PlaybackStateChangedEventArgs(bool isPaused, bool isIdle)
    {
        IsPaused = isPaused;
        IsIdle = isIdle;
    }

    public bool IsPaused { get; }
    public bool IsIdle { get; }
}

public sealed class VolumeChangedEventArgs
{
    public VolumeChangedEventArgs(double volume, bool isMuted)
    {
        Volume = volume;
        IsMuted = isMuted;
    }

    public double Volume { get; }
    public bool IsMuted { get; }
}

public sealed class PositionChangedEventArgs
{
    public PositionChangedEventArgs(double position, double duration, double percentPosition)
    {
        Position = position;
        Duration = duration;
        PercentPosition = percentPosition;
    }

    public double Position { get; }
    public double Duration { get; }
    public double PercentPosition { get; }
}

public sealed class SpeedChangedEventArgs
{
    public SpeedChangedEventArgs(double speed)
    {
        Speed = speed;
    }

    public double Speed { get; }
}

public sealed class MediaInfoChangedEventArgs
{
    public MediaInfoChangedEventArgs(string filename, string mediaTitle)
    {
        Filename = filename;
        MediaTitle = mediaTitle;
    }

    public string Filename { get; }
    public string MediaTitle { get; }
}

public sealed class NetworkInfoChangedEventArgs
{
    public NetworkInfoChangedEventArgs(long cacheSpeed)
    {
        CacheSpeed = cacheSpeed;
    }

    public long CacheSpeed { get; }
}

public sealed class PlaybackFailedEventArgs
{
    public PlaybackFailedEventArgs(string message)
    {
        Message = message;
    }

    public string Message { get; }
}

public sealed class TrackListChangedEventArgs
{
    public TrackListChangedEventArgs(IReadOnlyList<MpvTrack> tracks)
    {
        Tracks = tracks;
    }

    public IReadOnlyList<MpvTrack> Tracks { get; }
}

public sealed class TrackListCountChangedEventArgs
{
    public TrackListCountChangedEventArgs(int count)
    {
        Count = count;
    }

    public int Count { get; }
}

public sealed class WindowChangedEventArgs
{
    public WindowChangedEventArgs(string propertyName, int propertyId, bool value)
    {
        PropertyName = propertyName;
        PropertyId = propertyId;
        Value = value;
    }

    public string PropertyName { get; }
    public int PropertyId { get; }
    public bool Value { get; }
}
