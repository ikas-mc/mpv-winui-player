using System.Collections.Generic;

namespace mpv_winui.Modules.Menu.MpvMenu
{
    public sealed class MpvMenuItem
    {
        public string? Name
        {
            get; set;
        }

        public string? CommandString
        {
            get; set;
        }

        public bool IsSeparator
        {
            get; set;
        }

        public string? Hidden
        {
            get; set;
        }

        public string? Disabled
        {
            get; set;
        }

        public string? Checked
        {
            get; set;
        }

        public List<MpvMenuItem>? Children
        {
            get; set;
        }
    }
}
