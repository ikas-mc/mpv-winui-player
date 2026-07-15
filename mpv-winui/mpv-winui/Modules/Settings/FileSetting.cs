using mpv_winui.Modules.FileSystem;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace mpv_winui.Modules.Settings
{
    public class FileSetting : IDataSetting
    {
        private static readonly Logger Logger = AppContext.AppLogger;
        private readonly string _filePath;
        private readonly ConcurrentDictionary<string, string> _entries = new(StringComparer.Ordinal);

        public FileSetting(string file)
        {
            _filePath = AppData.Current.ResolveLocalData(file);
            Load();
        }

        public T GetValue<T>(string propertyName, T defaultValue)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                return defaultValue;
            }

            if (!_entries.TryGetValue(propertyName, out var value))
            {
                return defaultValue;
            }

            if (TryConvert(value, out T converted))
            {
                return converted;
            }

            return defaultValue;
        }

        public bool SetValue<T>(string propertyName, T value)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                return false;
            }

            if (!TryConvert(value, out string? serialized))
            {
                return false;
            }

            _entries[propertyName] = serialized ?? string.Empty;

            _ = SaveAsync();

            return true;
        }

        private void Load()
        {
            if (!File.Exists(_filePath))
            {
                return;
            }

            foreach (var line in File.ReadLines(_filePath))
            {
                var trimmed = line.AsSpan();
                if (trimmed.IsEmpty || trimmed[0] == '#' || trimmed[0] == '!')
                {
                    continue;
                }

                var eq = trimmed.IndexOf('=');
                if (eq < 0)
                {
                    continue;
                }

                var key = trimmed[..eq].TrimEnd().ToString();
                var valSpan = trimmed[(eq + 1)..];
                var val = valSpan.Length > 0 && char.IsWhiteSpace(valSpan[0])
                    ? valSpan.TrimStart().ToString()
                    : valSpan.ToString();
                _entries[key] = Unescape(val);
            }
        }

        private async Task SaveAsync()
        {
            try
            {
                var dir = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrWhiteSpace(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var sb = new StringBuilder();
                foreach (var kv in _entries)
                {
                    sb.Append(kv.Key).Append('=').AppendLine(Escape(kv.Value));
                }

                await File.WriteAllTextAsync(_filePath, sb.ToString());
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to save settings to {Path}", _filePath);
            }
        }

        private static string Escape(string s)
        {
            return s;
        }

        private static string Unescape(string s)
        {
            return s;
        }

        private static bool TryConvert<T>(string? input, out T value)
        {
            object? boxed = null;

            if (typeof(T) == typeof(string))
            {
                boxed = input;
            }
            else if (typeof(T) == typeof(bool) && bool.TryParse(input, out var b))
            {
                boxed = b;
            }
            else if (typeof(T) == typeof(int) && int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i))
            {
                boxed = i;
            }
            else if (typeof(T) == typeof(long) && long.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var l))
            {
                boxed = l;
            }
            else if (typeof(T) == typeof(ulong) && ulong.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ul))
            {
                boxed = ul;
            }
            else if (typeof(T) == typeof(double) && double.TryParse(input, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var d))
            {
                boxed = d;
            }
            else if (typeof(T) == typeof(float) && float.TryParse(input, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var f))
            {
                boxed = f;
            }
            else if (typeof(T) == typeof(decimal) && decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out var dc))
            {
                boxed = dc;
            }

            if (boxed is null && typeof(T) != typeof(string))
            {
                value = default!;
                return false;
            }

            value = (T)boxed!;
            return true;
        }

        private static bool TryConvert<T>(T value, out string? output)
        {
            switch (value)
            {
                case null:
                    output = null;
                    return true;
                case string s:
                    output = s;
                    return true;
                case bool b:
                    output = b ? "true" : "false";
                    return true;
                case int i:
                    output = i.ToString(CultureInfo.InvariantCulture);
                    return true;
                case long l:
                    output = l.ToString(CultureInfo.InvariantCulture);
                    return true;
                case ulong ul:
                    output = ul.ToString(CultureInfo.InvariantCulture);
                    return true;
                case double d:
                    output = d.ToString(CultureInfo.InvariantCulture);
                    return true;
                case float f:
                    output = f.ToString(CultureInfo.InvariantCulture);
                    return true;
                case decimal dc:
                    output = dc.ToString(CultureInfo.InvariantCulture);
                    return true;
                default:
                    output = null;
                    return false;
            }
        }
    }
}
