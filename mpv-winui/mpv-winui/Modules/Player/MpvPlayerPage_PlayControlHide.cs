using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Input;
using mpv_winui.Modules.Settings;

namespace mpv_winui.Modules.Player
{
    public sealed partial class MpvPlayerPage
    {
        private void SetupPlayControlHide()
        {
            PlayerView.PointerPressed += PlayerView_PointerPressed2;
            PlayerView.DoubleTapped += PlayerView_DoubleTapped;
        }

        private void CleanupPlayControlHide()
        {
            PlayerView.PointerPressed -= PlayerView_PointerPressed2;
            PlayerView.DoubleTapped -= PlayerView_DoubleTapped;
        }

        private void PlayerView_PointerPressed2(object sender, PointerRoutedEventArgs e)
        {
            if (!_isFullWindow && !_isFullScreen)
            {
                return;
            }

            var kind = e.GetCurrentPoint(PlayerView).Properties.PointerUpdateKind;
            if (kind != PointerUpdateKind.LeftButtonPressed && kind != PointerUpdateKind.MiddleButtonPressed)
            {
                return;
            }

            var toggleButton = AppContext.AppSetting.PlayerControlToggleButton;
            switch (toggleButton)
            {
                case AppSettings.PlayerControlToggleButton_Middle:
                {
                    if (kind == PointerUpdateKind.MiddleButtonPressed)
                    {
                        TogglePlayControl();
                    }

                    break;
                }
                case AppSettings.PlayerControlToggleButton_LeftDouble:
                {
                    break;
                }
                case AppSettings.PlayerControlToggleButton_LeftDoubleMiddle:
                {
                    if (kind == PointerUpdateKind.MiddleButtonPressed)
                    {
                        TogglePlayControl();
                    }

                    break;
                }
                case AppSettings.PlayerControlToggleButton_LeftMiddle:
                default:
                {
                    if (_discMenuActive && kind == PointerUpdateKind.LeftButtonPressed)
                    {
                        break;
                    }

                    TogglePlayControl();
                    break;
                }
            }
        }

        private void PlayerView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (!_isFullWindow && !_isFullScreen)
            {
                return;
            }

            var toggleButton = AppContext.AppSetting.PlayerControlToggleButton;
            switch (toggleButton)
            {
                case AppSettings.PlayerControlToggleButton_LeftDouble:
                case AppSettings.PlayerControlToggleButton_LeftDoubleMiddle:
                {
                    TogglePlayControl();
                    break;
                }
                default:
                {
                    break;
                }
            }
        }

        private void TogglePlayControl()
        {
            PlayerControl.ToggleControlPanel();
        }
    }
}
