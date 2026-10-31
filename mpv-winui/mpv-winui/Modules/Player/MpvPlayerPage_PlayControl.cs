using Microsoft.UI.Xaml;
using mpv_winui.Modules.Settings;

namespace mpv_winui.Modules.Player
{
    public sealed partial class MpvPlayerPage
    {
        private void SetupPlayControl()
        {
            UpdatePlayControlStyle();

            PlayerControl.MediaPlayer = _mediaPlayer;

            if (AppContext.AppSetting.EnablePlayerControlAutoHide)
            {
                SetupPlayControlAutoHide();
            }
            else
            {
                SetupPlayControlHide();
            }
        }

        private void CleanupPlayControl()
        {
            CleanupPlayControlAutoHide();
            CleanupPlayControlHide();

            PlayerControl.MediaPlayer = null;
        }

        private void UpdatePlayControl()
        {
            PlayerControl.ShowControlPanel();

            if (_playerControlAutoHideEnabled)
            {
                UpdatePlayControlAutoHide();
            }
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
