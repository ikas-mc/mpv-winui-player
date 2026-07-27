using System;
using Windows.Foundation;

namespace mpv_winui.Modules.Player
{
    public sealed partial class MpvPlayerPage
    {
        private const int PreviewWidth = 160;
        private const int PreviewHeight = 90;

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

        private void PlayerControl_PreviewUpdateRequested(object? sender, (double HoverSec, double RelativeX, double RelativeY) args)
        {
            var pointInPlayerView = PlayerControl.TransformToVisual(PlayerView).TransformPoint(new Point(args.RelativeX, args.RelativeY));

            var scaleX = PlayerView.CompositionScaleX;
            var scaleY = PlayerView.CompositionScaleY;
            var physicalX = pointInPlayerView.X * scaleX;
            var physicalY = pointInPlayerView.Y * scaleY;

            var previewX = physicalX - (PreviewWidth * scaleX / 2);
            var previewY = physicalY - (PreviewHeight * scaleY);
            if (previewY < 0)
            {
                previewY = 0;
            }

            _mediaPlayer.SetHoverSec(args.HoverSec);
            _mediaPlayer.SetDrawPreview((int)previewX, (int)previewY, (int)(PreviewWidth * scaleX), (int)(PreviewHeight * scaleY));
        }

        private void PlayerControl_PreviewClearRequested(object? sender, EventArgs e)
        {
            _mediaPlayer.ClearPreview();
        }
    }
}
