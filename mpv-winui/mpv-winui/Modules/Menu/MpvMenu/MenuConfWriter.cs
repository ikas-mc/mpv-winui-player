using mpv_winui.Modules.FileSystem;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace mpv_winui.Modules.Menu.MpvMenu
{
    public static class MenuConfWriter
    {
        public static async Task SaveAsync(string filePath, IEnumerable<MpvMenuItem> items, bool backup = false, int limit = 50)
        {
            var builder = new StringBuilder();
            WriteItems(builder, items, 0);
            await FileService.Instance.BackAndSaveAsync(filePath, builder.ToString(), backup, limit).ConfigureAwait(false);
        }

        private static void WriteItems(StringBuilder builder, IEnumerable<MpvMenuItem> items, int depth)
        {
            foreach (var item in items)
            {
                if (item.IsSeparator)
                {
                    builder.AppendLine();
                    continue;
                }

                builder.Append('\t', depth);
                builder.Append(item.Name);

                if (!string.IsNullOrWhiteSpace(item.CommandString))
                {
                    builder.Append('\t');
                    builder.Append(item.CommandString);
                }

                AppendState(builder, "hidden", item.Hidden);
                AppendState(builder, "disabled", item.Disabled);
                AppendState(builder, "checked", item.Checked);

                builder.AppendLine();

                if (item.Children is { Count: > 0 })
                {
                    WriteItems(builder, item.Children, depth + 1);
                }
            }
        }

        private static void AppendState(StringBuilder builder, string name, string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            builder.Append('\t');
            builder.Append(name);
            builder.Append('=');
            builder.Append(value);
        }
    }
}
