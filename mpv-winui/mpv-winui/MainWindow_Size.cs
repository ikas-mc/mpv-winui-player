using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using mpv_winui.Modules.Common.View;
using System;
using Windows.Graphics;
using WinRT;

namespace mpv_winui
{
    public sealed partial class MainWindow : Window
    {
        private int _x, _y, _w, _h;
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
                        if (v[0] > 0 && v[1] > 0 && v[2] > 0 && v[3] > 0)
                        {
                            AppWindow.MoveAndResize(new RectInt32(v[0], v[1], Math.Max(100, v[2]), Math.Max(100, v[3])));
                        }
                        else if (v[2] > 0 && v[3] > 0)
                        {
                            AppWindow.Resize(new SizeInt32(Math.Max(100, v[2]), Math.Max(100, v[3])));
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
                this.SetWindowMinSize(250, 250);
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
            this.SetWindowMinSize(250, 250);
        }
    }
}
