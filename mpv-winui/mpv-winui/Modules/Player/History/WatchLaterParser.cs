using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace mpv_winui.Modules.Player.History
{
    public static class WatchLaterParser
    {
        private static readonly Logger _logger = LogManager.GetLogger(nameof(WatchLaterParser));

        public static List<WatchLaterItem> Parse(string? directory)
        {
            var items = new List<WatchLaterItem>();
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
            {
                return items;
            }

            foreach (var file in Directory.EnumerateFiles(directory))
            {
                string? line;
                try
                {
                    line = File.ReadLines(file).FirstOrDefault();
                }
                catch (Exception ex)
                {
                    if (_logger.IsDebugEnabled)
                    {
                        _logger.Debug(ex, "watch later read failed, file={}", file);
                    }
                    continue;
                }

                if (string.IsNullOrEmpty(line) || line == "# redirect entry" || !line.StartsWith('#'))
                {
                    continue;
                }

                var name = line.Length > 2 ? line.Substring(2) : string.Empty;
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                items.Add(new WatchLaterItem
                {
                    Path = name,
                    Time = GetLastWriteTime(file)
                });
            }

            items.Sort(static (x, y) => y.Time.GetValueOrDefault().CompareTo(x.Time.GetValueOrDefault()));
            return items;
        }

        private static DateTimeOffset? GetLastWriteTime(string file)
        {
            try
            {
                var utc = File.GetLastWriteTimeUtc(file);
                return utc == DateTime.MinValue ? null : new DateTimeOffset(utc);
            }
            catch (Exception ex)
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug(ex, "watch later mtime failed, file={}", file);
                }
            }
            return null;
        }
    }
}
