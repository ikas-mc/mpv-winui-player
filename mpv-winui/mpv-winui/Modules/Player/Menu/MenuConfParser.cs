using NLog;
using System;
using System.Collections.Generic;
using System.IO;

namespace mpv_winui.Modules.Player.Menu
{
    public static class MenuConfParser
    {
        private static readonly Logger _logger = LogManager.GetLogger(nameof(MenuConfParser));

        public static List<CustomMenuItem>? Parse(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return null;
            }

            try
            {
                var lines = File.ReadAllLines(filePath);
                var items = new List<CustomMenuItem>();
                ParseItems(lines, 0, 0, items);
                return items;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "menu.conf parse failed, path={}", filePath);
                return null;
            }
        }

        private static int ParseItems(string[] lines, int index, int minDepth, List<CustomMenuItem> target)
        {
            while (index < lines.Length)
            {
                var line = lines[index];
                if (line.Trim().Length == 0)
                {
                    index++;
                    continue;
                }

                var depth = LeadingWhitespaceLength(line);
                if (depth < minDepth)
                {
                    return index;
                }

                index++;
                var (title, command, isSubmenu) = ParseLine(line.TrimStart());
                if (title is null)
                {
                    continue;
                }

                if (isSubmenu)
                {
                    var children = new List<CustomMenuItem>();
                    index = ParseItems(lines, index, depth + 1, children);
                    target.Add(new CustomMenuItem { Name = title, Children = children });
                }
                else
                {
                    target.Add(new CustomMenuItem { Name = title, CommandString = command });
                }
            }

            return index;
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

        private static (string? Title, string? Command, bool IsSubmenu) ParseLine(string line)
        {
            var tokens = line.Split('\t', StringSplitOptions.RemoveEmptyEntries);
            var title = tokens.Length > 0 ? tokens[0].Replace("&", string.Empty).Trim() : null;
            if (string.IsNullOrEmpty(title))
            {
                return (null, null, false);
            }

            if (tokens.Length == 1 || IsStateToken(tokens[1]))
            {
                return (title, null, true);
            }

            return (title, tokens[1].Trim(), false);
        }

        private static bool IsStateToken(string token)
        {
            return token.StartsWith("checked=", StringComparison.Ordinal)
                || token.StartsWith("disabled=", StringComparison.Ordinal)
                || token.StartsWith("hidden=", StringComparison.Ordinal);
        }
    }
}
