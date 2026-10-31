using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;

namespace mpv_winui.Modules.Player
{
    public sealed partial class MpvPlayerPage
    {
        private bool _isFullScreen;
        private bool _isFullWindow;

        private void SetupFullScreen()
        {
            CleanupFullScreen();
            PlayerControl.FullScreenClick += PlayerControl_FullScreenClick;
            PlayerControl.FullWindowClick += PlayerControl_FullWindowClick;
        }

        private void CleanupFullScreen()
        {
            PlayerControl.FullScreenClick -= PlayerControl_FullScreenClick;
            PlayerControl.FullWindowClick -= PlayerControl_FullWindowClick;
        }

        private void PlayerControl_FullScreenClick(object? sender, EventArgs e)
        {
            ToggleFullScreen();
        }

        private void PlayerControl_FullWindowClick(object? sender, EventArgs e)
        {
            ToggleFullWindow();
        }

        public void ToggleFullScreen()
        {
            SetFullScreen(!_isFullScreen);
        }

        public void ToggleFullWindow()
        {
            SetFullWindow(!_isFullWindow);
        }

        public void SetFullScreen(bool isFullScreen)
        {
            var presenterKind = isFullScreen ? AppWindowPresenterKind.FullScreen : AppWindowPresenterKind.Default;
            if (_appWindow.Presenter.Kind != presenterKind)
            {
                _appWindow.SetPresenter(presenterKind);
            }

            _isFullScreen = isFullScreen;
            SetFullWindow(isFullScreen);
            PlayerControl.UpdateFullScreenState(isFullScreen);
        }

        public void SetFullWindow(bool isFullWindow)
        {
            if (_isFullWindow == isFullWindow)
            {
                return;
            }

            _isFullWindow = isFullWindow;
            VisualStateManager.GoToState(this, isFullWindow ? "FullWindow" : "NormalWindow", true);
            if (App.Window is MainWindow window)
            {
                window.ChangeFullWindow(isFullWindow);
            }
            PlayerControl.UpdateFullWindowState(isFullWindow);
        }
    }
}
