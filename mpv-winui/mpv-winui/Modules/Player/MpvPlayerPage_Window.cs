using Microsoft.UI.Windowing;
using mpv_winrt;
using mpv_winui.Modules.Common.Utils;

namespace mpv_winui.Modules.Player
{
    public sealed partial class MpvPlayerPage
    {
        private void MpvPlayerPage_WindowChanged(WindowChangedEventArgs args)
        {
            DispatcherQueue.RunAsync(() =>
            {
                try
                {
                    switch (args.PropertyId)
                    {
                        case 201:
                            SetFullScreen(args.Value);
                            break;
                        case 202:
                            SetAlwaysOnTop(args.Value);
                            break;
                        case 203:
                            SetWindowMinimized(args.Value);
                            break;
                        case 204:
                            SetWindowMaximized(args.Value);
                            break;
                        case 205:
                            SetFullWindow(args.Value);
                            break;
                        case 206:
                            SetWindowBorder(args.Value);
                            break;
                    }
                }
                catch (System.Exception ex)
                {
                    OnException(ex);
                }
            });
        }

        private void SetFullScreen(bool fullscreen)
        {
            //TODO force
            PlayerControl.ToggleFullScreen();
        }

        private void SetAlwaysOnTop(bool enable)
        {
            if (_appWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.IsAlwaysOnTop = enable;
            }
        }

        private void ToggleAlwaysOnTop()
        {
            if (_appWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.IsAlwaysOnTop = !presenter.IsAlwaysOnTop;
            }
        }

        private void SetWindowMinimized(bool minimized)
        {
            if (_appWindow.Presenter is OverlappedPresenter presenter)
            {
                if (minimized)
                {
                    presenter.Minimize();
                }
                else
                {
                    presenter.Restore();
                }
            }
        }

        private void SetWindowMaximized(bool maximized)
        {
            if (_appWindow.Presenter is OverlappedPresenter presenter)
            {
                if (maximized)
                {
                    presenter.Maximize();
                }
                else
                {
                    presenter.Restore();
                }
            }
        }

        private void SetFullWindow(bool showTitleBar)
        {
            //TODO force
            PlayerControl.ToggleFullWindow();
        }

        private void SetWindowBorder(bool hasBorder)
        {
            if (_appWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.SetBorderAndTitleBar(hasBorder, true);
            }
        }
    }
}