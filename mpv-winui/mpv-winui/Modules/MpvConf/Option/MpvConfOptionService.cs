using mpv_winui.Modules.MpvConf.Conf;
using mpv_winui.Modules.MpvConf.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace mpv_winui.Modules.MpvConf.Option;

public enum MpvConfOptionIncludeType
{
    All,
    FromConfFile,
    Enabled,
    Modified,
}

public sealed class MpvConfOptionService
{
    public const string UnknownGroup = "Unknown";

    private readonly MpvConfManager _manager;
    private readonly MpvConfSchema _schema;

    public MpvConfOptionService(MpvConfManager manager, MpvConfSchema schema)
    {
        _manager = manager;
        _schema = schema;
    }

    public IReadOnlyList<MpvConfOptionItem> GetOptions(string profile, string? group, MpvConfOptionIncludeType mode)
    {
        bool unknownOnly = string.Equals(group, UnknownGroup, StringComparison.Ordinal);
        var entries = new List<MpvConfOptionItem>();

        if (!unknownOnly)
        {
            foreach (MpvConfSchemaItem def in _schema.OrderedOptions)
            {
                if (group is not null && !string.Equals(def.Group, group, StringComparison.Ordinal))
                {
                    continue;
                }

                IReadOnlyList<MpvConfLine> present = _manager.GetAll(def.Name, profile);
                if (present.Count > 0)
                {
                    foreach (MpvConfLine line in present)
                    {
                        if (ShouldInclude(line, mode))
                        {
                            entries.Add(new MpvConfOptionItem(profile, def, line));
                        }
                    }
                }
                else if (ShouldInclude(null, mode))
                {
                    entries.Add(new MpvConfOptionItem(profile, def, null));
                }
            }
        }

        if (group is null || unknownOnly)
        {
            foreach (MpvConfLine line in _manager.Options)
            {
                if (!string.Equals(line.Section, profile, StringComparison.Ordinal))
                {
                    continue;
                }

                if (_schema.Get(line.Key ?? string.Empty) is not null)
                {
                    continue;
                }

                if (ShouldInclude(line, mode))
                {
                    entries.Add(new MpvConfOptionItem(profile, null, line));
                }
            }
        }

        if (mode == MpvConfOptionIncludeType.Modified)
        {
            foreach (MpvConfLine removed in _manager.DeletedLines)
            {
                if (!string.Equals(removed.Section, profile, StringComparison.Ordinal))
                {
                    continue;
                }

                MpvConfSchemaItem? def = _schema.Get(removed.Key ?? string.Empty);
                if (def is not null)
                {
                    if (!unknownOnly && (group is null || string.Equals(def.Group, group, StringComparison.Ordinal)))
                    {
                        entries.Add(new MpvConfOptionItem(profile, def, null, removed));
                    }
                }
                else if (group is null || unknownOnly)
                {
                    entries.Add(new MpvConfOptionItem(profile, null, null, removed));
                }
            }
        }

        return entries;
    }

    public bool ContainsUnknownOptions(string profile, MpvConfOptionIncludeType mode)
    {
        if (mode == MpvConfOptionIncludeType.Modified
            && _manager.DeletedLines.Any(l => string.Equals(l.Section, profile, StringComparison.Ordinal) && _schema.Get(l.Key ?? string.Empty) is null))
        {
            return true;
        }

        foreach (MpvConfLine line in _manager.Options)
        {
            if (string.Equals(line.Section, profile, StringComparison.Ordinal)
                && _schema.Get(line.Key ?? string.Empty) is null
                && ShouldInclude(line, mode))
            {
                return true;
            }
        }

        return false;
    }

    public IReadOnlyList<string> GetGroups(string profile, MpvConfOptionIncludeType mode)
    {
        var groups = new List<string>();
        foreach (string group in _schema.Groups)
        {
            if (HasGroupEntries(group, profile, mode))
            {
                groups.Add(group);
            }
        }

        if (ContainsUnknownOptions(profile, mode))
        {
            groups.Add(UnknownGroup);
        }

        return groups;
    }

    private bool HasGroupEntries(string group, string profile, MpvConfOptionIncludeType mode)
    {
        foreach (MpvConfSchemaItem def in _schema.OrderedOptions)
        {
            if (string.Equals(def.Group, group, StringComparison.Ordinal))
            {
                var lines = _manager.GetAll(def.Name, profile);
                if (lines.Count == 0)
                {
                    if (ShouldInclude(null, mode))
                    {
                        return true;
                    }

                    if (mode == MpvConfOptionIncludeType.Modified
                        && _manager.DeletedLines.Any(l => string.Equals(l.Key, def.Name, StringComparison.Ordinal) && string.Equals(l.Section, profile, StringComparison.Ordinal)))
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
            MpvConfOptionIncludeType.FromConfFile => line is not null,
            MpvConfOptionIncludeType.Enabled => line is { Enabled: true },
            MpvConfOptionIncludeType.Modified => line is { IsDirty: true },
            _ => false,
        };

    public void SetState(MpvConfOptionItem item, MpvOptionState state)
    {
        if (state == item.State)
        {
            return;
        }

        switch (state)
        {
            case MpvOptionState.Enabled:
                EnsureLine(item, enabled: true);
                break;
            case MpvOptionState.Disabled:
                EnsureLine(item, enabled: false);
                break;
            case MpvOptionState.NotInFile:
                RemoveLine(item);
                break;
        }
    }

    public void SetValue(MpvConfOptionItem item, string value)
    {
        string v = value ?? string.Empty;
        if (item.Line is not null)
        {
            item.Line.Value = v;
            item.Line.Modified = true;
        }
        else
        {
            item.PendingValue = v;
        }
    }

    private void EnsureLine(MpvConfOptionItem item, bool enabled)
    {
        if (item.DeletedLine is not null)
        {
            if (_manager.Restore(item.DeletedLine))
            {
                item.Line = item.DeletedLine;
                item.DeletedLine = null;

                if (item.PendingValue is not null)
                {
                    item.Line.Value = item.PendingValue;
                    item.Line.Modified = true;
                    item.PendingValue = null;
                }
            }
            else
            {
                item.DeletedLine = null;
            }
        }

        if (item.Line is not null)
        {
            if (item.Line.Enabled != enabled)
            {
                item.Line.Enabled = enabled;
                item.Line.Modified = true;
            }

            return;
        }

        string value = CurrentValue(item);
        item.Line = enabled
            ? _manager.InsertOption(item.Key, value, item.Profile)
            : _manager.InsertDisabled(item.Key, value, item.Profile);
    }

    private static string CurrentValue(MpvConfOptionItem item)
    {
        return item.PendingValue ?? item.Definition?.DefaultValue ?? string.Empty;
    }

    private void RemoveLine(MpvConfOptionItem item)
    {
        if (item.Line is not null)
        {
            bool tombstone = item.Line.Status != MpvConfLineStatus.Added;
            _manager.Remove(item.Line);
            item.DeletedLine = tombstone ? item.Line : null;
        }

        item.PendingValue = null;
        item.Line = null;
    }
}
