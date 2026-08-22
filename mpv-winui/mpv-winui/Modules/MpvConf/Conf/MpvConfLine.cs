using System;
using System.Text;

namespace mpv_winui.Modules.MpvConf.Conf;

public enum MpvConfLineType
{
    Blank,
    Comment,
    Section,
    Option,
    Invalid
}

public enum MpvConfLineStatus
{
    Existing,
    Added,
    Deleted
}

public sealed class MpvConfLine
{
    private string _raw;
    private string? _value;
    private bool _enabled;

    private readonly char? _quoteChar;
    private readonly string _inlineComment;

    private MpvConfLine(MpvConfLineType type, string raw, string section, string? key = null, string? value = null, bool enabled = false, char? quoteChar = null, string inlineComment = "")
    {
        Type = type;
        _raw = raw;
        Section = section;
        Key = key;
        _value = value;
        _enabled = enabled;
        _quoteChar = quoteChar;
        _inlineComment = inlineComment;
    }

    public static MpvConfLine Blank(string raw, string section) => new(MpvConfLineType.Blank, raw, section);

    public static MpvConfLine Comment(string raw, string section) => new(MpvConfLineType.Comment, raw, section);

    public static MpvConfLine SectionLine(string raw, string name) => new(MpvConfLineType.Section, raw, name);

    public static MpvConfLine Invalid(string raw, string section) => new(MpvConfLineType.Invalid, raw, section);

    public static MpvConfLine Option(string raw, string section, string key, string value, bool enabled, char? quoteChar, string inlineComment)
    {
        var line = new MpvConfLine(MpvConfLineType.Option, raw, section, key, value, enabled, quoteChar, inlineComment);
        if (raw.Length == 0)
        {
            line.Status = MpvConfLineStatus.Added;
            line._raw = line.BuildRaw();
        }

        return line;
    }

    public MpvConfLineType Type
    {
        get;
    }

    public MpvConfLineStatus Status
    {
        get;
        set;
    }

    public bool IsDirty => Modified || Status is MpvConfLineStatus.Added or MpvConfLineStatus.Deleted;

    public string Section
    {
        get;
        internal set;
    }

    public bool SectionDeleted
    {
        get;
        internal set;
    }

    public string? Key
    {
        get;
    }

    public bool IsOption => Type == MpvConfLineType.Option;

    public int LineNumber
    {
        get;
        internal set;
    }

    public string Raw => _raw;

    public bool Modified
    {
        get;
        set;
    }

    public string? Value
    {
        get => _value;
        set
        {
            if (Type != MpvConfLineType.Option)
            {
                throw new InvalidOperationException("Cannot set value on a non-option line.");
            }

            _value = value;
            _raw = BuildRaw();
        }
    }

    public bool Enabled
    {
        get => _enabled;
        set
        {
            if (Type != MpvConfLineType.Option)
            {
                throw new InvalidOperationException("Cannot enable/disable a non-option line.");
            }

            _enabled = value;
            _raw = BuildRaw();
        }
    }

    public override string ToString() => _raw;

    internal void RetargetSection(string newSection)
    {
        Section = newSection;
        if (Type == MpvConfLineType.Section)
        {
            int open = _raw.IndexOf('[');
            int close = open >= 0 ? _raw.IndexOf(']', open) : -1;
            if (open >= 0 && close > open)
            {
                _raw = _raw.Substring(0, open + 1) + newSection + _raw.Substring(close);
            }
        }
    }

    private string BuildRaw()
    {
        string value = _value ?? string.Empty;

        string body = (Key ?? string.Empty) + "=" + EncodeValue(value) + _inlineComment;

        return _enabled ? body : "# " + body;
    }

    private string EncodeValue(string value)
    {
        if (_quoteChar is { } original && value.IndexOf(original) < 0)
        {
            return original + value + original;
        }

        if (_quoteChar is null && !NeedsQuoting(value))
        {
            return value;
        }

        if (value.IndexOf('"') < 0)
        {
            return "\"" + value + "\"";
        }

        if (value.IndexOf('\'') < 0)
        {
            return "'" + value + "'";
        }

        return "%" + Encoding.UTF8.GetByteCount(value) + "%" + value;
    }

    private static bool NeedsQuoting(string value)
    {
        if (value.Length == 0)
        {
            return true;
        }

        return value[0] is '"' or '\'' or '%' || value.IndexOf('#') >= 0 || char.IsWhiteSpace(value[0]) || char.IsWhiteSpace(value[value.Length - 1]);
    }
}
