using Microsoft.UI.Xaml;
using System;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace mpv_winui.Modules.Common.Utils
{
    internal class Win32WindowHelper
    {
        public static nint GetHwnd(Window window) => WinRT.Interop.WindowNative.GetWindowHandle(window);

        public static double GetWindowScale(Window window)
        {
            var dpi = PInvoke.GetDpiForWindow(new HWND(GetHwnd(window)));
            var scalingFactor = (float)dpi / 96;
            return scalingFactor > 0 ? scalingFactor : 1;
        }

        public static void SetForegroundWindow(Window window)
        {
            PInvoke.SetForegroundWindow(new HWND(GetHwnd(window)));
        }

        private static void ShowWindow(Window window, int value)
        {
            PInvoke.ShowWindow(new HWND(GetHwnd(window)), (SHOW_WINDOW_CMD)value);
        }

        public static HMONITOR GetMonitorFromRect(RECT rect)
        {
            return PInvoke.MonitorFromRect(rect, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);
        }

        public static HMONITOR GetMonitor(Window window)
        {
            var hwnd = new HWND(GetHwnd(window));
            return PInvoke.MonitorFromWindow(hwnd, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);
        }

        public static uint GetDisplayFrequency(HMONITOR hMonitor, uint value = 60)
        {
            if (hMonitor == IntPtr.Zero)
            {
                return value;
            }

            unsafe
            {
                MONITORINFOEXW info = new()
                {
                };
                info.monitorInfo.cbSize = (uint)sizeof(MONITORINFOEXW);

                if (!PInvoke.GetMonitorInfo(hMonitor, (MONITORINFO*)&info))
                {
                    return value;
                }

                DEVMODEW dm = new()
                {
                    dmSize = (ushort)sizeof(DEVMODEW)
                };

                if (!PInvoke.EnumDisplaySettings(info.szDevice.ToString(), ENUM_DISPLAY_SETTINGS_MODE.ENUM_CURRENT_SETTINGS, ref dm))
                {
                    return value;
                }

                return dm.dmDisplayFrequency;
            }
        }
    }
}
