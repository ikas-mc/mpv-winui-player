using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using mpv_winui.Modules.Settings;

namespace mpv_winui.Modules.Player
{
    public sealed partial class MpvPlayerPage
    {
        private void SetupPlayControl()
        {
            UpdatePlayControlStyle();

            PlayerControl.MediaPlayer = _mediaPlayer;
            PlayerView.PointerPressed += PlayerView_PointerPressed2;
            PlayerView.DoubleTapped += PlayerView_DoubleTapped;
        }

        private void CleanupPlayControl()
        {
            PlayerView.PointerPressed -= PlayerView_PointerPressed2;
            PlayerView.DoubleTapped -= PlayerView_DoubleTapped;
            PlayerControl.MediaPlayer = null;
        }

        private void PlayerView_PointerPressed2(object sender, PointerRoutedEventArgs e)
        {
            var kind = e.GetCurrentPoint(PlayerView).Properties.PointerUpdateKind;

            if (kind != PointerUpdateKind.LeftButtonPressed && kind != PointerUpdateKind.MiddleButtonPressed)
            {
                return;
            }

            var mode = AppContext.AppSetting.PlayerControlToggleButton;

            switch (mode)
            {
                case AppSettings.PlayerControlToggleButton_Middle:
                {
                    if (kind == PointerUpdateKind.MiddleButtonPressed)
                    {
                        TogglePlayerControl();
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
                        TogglePlayerControl();
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

                    TogglePlayerControl();
                    break;
                }
            }
        }

        private void PlayerView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var mode = AppContext.AppSetting.PlayerControlToggleButton;

            switch (mode)
            {
                case AppSettings.PlayerControlToggleButton_LeftDouble:
                case AppSettings.PlayerControlToggleButton_LeftDoubleMiddle:
                {
                    TogglePlayerControl();
                    break;
                }

                default:
                {
                    break;
                }
            }
        }

        private void TogglePlayerControl()
        {
            PlayerControl.ToggleControlPanel();
        }

        public void UpdatePlayControlStyle()
        {
            var style = AppContext.AppSetting.PlayerStyle;

            switch (style)
            {
                case AppSettings.PlayerStyle_Center:
                {
                    PlayerControl.Style = Application.Current.Resources["CenterPlayerControlStyle"] as Style;
                    break;
                }

                case AppSettings.PlayerStyle_Compact:
                {
                    PlayerControl.Style = Application.Current.Resources["CompactPlayerControlStyle"] as Style;
                    break;
                }

                case AppSettings.PlayerStyle_Default:
                default:
                {
                    PlayerControl?.Style = null;
                    break;
                }
            }
        }
    }
}