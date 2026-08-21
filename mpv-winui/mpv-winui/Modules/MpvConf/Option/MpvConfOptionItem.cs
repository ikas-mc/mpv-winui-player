using mpv_winui.Modules.MpvConf.Conf;
using mpv_winui.Modules.MpvConf.Schema;

namespace mpv_winui.Modules.MpvConf.Option;

public enum MpvOptionState
{
    Enabled,
    Disabled,
    NotInFile,
}

public sealed class MpvConfEnumItem
{
    public MpvConfEnumItem(string value, string label)
    {
        Value = value;
        Label = label;
    }

    public string Value
    {
        get;
    }

    public string Label
    {
        get;
    }

    public override string ToString() => Label;
}

public sealed class MpvConfOptionItem
{
    public MpvConfOptionItem(string profile, MpvConfSchemaItem? definition, MpvConfLine? line, MpvConfLine? deletedLine = null)
    {
        Profile = profile;
        Definition = definition;
        Line = line;
        DeletedLine = deletedLine;
    }

    public string Profile
    {
        get;
    }

    public MpvConfSchemaItem? Definition
    {
        get;
    }

    public MpvConfLine? Line
    {
        get;
        set;
    }

    public MpvConfLine? DeletedLine
    {
        get;
        set;
    }

    public string? PendingValue
    {
        get;
        set;
    }

    public bool Present => Line is not null;

    public bool IsModified => Line?.IsDirty ?? DeletedLine is not null;

    public string Key => Line?.Key ?? DeletedLine?.Key ?? Definition?.Name ?? string.Empty;

    public bool IsKnown => Definition is not null;

    public string Group => Definition?.Group ?? MpvConfOptionService.UnknownGroup;

    public string Description => Definition?.Description ?? string.Empty;

    public string? Link => Definition?.Link;

    public MpvOptionState State => Line is null ? MpvOptionState.NotInFile : (Line.Enabled ? MpvOptionState.Enabled : MpvOptionState.Disabled);

    public string Value => Line?.Value ?? PendingValue ?? DeletedLine?.Value ?? Definition?.DefaultValue ?? string.Empty;
}
