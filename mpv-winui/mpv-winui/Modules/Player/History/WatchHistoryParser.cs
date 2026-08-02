using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace mpv_winui.Modules.Player.History
{
    public static class WatchHistoryParser
    {
        private static readonly Logger _logger = LogManager.GetLogger(nameof(WatchHistoryParser));

        public static List<WatchHistoryItem> Parse(string? filePath)
        {
            var items = new List<WatchHistoryItem>();
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return items;
            }

            var seen = new Dictionary<string, WatchHistoryItem>(StringComparer.Ordinal);
            try
            {
                using var reader = new StreamReader(filePath, Encoding.UTF8);
                string? line;
                while ((line = reader.ReadLine()) is not null)
                {
                    if (!TryParseEntry(line, out var path, out var time, out var title))
                    {
                        continue;
                    }

                    if (seen.TryGetValue(path, out var existing))
                    {
                        if (time > (existing.Time?.ToUnixTimeSeconds() ?? 0))
                        {
                            existing.Time = ToDateTime(time);
                            existing.Title = title;
                        }
                    }
                    else
                    {
                        var item = BuildItem(path, time, title);
                        seen.Add(path, item);
                        items.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug(ex, "watch history read failed, path={}", filePath);
                }
            }

            items.Sort(static (x, y) => y.Time.GetValueOrDefault().CompareTo(x.Time.GetValueOrDefault()));
            return items;
        }

        private static WatchHistoryItem BuildItem(string path, long time, string? title)
        {
            return new WatchHistoryItem
            {
                Path = path,
                Title = title,
                Time = ToDateTime(time)
            };
        }

        private static DateTimeOffset? ToDateTime(long time)
        {
            if (time <= 0)
            {
                return null;
            }

            return DateTimeOffset.FromUnixTimeSeconds(time);
        }

        private static bool TryParseEntry(string line, out string path, out long time, out string? title)
        {
            path = string.Empty;
            time = 0;
            title = null;

            try
            {
                using var document = JsonDocument.Parse(line);
                var root = document.RootElement;
                if (!root.TryGetProperty("path", out var pathElement) || pathElement.ValueKind != JsonValueKind.String)
                {
                    return false;
                }

                path = pathElement.GetString() ?? string.Empty;
                if (path.Length == 0)
                {
                    return false;
                }

                if (root.TryGetProperty("time", out var timeElement) && timeElement.ValueKind == JsonValueKind.Number)
                {
                    timeElement.TryGetInt64(out time);
                }

                if (root.TryGetProperty("title", out var titleElement) && titleElement.ValueKind == JsonValueKind.String)
                {
                    title = titleElement.GetString();
                }

                return true;
            }
            catch (Exception ex)
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug(ex, "watch history entry parse failed, line={}", line);
                }
                return false;
            }
        }
    }
}
