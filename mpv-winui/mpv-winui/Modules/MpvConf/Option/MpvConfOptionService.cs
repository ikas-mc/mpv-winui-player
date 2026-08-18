using mpv_winui.Modules.MpvConf.Conf;
using mpv_winui.Modules.MpvConf.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace mpv_winui.Modules.MpvConf.Option;

public enum MpvConfOptionIncludeType
{
    All,
    FromConfig,
    Effective,
    Modified,
}

public static class MpvConfOptionService
{
    public const string UnknownGroup = "Unknown";

    public static IReadOnlyList<MpvConfOptionItem> GetOptions(MpvConfManager manager, MpvConfSchema schema, string profile, string? group, MpvConfOptionIncludeType mode)
    {
        bool unknownOnly = string.Equals(group, UnknownGroup, StringComparison.Ordinal);
        var entries = new List<MpvConfOptionItem>();

        if (!unknownOnly)
        {
            foreach (MpvConfSchemaItem def in schema.OrderedOptions)
            {
                if (group is not null && !string.Equals(def.Group, group, StringComparison.Ordinal))
                {
                    continue;
                }

                IReadOnlyList<MpvConfLine> present = manager.GetAll(def.Name, profile);
                if (present.Count > 0)
                {
                    foreach (MpvConfLine line in present)
                    {
                        if (ShouldInclude(line, mode))
                        {
                            entries.Add(new MpvConfOptionItem(manager, profile, def, line));
                        }
                    }
                }
                else if (ShouldInclude(null, mode))
                {
                    entries.Add(new MpvConfOptionItem(manager, profile, def, null));
                }
            }
        }

        if (group is null || unknownOnly)
        {
            foreach (MpvConfLine line in manager.Options)
            {
                if (!string.Equals(line.Section, profile, StringComparison.Ordinal))
                {
                    continue;
                }

                if (schema.Get(line.Key ?? string.Empty) is not null)
                {
                    continue;
                }

                if (ShouldInclude(line, mode))
                {
                    entries.Add(new MpvConfOptionItem(manager, profile, null, line));
                }
            }
        }

        if (mode == MpvConfOptionIncludeType.Modified)
        {
            foreach (MpvConfLine removed in manager.DeletedLines)
            {
                if (!string.Equals(removed.Section, profile, StringComparison.Ordinal))
                {
                    continue;
                }

                MpvConfSchemaItem? def = schema.Get(removed.Key ?? string.Empty);
                if (def is not null)
                {
                    if (!unknownOnly && (group is null || string.Equals(def.Group, group, StringComparison.Ordinal)))
                    {
                        entries.Add(new MpvConfOptionItem(manager, profile, def, null, removed));
                    }
                }
                else if (group is null || unknownOnly)
                {
                    entries.Add(new MpvConfOptionItem(manager, profile, null, null, removed));
                }
            }
        }

        return entries;
    }

    public static bool ContainsUnknownOptions(MpvConfManager manager, MpvConfSchema schema, string profile, MpvConfOptionIncludeType mode)
    {
        if (mode == MpvConfOptionIncludeType.Modified
            && manager.DeletedLines.Any(l => string.Equals(l.Section, profile, StringComparison.Ordinal) && schema.Get(l.Key ?? string.Empty) is null))
        {
            return true;
        }

        foreach (MpvConfLine line in manager.Options)
        {
            if (string.Equals(line.Section, profile, StringComparison.Ordinal)
                && schema.Get(line.Key ?? string.Empty) is null
                && ShouldInclude(line, mode))
            {
                return true;
            }
        }

        return false;
    }

    public static IReadOnlyList<string> GetGroups(MpvConfManager manager, MpvConfSchema schema, string profile, MpvConfOptionIncludeType mode)
    {
        var groups = new List<string>();
        foreach (string group in schema.Groups)
        {
            if (HasGroupEntries(manager, schema, group, profile, mode))
            {
                groups.Add(group);
            }
        }

        if (ContainsUnknownOptions(manager, schema, profile, mode))
        {
            groups.Add(UnknownGroup);
        }

        return groups;
    }

    private static bool HasGroupEntries(MpvConfManager manager, MpvConfSchema schema, string group, string profile, MpvConfOptionIncludeType mode)
    {
        foreach (MpvConfSchemaItem def in schema.OrderedOptions)
        {
            if (string.Equals(def.Group, group, StringComparison.Ordinal))
            {
                var lines = manager.GetAll(def.Name, profile);
                if (lines.Count == 0)
                {
                    if (ShouldInclude(null, mode))
                    {
                        return true;
                    }

                    if (mode == MpvConfOptionIncludeType.Modified
                        && manager.DeletedLines.Any(l => string.Equals(l.Key, def.Name, StringComparison.Ordinal) && string.Equals(l.Section, profile, StringComparison.Ordinal)))
                    {
                        return true;
                    }
                }
                else
                {
                    foreach (var line in lines)
                    {
                        if (ShouldInclude(line, mode))
                        {
                            return true;
                        }
                    }
                }

            }
        }
        return false;
    }

    private static bool ShouldInclude(MpvConfLine? line, MpvConfOptionIncludeType mode) =>
        mode switch
        {
            MpvConfOptionIncludeType.All => true,
            MpvConfOptionIncludeType.FromConfig => line is not null,
            MpvConfOptionIncludeType.Effective => line is { Enabled: true },
            MpvConfOptionIncludeType.Modified => line is { IsDirty: true },
            _ => false,
        };
}
