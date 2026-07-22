using Microsoft.UI.Windowing;
using mpv_winrt;
using mpv_winui.Modules.Common.Utils;

namespace mpv_winui.Modules.Player
{
    public sealed partial class MpvPlayerPage
    {
        private void MpvPlayerPage_WindowChanged(MpvMediaPlayer player, WindowChangedEventArgs args)
        {
            DispatcherQueue.RunAsync(() =>
            {
                try
                {
                    switch (args.PropertyId)
                    {
                        case 201:
                            HandleFullscreenProperty(args.Value);
                            break;
                        case 202:
                            HandleOnTopProperty(args.Value);
                            break;
                        case 203:
                            HandleWindowMinimizedProperty(args.Value);
                            break;
                        case 204:
                            HandleWindowMaximizedProperty(args.Value);
                            break;
                        case 205:
                            HandleTitleBarProperty(args.Value);
                            break;
                        case 206:
                            HandleBorderProperty(args.Value);
                            break;
                    }
                }
                catch (System.Exception ex)
                {
                    OnException(ex);
                }
            });
        }

        private void HandleFullscreenProperty(bool fullscreen)
        {
            //TODO force
            PlayerControl.ToggleFullScreen();
        }

        private void HandleOnTopProperty(bool enable)
        {
            if (_appWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.IsAlwaysOnTop = enable;
            }
        }

        private void HandleWindowMinimizedProperty(bool minimized)
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

        private void HandleWindowMaximizedProperty(bool maximized)
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

        private void HandleTitleBarProperty(bool showTitleBar)
        {
            //TODO force
            PlayerControl.ToggleFullWindow();
        }

        private void HandleBorderProperty(bool hasBorder)
        { if (_appWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.SetBorderAndTitleBar(hasBorder,true);
            }
        }
    }
}