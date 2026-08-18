using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace mpv_winui.Modules.MpvConf.Schema;

public sealed class MpvConfSchemaItemValue
{
    public const string Raw = "raw";
    public const string String = "string";
    public const string Int = "int";
    public const string Float = "float";

    public const string Bool = "bool";

    // Schema-only marker: values using it fall back to the text editor kind.
    public const string Array = "array";

    [JsonPropertyName("type")]
    public string Type
    {
        get;
        set;
    } = Raw;

    [JsonPropertyName("minimum")]
    public double? Minimum
    {
        get;
        set;
    }

    [JsonPropertyName("maximum")]
    public double? Maximum
    {
        get;
        set;
    }

    [JsonPropertyName("enum")]
    public IReadOnlyList<string>? EnumValues
    {
        get;
        set;
    }

    public bool HasEnum => EnumValues is { Count: > 0 };
}
