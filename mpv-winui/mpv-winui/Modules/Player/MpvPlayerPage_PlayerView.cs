using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using mpv_winui.Modules.Common.Utils;
using System;

namespace mpv_winui.Modules.Player
{
    public record struct ViewSize(double Width, double Height, double WidthScale, double HidthScale);

    public sealed partial class MpvPlayerPage
    {
        private Action<ViewSize>? _sizeChangedAction;

        private void SetupPlayerView()
        {
            //TODO fix SetOption("force-window", "immediate");
            //var size = new ViewSize(PlayerView.ActualWidth, PlayerView.ActualHeight, PlayerView.CompositionScaleX, PlayerView.CompositionScaleY);
            //UpdatePlayerViewSize(size);
            //_mediaPlayer.UpdatePanel(PlayerView);

            _mediaPlayer.VoConfigured += MpvPlayer_VoConfigured;

            _sizeChangedAction = DebounceUtil.Debounce<ViewSize>(UpdatePlayerViewSize, TimeSpan.FromMilliseconds(100));
            PlayerView.SizeChanged += PlayerView_SizeChanged;

            //TODO 
            _lastCompositionScaleX = PlayerView.CompositionScaleX;
            _lastCompositionScaleY = PlayerView.CompositionScaleY;
            PlayerView.CompositionScaleChanged += PlayerView_CompositionScaleChanged;
        }

        private void TeardownPlayerView()
        {
            _sizeChangedAction = null;
            PlayerView.SizeChanged -= PlayerView_SizeChanged;
            PlayerView.CompositionScaleChanged -= PlayerView_CompositionScaleChanged;
            _mediaPlayer.VoConfigured -= MpvPlayer_VoConfigured;
        }

        private void MpvPlayer_VoConfigured(MpvMediaPlayer player, object? arg)
        {
            DispatcherQueue.RunAsync(() =>
            {
                var size = new ViewSize(PlayerView.ActualWidth, PlayerView.ActualHeight, PlayerView.CompositionScaleX, PlayerView.CompositionScaleY);
                UpdatePlayerViewSize(size);
                _mediaPlayer.UpdatePanel(PlayerView);
            });
        }

        private async void PlayerView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!_isPlayerInitialized)
            {
                return;
            }

            var size = new ViewSize(e.NewSize.Width, e.NewSize.Height, PlayerView.CompositionScaleX, PlayerView.CompositionScaleY);
            _sizeChangedAction?.Invoke(size);
        }

        private void UpdatePlayerViewSize(ViewSize size)
        {
            var width = (uint)Math.Floor(size.Width * size.WidthScale);
            var height = (uint)Math.Floor(size.Height * size.HidthScale);
            if (width <= 0)
            {
                width = 1;
            }
            if (height <= 0)
            {
                height = 1;
            }
            _mediaPlayer?.UpdateSize(width, height);
        }

        private double _lastCompositionScaleX;
        private double _lastCompositionScaleY;
        private void PlayerView_CompositionScaleChanged(Microsoft.UI.Xaml.Controls.SwapChainPanel sender, object args)
        {
            if (_lastCompositionScaleX != sender.CompositionScaleX || _lastCompositionScaleY != sender.CompositionScaleY)
            {
                _lastCompositionScaleX = sender.CompositionScaleX;
                _lastCompositionScaleY = sender.CompositionScaleY;

                _mediaPlayer?.UpdatePanelScale(sender.CompositionScaleX, sender.CompositionScaleY);

                //TODO
                var size = new ViewSize(sender.ActualWidth, sender.ActualHeight, sender.CompositionScaleX, sender.CompositionScaleY);
                _sizeChangedAction?.Invoke(size);
            }
        }
    }
}