using System;

namespace mpv_winui.Modules.Player
{
    public sealed partial class MpvPlayerPage
    {
        private const int PreviewWidth = 192;
        private const int PreviewHeight = 108;

        private void SetupPreview()
        {
            if (AppContext.AppSetting.EnableVideoPreview)
            {
                PlayerControl.PreviewUpdateRequested += PlayerControl_PreviewUpdateRequested;
                PlayerControl.PreviewClearRequested += PlayerControl_PreviewClearRequested;
            }
        }

        private void CleanupPreview()
        {
            PlayerControl.PreviewUpdateRequested -= PlayerControl_PreviewUpdateRequested;
            PlayerControl.PreviewClearRequested -= PlayerControl_PreviewClearRequested;
        }

        private void PlayerControl_PreviewUpdateRequested(object? sender, (double HoverSec, double X, double Y) args)
        {
            var point = PlayerControl.TransformSliderPoint(PlayerView, args.X, args.Y);

            var scaleX = PlayerView.CompositionScaleX;
            var scaleY = PlayerView.CompositionScaleY;

            double previewX;
            if (point.X < PreviewWidth / 2.0)
            {
                previewX = 0;
            }
            else if (point.X > (PlayerView.ActualWidth - (PreviewWidth / 2.0)))
            {
                previewX = PlayerView.ActualWidth - PreviewWidth;
            }
            else
            {
                previewX = point.X - (PreviewWidth / 2.0);
            }
            var previewY = point.Y - PreviewHeight - 8;

            _mediaPlayer.SetHoverSec(args.HoverSec);
            _mediaPlayer.SetDrawPreview((int)(previewX * scaleX), (int)(previewY * scaleY), (int)(PreviewWidth * scaleX), (int)(PreviewHeight * scaleY));
        }

        private void PlayerControl_PreviewClearRequested(object? sender, EventArgs e)
        {
            _mediaPlayer.ClearPreview();
        }
    }
}
