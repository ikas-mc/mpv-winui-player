using Microsoft.UI.Xaml;

namespace mpv_winui.Modules.Common.Convertor
{
    public static class BindConvertor
    {
        public static bool InverseBool(bool? value)
        {
            return value is null || !value.Value;
        }

        public static Visibility InvertVisibility(bool? value)
        {
            return value is null || !value.Value ? Visibility.Visible : Visibility.Collapsed;
        }

        public static Visibility InvertVisibility(Visibility? value)
        {
            return value is null || value.Value == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
