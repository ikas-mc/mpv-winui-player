using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace mpv_winui.Modules.MpvConf.Schema;

public sealed class MpvConfSchemaItem
{
    [JsonPropertyName("name")]
    public string Name
    {
        get;
        set;
    } = string.Empty;

    [JsonPropertyName("group")]
    public string Group
    {
        get;
        set;
    } = "General";

    [JsonPropertyName("desc")]
    public string Description
    {
        get;
        set;
    } = string.Empty;

    [JsonPropertyName("link")]
    public string Link
    {
        get;
        set;
    } = string.Empty;

    [JsonPropertyName("deprecated")]
    public bool Deprecated
    {
        get;
        set;
    }

    [JsonPropertyName("default")]
    public string? DefaultValue
    {
        get;
        set;
    }

    [JsonPropertyName("values")]
    public IReadOnlyList<MpvConfSchemaItemValue> Values
    {
        get => field ?? (MpvConfSchemaItemValue[])[];
        set;
    }

}
