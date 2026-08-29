using Microsoft.UI.Xaml;
using System;

namespace mpv_winui.Modules.Player
{
    public sealed partial class MpvPlayerPage
    {
        //TODO
        private bool _enableBuiltInPreview = AppContext.AppSetting.EnableVideoBuiltInPreview;

        private void SetupBuiltInPreview()
        {
            if (_enableBuiltInPreview)
            {
                FindName("MpvPreview");
                MpvPreview.KeepAlive = AppContext.AppSetting.KeepVideoBuiltInPreviewAlive;
                MpvPreview.KeepAliveTimeout = AppContext.AppSetting.BuiltInPreviewAliveTimeout;
                PlayerControl.PreviewUpdateRequested += PlayerControl_Preview2UpdateRequested;
                PlayerControl.PreviewClearRequested += PlayerControl_Preview2ClearRequested;
            }
        }

        private void CleanupBuiltInPreview()
        {
            PlayerControl.PreviewUpdateRequested -= PlayerControl_Preview2UpdateRequested;
            PlayerControl.PreviewClearRequested -= PlayerControl_Preview2ClearRequested;
            MpvPreview?.Close();
        }

        private void PlayerControl_Preview2UpdateRequested(object? sender, (double HoverSec, double X, double Y) args)
        {
            var path = _mediaPlayer.GetCurrentPath();
            if (string.IsNullOrEmpty(path))
            {
                MpvPreview.Hide();
                return;
            }

            var point = PlayerControl.TransformSliderPoint(PlayerView, args.X, args.Y);
            if (_logger.IsDebugEnabled)
            {
                _logger.Debug("BuiltInPreview UpdateRequested,  HoverSec={}, RelativeX={}, RelativeY={}", args.HoverSec, point.X, point.Y);
            }
            if (point.X < MpvPreview.ActualWidth / 2.0)
            {
                PreviewControlTranslation.X = 0;
            }
            else if (point.X > (PlayerControl.ActualWidth - (MpvPreview.ActualWidth / 2.0)))
            {
                PreviewControlTranslation.X = PlayerControl.ActualWidth - MpvPreview.ActualWidth;
            }
            else
            {
                PreviewControlTranslation.X = point.X - (MpvPreview.ActualWidth / 2.0);
            }
            PreviewControlTranslation.Y = point.Y - MpvPreview.ActualHeight - 8;

            MpvPreview.Show(args.HoverSec, path);
            MpvPreview.Visibility = Visibility.Visible;
        }

        private void PlayerControl_Preview2ClearRequested(object? sender, EventArgs e)
        {
            MpvPreview.Visibility = Visibility.Collapsed;
            MpvPreview.Hide();
        }
    }
}
