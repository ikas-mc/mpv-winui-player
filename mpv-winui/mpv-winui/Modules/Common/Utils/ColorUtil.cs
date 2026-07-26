namespace mpv_winui.Modules.Common.Utils
{
    public static class ColorUtil
    {
        public static bool IsColorLight(Windows.UI.Color clr)
        {
            return ((5 * clr.G) + (2 * clr.R) + clr.B) > (8 * 128);
        }
    }
}
