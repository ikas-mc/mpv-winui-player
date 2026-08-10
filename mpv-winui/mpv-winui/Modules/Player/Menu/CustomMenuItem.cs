using System.Collections.Generic;

namespace mpv_winui.Modules.Player.Menu
{
    public class CustomMenuItem
    {
        public string? Name
        {
            get; set;
        }

        public List<string>? Command
        {
            get; set;
        }

        public string? CommandString
        {
            get; set;
        }

        public List<CustomMenuItem>? Children
        {
            get; set;
        }
    }
}
