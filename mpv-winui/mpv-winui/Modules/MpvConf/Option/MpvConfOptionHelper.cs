using mpv_winui.Modules.MpvConf.Schema;
using System;
using System.Globalization;
using System.Linq;

namespace mpv_winui.Modules.MpvConf.Option;

public enum MpvOptionEditorKind
{
    Text,
    Bool,
    Int,
    Float,
    Enum,
}

public static class MpvConfOptionHelper
{
    public const string RawTypeName = "raw";

    private static readonly string[] TrueWords = ["yes"];
    private static readonly string[] FalseWords = ["no"];

    public static MpvOptionEditorKind ResolveEditorKind(MpvConfSchemaItemValue? type)
    {
        if (type is null)
        {
            return MpvOptionEditorKind.Text;
        }

        if (type.HasEnum)
        {
            return MpvOptionEditorKind.Enum;
        }

        return type.Type switch
        {
            MpvConfSchemaItemValue.Bool => MpvOptionEditorKind.Bool,
            MpvConfSchemaItemValue.Int => MpvOptionEditorKind.Int,
            MpvConfSchemaItemValue.Float => MpvOptionEditorKind.Float,
            _ => MpvOptionEditorKind.Text,
        };
    }

    public static bool? ParseBool(string? value)
    {
        string v = (value ?? string.Empty).Trim();
        if (v.Length == 0)
        {
            return null;
        }

        if (TrueWords.Contains(v, StringComparer.OrdinalIgnoreCase))
        {
            return true;
        }

        if (FalseWords.Contains(v, StringComparer.OrdinalIgnoreCase))
        {
            return false;
        }

        return null;
    }

    public static string FormatBool(bool value) => value ? "yes" : "no";

    public static double ParseInt(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return double.NaN;
        }

        if (long.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out long v))
        {
            return v;
        }

        return double.NaN;
    }

    public static double ParseFloat(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return double.NaN;
        }

        if (double.TryParse(value.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double v))
        {
            return v;
        }

        return double.NaN;
    }

    public static string FormatNumber(double value) => value.ToString(CultureInfo.InvariantCulture);
}
