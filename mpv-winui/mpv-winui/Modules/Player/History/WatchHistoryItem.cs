using System;

namespace mpv_winui.Modules.Player.History
{
    public class WatchHistoryItem
    {
        public string Path
        {
            get; set;
        } = string.Empty;

        public string? Title
        {
            get; set;
        }

        public DateTimeOffset? Time
        {
            get; set;
        }
    }
}
