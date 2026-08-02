using Microsoft.UI.Xaml;
using System;

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

        public static string FormatTime(DateTimeOffset? time)
        {
            return time?.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty;
        }

        public static string FormatDate(DateTimeOffset? time)
        {
            return time?.ToLocalTime().ToString("yyyy-MM-dd") ?? string.Empty;
        }

        public static Visibility HasTextVisibility(string? text)
        {
            return string.IsNullOrEmpty(text) ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
