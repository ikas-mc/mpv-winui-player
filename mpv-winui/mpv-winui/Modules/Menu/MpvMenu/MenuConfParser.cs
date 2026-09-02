using NLog;
using System;
using System.Collections.Generic;
using System.IO;

namespace mpv_winui.Modules.Menu.MpvMenu
{
    public static class MenuConfParser
    {
        private static readonly Logger _logger = LogManager.GetLogger(nameof(MenuConfParser));

        public static List<MpvMenuItem>? Parse(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return null;
            }

            try
            {
                var lines = File.ReadAllLines(filePath);
                return Parse(lines);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "menu.conf parse failed, path={}", filePath);
                return null;
            }
        }

        public static List<MpvMenuItem> Parse(string[] lines)
        {
            var root = new List<MpvMenuItem>();
            var menusByDepth = new Dictionary<int, List<MpvMenuItem>> { [0] = root };
            var lastDepth = 0;

            for (var index = 0; index < lines.Length; index++)
            {
                var line = lines[index];
                var depth = LeadingWhitespaceLength(line);
                var trimmed = line.TrimStart();

                if (trimmed.Length == 0)
                {
                    var separator = new MpvMenuItem { IsSeparator = true };
                    List<MpvMenuItem>? target = null;

                    if (line.Length == 0)
                    {
                        // mpv: a blank line's separator depth is taken from the
                        // following line, and a trailing blank line is ignored.
                        if (index + 1 < lines.Length)
                        {
                            menusByDepth.TryGetValue(LeadingWhitespaceLength(lines[index + 1]), out target);
                        }
                    }
                    else
                    {
                        menusByDepth.TryGetValue(depth, out target);
                    }

                    if (target is null)
                    {
                        if (line.Length == 0)
                        {
                            continue;
                        }

                        target = menusByDepth[lastDepth];
                    }

                    target.Add(separator);
                    continue;
                }

                var item = ParseLine(trimmed);
                if (item is null)
                {
                    continue;
                }

                if (item.CommandString is null)
                {
                    item.Children = new List<MpvMenuItem>();
                }

                if (depth > lastDepth)
                {
                    var parentMenu = menusByDepth[lastDepth];
                    if (parentMenu.Count > 0 &&
                        parentMenu[^1] is { CommandString: null, IsSeparator: false } parent)
                    {
                        menusByDepth[depth] = parent.Children ??= new List<MpvMenuItem>();
                    }
                }

                if (!menusByDepth.TryGetValue(depth, out var targetMenu))
                {
                    targetMenu = menusByDepth[lastDepth];
                }

                targetMenu.Add(item);
                lastDepth = depth;
            }

            return root;
        }

        private static MpvMenuItem? ParseLine(string line)
        {
            var tokens = line.Split('\t', StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0 || string.IsNullOrWhiteSpace(tokens[0]))
            {
                return null;
            }

            var title = tokens[0].Trim();
            var command = default(string);
            var stateStart = 1;

            if (tokens.Length > 1)
            {
                var candidate = tokens[1].Trim();
                if (!IsStateToken(candidate))
                {
                    command = candidate;
                    stateStart = 2;
                }
            }

            return new MpvMenuItem
            {
                Name = title,
                CommandString = command,
                Hidden = ReadState(tokens, stateStart, "hidden="),
                Disabled = ReadState(tokens, stateStart, "disabled="),
                Checked = ReadState(tokens, stateStart, "checked="),
            };
        }

        private static string? ReadState(string[] tokens, int start, string prefix)
        {
            for (var index = start; index < tokens.Length; index++)
            {
                var token = tokens[index].Trim();
                if (token.StartsWith(prefix, StringComparison.Ordinal))
                {
                    return token.Substring(prefix.Length).Trim();
                }
            }

            return null;
        }

        private static bool IsStateToken(string token)
        {
            return token.StartsWith("checked=", StringComparison.Ordinal)
                || token.StartsWith("disabled=", StringComparison.Ordinal)
                || token.StartsWith("hidden=", StringComparison.Ordinal);
        }

        private static int LeadingWhitespaceLength(string line)
        {
            var count = 0;
            while (count < line.Length && char.IsWhiteSpace(line[count]))
            {
                count++;
            }

            return count;
        }
    }
}
