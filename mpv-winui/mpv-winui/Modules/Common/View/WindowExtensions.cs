using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using mpv_winui.Modules.Common.Utils;
using WinRT;

namespace mpv_winui.Modules.Common.View
{
    public static class WindowExtensions
    {
        extension(Window window)
        {
            public void ShowWindow()
            {
                Win32WindowHelper.SetForegroundWindow(window);
            }

            public void SetWindowMinSize(double widthPx, double heightPx)
            {
                if (window.Content.XamlRoot is XamlRoot root && window.AppWindow.Presenter.Kind == AppWindowPresenterKind.Overlapped)
                {
                    var overlappedPresenter = window.AppWindow.Presenter.As<OverlappedPresenter>();
                    if (overlappedPresenter != null)
                    {
                        var scale = root.RasterizationScale > 0 ? root.RasterizationScale : 1;
                        overlappedPresenter.PreferredMinimumWidth = (int)(widthPx * scale);
                        overlappedPresenter.PreferredMinimumHeight = (int)(heightPx * scale);
                    }
                }
            }
        }
    }
}
