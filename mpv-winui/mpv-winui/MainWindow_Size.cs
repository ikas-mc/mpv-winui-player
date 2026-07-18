using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using mpv_winui.Modules.Win32;
using System;
using Windows.Graphics;

namespace mpv_winui
{
    public sealed partial class MainWindow : Window
    {
        private int _x, _y, _w, _h;
        private double _scale;
        private void SetupWindowSize()
        {
            try
            {
                string saved = AppContext.AppSetting.WindowPositionAndSize;
                if (!string.IsNullOrEmpty(saved))
                {
                    int[] v = Array.ConvertAll(saved.Split(','), int.Parse);
                    if (v.Length == 4)
                    {
                        var scale = Win32WindowHelper.GetWindowScale(this);
                        if (v[0] > 0 && v[1] > 0)
                        {
                            AppWindow.MoveAndResize(new RectInt32((int)(v[0] / scale), (int)(v[1] / scale), Math.Max(100, (int)(v[2] / scale)), Math.Max(100, (int)(v[3] / scale))));
                        }
                        else
                        {
                            AppWindow.Resize(new SizeInt32(Math.Max(100, (int)(v[2] / scale)), Math.Max(100, (int)(v[3] / scale))));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AppContext.AppLogger.Error(ex, "restore window position and size failed");
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

            //TODO Maximized
            if (sender.Presenter.Kind == AppWindowPresenterKind.Overlapped || sender.Presenter.Kind == AppWindowPresenterKind.CompactOverlay)
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

            if (AppContext.AppLogger.IsTraceEnabled)
            {
                AppContext.AppLogger.Debug("window last rect: x={},y={},w={},h={}. scale={}", _x, _y, _w, _h, _scale);
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
                UpdateWindowMinSize(240, 240);
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
            UpdateWindowMinSize(240, 240);
        }

        public void UpdateWindowMinSize(int w, int h)
        {
            if (Content.XamlRoot is XamlRoot root)
            {
                var scale = root.RasterizationScale > 0 ? root.RasterizationScale : 1;
                if (AppWindow.Presenter is OverlappedPresenter overlappedPresenter)
                {
                    overlappedPresenter.PreferredMinimumWidth = (int)(250 * scale);
                    overlappedPresenter.PreferredMinimumHeight = (int)(250 * scale);
                }
            }
        }
    }
}
