using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using Windows.Graphics;

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
                        if (v[0] > 0 && v[1] > 0)
                        {
                            AppWindow.MoveAndResize(new RectInt32(v[0], v[1], Math.Max(100, v[2]), Math.Max(100, v[3])));
                        }
                        else
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

            AppWindow.Changed += Size_AppWindow_Changed;
            Closed += Size_Window_Closed;
        }

        private void Size_AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args)
        {
            //TODO OverlappedPresenterState.Maximized
            if (sender.Presenter.Kind != AppWindowPresenterKind.FullScreen || sender.Presenter.Kind != AppWindowPresenterKind.Overlapped)
            {
                _x = sender.Position.X;
                _y = sender.Position.Y;
                _w = sender.Size.Width;
                _h = sender.Size.Height;
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
    }
}
