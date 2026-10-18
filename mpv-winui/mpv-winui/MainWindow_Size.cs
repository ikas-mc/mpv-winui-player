using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using mpv_winui.Modules.Common.Utils;
using mpv_winui.Modules.Common.View;
using System;
using Windows.Win32.UI.WindowsAndMessaging;

namespace mpv_winui
{
    public sealed partial class MainWindow : BaseWindow
    {
        public const int MIN_LOGICAL_WIDTH = 250;
        public const int MIN_LOGICAL_HEIGHT = 250;

        private void SetupWindowSize()
        {
            RestoreWindowPositionAndSize();

            Body.Loaded += Body_Loaded;
            Body.Unloaded += Body_Unloaded;

            Closed += Size_Window_Closed;
        }

        private void Size_Window_Closed(object sender, WindowEventArgs args)
        {
            Closed -= Size_Window_Closed;
            SaveWindowPositionAndSize();
        }

        private void RestoreWindowPositionAndSize()
        {
            var lastRect = string.Empty;
            try
            {
                lastRect = AppContext.AppSetting.WindowPositionAndSize;
                if (string.IsNullOrEmpty(lastRect))
                {
                    return;
                }

                if (AppContext.AppLogger.IsDebugEnabled)
                {
                    AppContext.AppLogger.Debug("restore window position and size: lastRect={}", lastRect);
                }

                var v = Array.ConvertAll(lastRect.Split(','), int.Parse);
                if (v.Length >= 4 && v[2] > 0 && v[3] > 0)
                {
                    var x = v[0];
                    var y = v[1];
                    var w = Math.Max(100, v[2]);
                    var h = Math.Max(100, v[3]);
                    var maximized = v.Length >= 5 && v[4] == (int)SHOW_WINDOW_CMD.SW_SHOWMAXIMIZED;

                    if (AppContext.AppLogger.IsDebugEnabled)
                    {
                        AppContext.AppLogger.Debug("restore window position and size: x={},y={},w={},h={},maximized={}.", x, y, w, h, maximized);
                    }

                    Win32WindowHelper.SetRestoredPlacement(this, x, y, w, h, maximized);
                }
            }
            catch (Exception ex)
            {
                AppContext.AppLogger.Error(ex, "restore window position and size failed, saved={}", lastRect);
            }
        }

        public void SaveWindowPositionAndSize()
        {
            try
            {
                if (AppWindow?.Presenter.Kind is not AppWindowPresenterKind.Overlapped)
                {
                    return;
                }

                var (x, y, w, h, maximized) = Win32WindowHelper.GetRestoredPlacement(this);
                if (w <= 0 || h <= 0)
                {
                    return;
                }

                var showCmd = maximized ? (int)SHOW_WINDOW_CMD.SW_SHOWMAXIMIZED : (int)SHOW_WINDOW_CMD.SW_SHOWNORMAL;
                AppContext.AppSetting.WindowPositionAndSize = $"{x},{y},{w},{h},{showCmd}";

                if (AppContext.AppLogger.IsDebugEnabled)
                {
                    AppContext.AppLogger.Debug("save window position and size: x={},y={},w={},h={},maximized={}.", x, y, w, h, maximized);
                }
            }
            catch (Exception ex)
            {
                AppContext.AppLogger.Error(ex, "save window position and size failed");
            }
        }

        private void Body_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement body)
            {
                body.XamlRoot?.Changed += RootGridXamlRoot_Changed;
                this.SetWindowMinSize(MIN_LOGICAL_WIDTH, MIN_LOGICAL_HEIGHT);
            }
        }

        private void Body_Unloaded(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement body)
            {
                body.XamlRoot?.Changed -= RootGridXamlRoot_Changed;
            }
        }

        private void RootGridXamlRoot_Changed(XamlRoot sender, XamlRootChangedEventArgs args)
        {
            this.SetWindowMinSize(MIN_LOGICAL_WIDTH, MIN_LOGICAL_HEIGHT);
        }
    }
}
