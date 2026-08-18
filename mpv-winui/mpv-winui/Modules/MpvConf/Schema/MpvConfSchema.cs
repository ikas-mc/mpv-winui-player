using System;
using System.Collections.Generic;
using System.Linq;

namespace mpv_winui.Modules.MpvConf.Schema;

public sealed class MpvConfSchema
{
    private readonly IReadOnlyDictionary<string, MpvConfSchemaItem> _options;
    private readonly IReadOnlyList<MpvConfSchemaItem> _ordered;

    internal MpvConfSchema(IReadOnlyDictionary<string, MpvConfSchemaItem> options, IReadOnlyList<MpvConfSchemaItem> ordered)
    {
        _options = options;
        _ordered = ordered;
    }

    public static MpvConfSchema Empty
    {
        get;
    } = new(new Dictionary<string, MpvConfSchemaItem>(StringComparer.Ordinal), Array.Empty<MpvConfSchemaItem>());

    public int Count => _options.Count;

    public IReadOnlyDictionary<string, MpvConfSchemaItem> Options => _options;

    public IReadOnlyList<MpvConfSchemaItem> OrderedOptions => _ordered;

    public IReadOnlyList<string> Groups => _ordered.Select(d => d.Group).Distinct(StringComparer.Ordinal).ToList();

    public MpvConfSchemaItem? Get(string key) => _options.TryGetValue(key, out MpvConfSchemaItem? def) ? def : null;
}
