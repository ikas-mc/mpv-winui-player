using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace mpv_winui.Modules.MpvConf.Schema;

public static class MpvConfSchemaService
{
    public const string DefinitionDirectoryName = "mpv-conf-editor";

    public static MpvConfSchema LoadFromJson(string json)
    {
        var defs = JsonSerializer.Deserialize(json, MpvConfSchemaJsonContext.Default.ListMpvConfSchemaItem);

        var options = new Dictionary<string, MpvConfSchemaItem>(StringComparer.Ordinal);
        var ordered = new List<MpvConfSchemaItem>();
        if (defs is { } list)
        {
            foreach (MpvConfSchemaItem def in list)
            {
                if (options.ContainsKey(def.Name))
                {
                    continue;
                }

                options[def.Name] = def;
                ordered.Add(def);
            }
        }

        return new MpvConfSchema(options, ordered);
    }

    public static MpvConfSchema LoadFromFile(string path)
    {
        try
        {
            return LoadFromJson(File.ReadAllText(path));
        }
        catch (IOException)
        {
            return MpvConfSchema.Empty;
        }
        catch (JsonException)
        {
            return MpvConfSchema.Empty;
        }
    }

    public static MpvConfSchema LoadFromDirectory(string directory)
    {
        if (!Directory.Exists(directory))
        {
            return MpvConfSchema.Empty;
        }

        var options = new Dictionary<string, MpvConfSchemaItem>(StringComparer.Ordinal);
        var ordered = new List<MpvConfSchemaItem>();
        foreach (string file in Directory.EnumerateFiles(directory, "*.json").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
        {
            MpvConfSchema schema = LoadFromFile(file);
            foreach (MpvConfSchemaItem def in schema.OrderedOptions)
            {
                if (!options.ContainsKey(def.Name))
                {
                    options[def.Name] = def;
                    ordered.Add(def);
                }
            }
        }

        return options.Count == 0 ? MpvConfSchema.Empty : new MpvConfSchema(options, ordered);
    }


    public static MpvConfSchema Merge(MpvConfSchema primary, MpvConfSchema added)
    {
        var merged = new Dictionary<string, MpvConfSchemaItem>(primary.Options, StringComparer.Ordinal);
        var ordered = new List<MpvConfSchemaItem>(primary.OrderedOptions);
        foreach ((string key, MpvConfSchemaItem def) in added.Options)
        {
            if (!merged.ContainsKey(key))
            {
                merged[key] = def;
                ordered.Add(def);
            }
        }

        return new MpvConfSchema(merged, ordered);
    }
}
