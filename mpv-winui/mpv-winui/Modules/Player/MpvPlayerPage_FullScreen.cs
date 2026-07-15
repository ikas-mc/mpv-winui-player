using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace mpv_winui.Modules.Player
{
    public sealed partial class MpvPlayerPage
    {
        private bool _isFullScreen;
        private bool _isFullWindow;

        private bool PlayerControl_OnFullScreenRequest()
        {
            bool isFullScreen;

            if (_appWindow.Presenter.Kind == AppWindowPresenterKind.FullScreen)
            {
                _appWindow.SetPresenter(AppWindowPresenterKind.Default);
            }
            else
            {
                _appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
            }

            isFullScreen = _appWindow.Presenter.Kind == AppWindowPresenterKind.FullScreen;

            if (isFullScreen)
            {
                if (!_isFullWindow)
                {
                    PlayerControl.ToggleFullWindow();
                }
            }
            else
            {
                if (_isFullWindow)
                {
                    PlayerControl.ToggleFullWindow();
                }
            }

            _isFullScreen = isFullScreen;
            PlayerControl.UpdateFullScreen(isFullScreen);

            return isFullScreen;
        }

        private bool PlayerControl_OnFullWindowRequest()
        {
            bool isFullWindow = _isFullWindow;

            if (isFullWindow)
            {
                isFullWindow = !VisualStateManager.GoToState(this, "NormalWindow", true);
            }
            else
            {
                isFullWindow = VisualStateManager.GoToState(this, "FullWindow", true);
            }

            //TODO 
            if (App.Window is MainWindow window)
            {
                window.ChangeFullWindow(isFullWindow);
            }

            _isFullWindow = isFullWindow;

            return isFullWindow;
        }
    }
}