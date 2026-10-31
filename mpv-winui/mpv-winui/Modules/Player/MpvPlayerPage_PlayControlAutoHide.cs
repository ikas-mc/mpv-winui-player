using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using mpv_winui.Modules.Player.PlayerControl;
using System.Linq;
using Windows.Foundation;

namespace mpv_winui.Modules.Player
{
    public sealed partial class MpvPlayerPage
    {
        private PlayControlAutoHideController? _playControlAutoHide;
        private bool _playerControlAutoHideEnabled;
        private int _playerControlAutoHideDelay;
        private bool _pointerButtonPressed;
        private Point? _lastPointerPosition;

        private void SetupPlayControlAutoHide()
        {
            _playerControlAutoHideEnabled = AppContext.AppSetting.EnablePlayerControlAutoHide;
            _playerControlAutoHideDelay = AppContext.AppSetting.PlayerControlAutoHideDelay;

            _playControlAutoHide = new PlayControlAutoHideController(
                DispatcherQueue,
                PlayerControl.IsVisible,
                PlayerControl.ShowControlPanel,
                PlayerControl.HideControlPanel);

            _playControlAutoHide.SetBlockReasonProvider(GetPlayControlAutoHideBlockReason);
            _playControlAutoHide.Configure(_playerControlAutoHideEnabled, _playerControlAutoHideDelay);

            AddHandler(UIElement.PointerMovedEvent, (PointerEventHandler)MpvPlayerPage_PointerMoved, true);
            AddHandler(UIElement.PointerPressedEvent, (PointerEventHandler)MpvPlayerPage_PointerPressed, true);
            AddHandler(UIElement.PointerReleasedEvent, (PointerEventHandler)MpvPlayerPage_PointerReleased, true);
            AddHandler(UIElement.PointerCanceledEvent, (PointerEventHandler)MpvPlayerPage_PointerReleased, true);
            AddHandler(UIElement.PointerCaptureLostEvent, (PointerEventHandler)MpvPlayerPage_PointerCaptureLost, true);
            AddHandler(UIElement.PointerExitedEvent, (PointerEventHandler)MpvPlayerPage_PointerExited, true);
            AddHandler(UIElement.PointerEnteredEvent, (PointerEventHandler)MpvPlayerPage_PointerEntered, true);

            UpdatePlayControlAutoHide();
        }

        private void CleanupPlayControlAutoHide()
        {
            RemoveHandler(UIElement.PointerMovedEvent, (PointerEventHandler)MpvPlayerPage_PointerMoved);
            RemoveHandler(UIElement.PointerPressedEvent, (PointerEventHandler)MpvPlayerPage_PointerPressed);
            RemoveHandler(UIElement.PointerReleasedEvent, (PointerEventHandler)MpvPlayerPage_PointerReleased);
            RemoveHandler(UIElement.PointerCanceledEvent, (PointerEventHandler)MpvPlayerPage_PointerReleased);
            RemoveHandler(UIElement.PointerCaptureLostEvent, (PointerEventHandler)MpvPlayerPage_PointerCaptureLost);
            RemoveHandler(UIElement.PointerExitedEvent, (PointerEventHandler)MpvPlayerPage_PointerExited);
            RemoveHandler(UIElement.PointerEnteredEvent, (PointerEventHandler)MpvPlayerPage_PointerEntered);

            _playControlAutoHide?.Dispose();
            _playControlAutoHide = null;
            _pointerButtonPressed = false;
            _lastPointerPosition = null;
        }

        private void UpdatePlayControlAutoHide()
        {
            if (!_playerControlAutoHideEnabled)
            {
                _playControlAutoHide?.Stop();
                return;
            }

            if (_isFullWindow || _isFullScreen)
            {
                _playControlAutoHide?.Start();
            }
            else
            {
                _playControlAutoHide?.Stop();
            }
        }

        private PlayControlAutoHideController.AutoHideBlockReason GetPlayControlAutoHideBlockReason()
        {
            var reason = PlayControlAutoHideController.AutoHideBlockReason.None;

            if (_pointerButtonPressed)
            {
                reason |= PlayControlAutoHideController.AutoHideBlockReason.PointerButtonPressed;
            }

            if (!_windowActivated)
            {
                reason |= PlayControlAutoHideController.AutoHideBlockReason.WindowInactive;
            }

            if (_discMenuActive)
            {
                reason |= PlayControlAutoHideController.AutoHideBlockReason.DiscMenuActive;
            }

            if (IsPointerOverPlayControl())
            {
                reason |= PlayControlAutoHideController.AutoHideBlockReason.PointerOverPlayControl;
            }

            if (IsAnyPopupOpen())
            {
                reason |= PlayControlAutoHideController.AutoHideBlockReason.PopupOpen;
            }

            return reason;
        }

        private bool IsPointerOverPlayControl()
        {
            if (_lastPointerPosition is not { } pointerPosition)
            {
                return false;
            }

            try
            {
                var hostPosition = TransformToVisual(null).TransformPoint(pointerPosition);
                return VisualTreeHelper.FindElementsInHostCoordinates(hostPosition, PlayerControl).Any();
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        private bool IsAnyPopupOpen()
        {
            var xamlRoot = XamlRoot;
            if (xamlRoot is null)
            {
                return false;
            }

            return VisualTreeHelper.GetOpenPopupsForXamlRoot(xamlRoot).Count > 0;
        }

        private void MpvPlayerPage_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            _lastPointerPosition = e.GetCurrentPoint(this).Position;
            _playControlAutoHide?.OnPointerActivity();
        }

        private void MpvPlayerPage_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _lastPointerPosition = e.GetCurrentPoint(this).Position;
            _pointerButtonPressed = true;
            _playControlAutoHide?.OnPointerActivity();
        }

        private void MpvPlayerPage_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _pointerButtonPressed = false;
            _playControlAutoHide?.OnPointerActivity();
        }

        private void MpvPlayerPage_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            _pointerButtonPressed = false;
            _playControlAutoHide?.OnPointerActivity();
        }

        private void MpvPlayerPage_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            _lastPointerPosition = null;
        }

        private void MpvPlayerPage_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(this);

            _lastPointerPosition = point.Position;
            _pointerButtonPressed =
                point.Properties.IsLeftButtonPressed ||
                point.Properties.IsMiddleButtonPressed ||
                point.Properties.IsRightButtonPressed;

            _playControlAutoHide?.OnPointerActivity();
        }
    }
}
