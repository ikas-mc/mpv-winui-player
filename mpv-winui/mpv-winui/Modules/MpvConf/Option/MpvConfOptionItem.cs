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
    private readonly MpvConfManager _manager;
    private readonly string _profile;
    private MpvConfLine? _line;
    private MpvConfLine? _deletedLine;
    private string? _pendingValue;

    public MpvConfOptionItem(MpvConfManager manager, string profile, MpvConfSchemaItem? definition, MpvConfLine? line, MpvConfLine? deletedLine = null)
    {
        _manager = manager;
        _profile = profile;
        Definition = definition;
        _line = line;
        _deletedLine = deletedLine;
    }

    public MpvConfSchemaItem? Definition
    {
        get;
    }

    public MpvConfLine? Line => _line;

    public bool Present => _line is not null;

    public bool IsModified => _line?.IsDirty ?? _deletedLine is not null;

    public string Key => _line?.Key ?? _deletedLine?.Key ?? Definition?.Name ?? string.Empty;

    public bool IsKnown => Definition is not null;

    public string Group => Definition?.Group ?? MpvConfOptionService.UnknownGroup;

    public string Profile => _profile;

    public string Description => Definition?.Description ?? string.Empty;

    public string? Link => Definition?.Link;

    public MpvOptionState State
    {
        get => _line is null ? MpvOptionState.NotInFile : (_line.Enabled ? MpvOptionState.Enabled : MpvOptionState.Disabled);
        set
        {
            if (value == State)
            {
                return;
            }

            switch (value)
            {
                case MpvOptionState.Enabled:
                    EnsureLine(enabled: true);
                    break;
                case MpvOptionState.Disabled:
                    EnsureLine(enabled: false);
                    break;
                case MpvOptionState.NotInFile:
                    RemoveLine();
                    break;
            }
        }
    }

    public string Value
    {
        get => _line?.Value ?? _pendingValue ?? _deletedLine?.Value ?? Definition?.DefaultValue ?? string.Empty;
        set
        {
            string v = value ?? string.Empty;
            if (_line is not null)
            {
                _line.Value = v;
                _line.Modified = true;
            }
            else
            {
                _pendingValue = v;
            }
        }
    }

    private void EnsureLine(bool enabled)
    {
        if (_deletedLine is not null)
        {
            if (_manager.Restore(_deletedLine))
            {
                _line = _deletedLine;
                _deletedLine = null;

                if (_pendingValue is not null)
                {
                    _line.Value = _pendingValue;
                    _line.Modified = true;
                    _pendingValue = null;
                }
            }
            else
            {
                _deletedLine = null;
            }
        }

        if (_line is not null)
        {
            if (_line.Enabled != enabled)
            {
                _line.Enabled = enabled;
                _line.Modified = true;
            }

            return;
        }

        string value = CurrentValue();
        _line = enabled ? _manager.InsertOption(Key, value, _profile) : _manager.InsertDisabled(Key, value, _profile);
    }

    private string CurrentValue()
    {
        return _pendingValue ?? Definition?.DefaultValue ?? string.Empty;
    }

    private void RemoveLine()
    {
        if (_line is not null)
        {
            bool tombstone = _line.Status != MpvConfLineStatus.Added;
            _manager.Remove(_line);
            _deletedLine = tombstone ? _line : null;
        }

        _pendingValue = null;
        _line = null;
    }
}
