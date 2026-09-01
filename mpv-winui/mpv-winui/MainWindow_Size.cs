using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using mpv_winui.Modules.Common.View;
using System;
using Windows.Graphics;
using WinRT;

namespace mpv_winui
{
    public sealed partial class MainWindow : BaseWindow
    {
        public const int MIN_LOGICAL_WIDTH = 250;
        public const int MIN_LOGICAL_HEIGHT = 250;

        private int _x;
        private int _y;
        private int _w;
        private int _h;

        private void SetupWindowSize()
        {
            var lastRect = string.Empty;
            try
            {
                lastRect = AppContext.AppSetting.WindowPositionAndSize;
                if (!string.IsNullOrEmpty(lastRect))
                {
                    int[] v = Array.ConvertAll(lastRect.Split(','), int.Parse);
                    if (v.Length == 4)
                    {
                        _x = v[0];
                        _y = v[1];
                        _w = v[2];
                        _h = v[3];
                        if (_x > 0 && _y > 0 && _w > 0 && _h > 0)
                        {
                            AppWindow.MoveAndResize(new RectInt32(_x, _y, Math.Max(100, _w), Math.Max(100, _h)));
                        }
                        else if (_w > 0 && _h > 0)
                        {
                            AppWindow.Resize(new SizeInt32(Math.Max(100, _w), Math.Max(100, _h)));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AppContext.AppLogger.Error(ex, "restore window position and size failed, saved={}", lastRect);
            }

            this.Body.Loaded += Body_Loaded;
            this.Body.Unloaded += Body_Unloaded;

            AppWindow.Changed += Size_AppWindow_Changed;
            Closed += Size_Window_Closed;
        }

        private void Size_AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args)
        {
            if (!args.DidSizeChange && !args.DidPositionChange)
            {
                return;
            }

            if (sender.Presenter.Kind is AppWindowPresenterKind.Overlapped)
            {
                var overlappedPresenter = AppWindow.Presenter.As<OverlappedPresenter>();
                if (overlappedPresenter != null)
                {
                    if (overlappedPresenter.State is OverlappedPresenterState.Restored)
                    {
                        if (sender.Position.X > 0)
                        {
                            _x = sender.Position.X;
                        }
                        if (sender.Position.Y > 0)
                        {
                            _y = sender.Position.Y;
                        }
                        _w = sender.Size.Width;
                        _h = sender.Size.Height;
                    }
                }
            }

            if (AppContext.AppLogger.IsTraceEnabled)
            {
                AppContext.AppLogger.Debug("window last rect: x={},y={},w={},h={}.", _x, _y, _w, _h);
            }
        }

        private void Size_Window_Closed(object sender, WindowEventArgs args)
        {
            Closed -= Size_Window_Closed;
            AppWindow.Changed -= Size_AppWindow_Changed;

            SaveWindowPositionAndSize();
        }

        public void SaveWindowPositionAndSize()
        {
            try
            {
                AppContext.AppSetting.WindowPositionAndSize = $"{_x},{_y},{_w},{_h}";
                if (AppContext.AppLogger.IsTraceEnabled)
                {
                    AppContext.AppLogger.Debug("save window position and size: x={},y={},w={},h={}.", _x, _y, _w, _h);
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
