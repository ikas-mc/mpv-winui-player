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
        }

        private void CleanupPlayControl()
        {
            PlayerView.PointerPressed -= PlayerView_PointerPressed2;
            PlayerControl.MediaPlayer = null;
        }

        private void PlayerView_PointerPressed2(object sender, PointerRoutedEventArgs e)
        {
            var kind = e.GetCurrentPoint(PlayerView).Properties.PointerUpdateKind;
            HandlePlayControl(kind);
        }

        private void HandlePlayControl(PointerUpdateKind kind)
        {
            if (_discMenuActive && kind != PointerUpdateKind.MiddleButtonPressed)
            {
                return;
            }

            if (kind != PointerUpdateKind.LeftButtonPressed && kind != PointerUpdateKind.MiddleButtonPressed)
            {
                return;
            }

            TogglePlayerControl();
        }

        public void TogglePlayerControl()
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