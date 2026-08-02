using System;

namespace mpv_winui.Modules.Player.History
{
    public class WatchLaterItem
    {
        public string Path
        {
            get; set;
        } = string.Empty;

        public DateTimeOffset? Time
        {
            get; set;
        }
    }
}
