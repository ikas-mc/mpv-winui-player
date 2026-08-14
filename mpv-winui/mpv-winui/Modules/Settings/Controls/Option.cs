using System;
using System.Collections.Generic;

namespace mpv_winui.Modules.Settings.Controls;

public sealed class Option
{
    public const string GroupOtherKey = "other";

    public string Key
    {
        get; set;
    } = string.Empty;

    public string Label
    {
        get; set;
    } = string.Empty;

    public string GroupKey
    {
        get; set;
    } = GroupOtherKey;

    public string GroupLabel
    {
        get; set;
    } = string.Empty;

    public string? Description
    {
        get; set;
    }

    public string? Icon
    {
        get; set;
    }

    public OptionType Type
    {
        get; set;
    }

    public double? Min
    {
        get; set;
    }
    public double? Max
    {
        get; set;
    }
    public double? Step
    {
        get; set;
    }

    public IList<string>? Options
    {
        get; set;
    }

    public bool AllowEmpty
    {
        get; set;
    }

    public Func<object>? Getter
    {
        get; set;
    }

    public Action<object>? Setter
    {
        get; set;
    }
}