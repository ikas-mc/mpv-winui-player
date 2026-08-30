using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Input;
using mpv;
using System;
using Windows.Foundation;

namespace mpv_winui.Modules.Player
{
    public sealed partial class MpvPlayerPage
    {
        private const int WheelDelta = 120;
        private const int MouseMoveIntervalMs = 50;

        private static bool _suppressMouse = false;

        private long _lastMouseMoveSent = 0;
        private bool _enableMouseInput;
        private bool _enableMouseInputDiscNavOnly;

        private void SetupMouseInput()
        {
            _enableMouseInput = AppContext.AppSetting.EnableMouseInput;
            if (_enableMouseInput)
            {
                PlayerView.PointerEntered += PlayerView_PointerEntered;
                PlayerView.PointerMoved += PlayerView_PointerMoved;
                PlayerView.PointerPressed += PlayerView_PointerPressed;
                PlayerView.PointerReleased += PlayerView_PointerReleased;
                PlayerView.PointerExited += PlayerView_PointerExited;
                PlayerView.PointerWheelChanged += PlayerView_PointerWheelChanged;
            }

            _enableMouseInputDiscNavOnly = AppContext.AppSetting.EnableMouseInputDiscNavOnly;

            if (_logger.IsDebugEnabled)
            {
                _logger.Debug("Setup Mouse Input, enableMouseInput={}, enableMouseInputDiscNavOnly={}", _enableMouseInput, _enableMouseInputDiscNavOnly);
            }
        }

        private void CleanupMouseInput()
        {
            PlayerView.PointerEntered -= PlayerView_PointerEntered;
            PlayerView.PointerMoved -= PlayerView_PointerMoved;
            PlayerView.PointerPressed -= PlayerView_PointerPressed;
            PlayerView.PointerReleased -= PlayerView_PointerReleased;
            PlayerView.PointerExited -= PlayerView_PointerExited;
            PlayerView.PointerWheelChanged -= PlayerView_PointerWheelChanged;

            _lastMouseMoveSent = 0;
            _enableMouseInput = false;
        }

        private bool ShouldSendMouseMove()
        {
            long now = Environment.TickCount64;
            if (now - _lastMouseMoveSent < MouseMoveIntervalMs)
            {
                return false;
            }

            _lastMouseMoveSent = now;
            return true;
        }

        private bool IgnoreInput()
        {
            if (_suppressMouse || !_windowActivated)
            {
                return true;
            }

            if (_enableMouseInputDiscNavOnly && !_discMenuActive)
            {
                return true;
            }

            return false;
        }

        private void PlayerView_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (IgnoreInput())
            {
                return;
            }

            if (ShouldSendMouseMove())
            {
                var (x, y) = GetVideoPoint(e.GetCurrentPoint(PlayerView).Position);
                SendMouseMove(x, y);
            }
        }

        private void PlayerView_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (IgnoreInput())
            {
                return;
            }

            var kind = e.GetCurrentPoint(PlayerView).Properties.PointerUpdateKind;
            if (!IsMouseButtonEvent(kind))
            {
                return;
            }

            var code = GetButtonCode(kind);
            var (x, y) = GetVideoPoint(e.GetCurrentPoint(PlayerView).Position);
            SendMouseMove(x, y);
            SendMouseButtonDown(code);
        }

        private void PlayerView_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (IgnoreInput())
            {
                return;
            }

            var kind = e.GetCurrentPoint(PlayerView).Properties.PointerUpdateKind;
            if (!IsMouseButtonEvent(kind))
            {
                return;
            }

            var code = GetButtonCode(kind);
            SendMouseButtonUp(code);
        }

        private void PlayerView_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (IgnoreInput())
            {
                return;
            }

            SendMouseEnter();
        }

        private void PlayerView_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (IgnoreInput())
            {
                return;
            }

            SendMouseLeave();
        }

        private void PlayerView_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (IgnoreInput())
            {
                return;
            }

            var point = e.GetCurrentPoint(PlayerView);
            var (x, y) = GetVideoPoint(point.Position);
            int delta = point.Properties.MouseWheelDelta;
            if (delta == 0)
            {
                return;
            }

            var button = GetWheelButtonCode(point.Properties.IsHorizontalMouseWheel, delta);
            SendMouseWheel(x, y, button, delta);
        }

        private (int X, int Y) GetVideoPoint(Point position)
        {
            double scale = PlayerView.XamlRoot?.RasterizationScale ?? 1.0;
            return ((int)Math.Round(position.X * scale), (int)Math.Round(position.Y * scale));
        }

        private (double Width, double Height) GetVideoSize()
        {
            double scale = PlayerView.XamlRoot?.RasterizationScale ?? 1.0;
            return (PlayerView.ActualWidth * scale, PlayerView.ActualHeight * scale);
        }

        private bool IsMouseButtonEvent(PointerUpdateKind kind)
        {
            return kind switch
            {
                PointerUpdateKind.LeftButtonPressed => true,
                PointerUpdateKind.LeftButtonReleased => true,
                PointerUpdateKind.RightButtonPressed => true,
                PointerUpdateKind.RightButtonReleased => true,
                PointerUpdateKind.MiddleButtonPressed => true,
                PointerUpdateKind.MiddleButtonReleased => true,
                PointerUpdateKind.XButton1Pressed => true,
                PointerUpdateKind.XButton1Released => true,
                PointerUpdateKind.XButton2Pressed => true,
                PointerUpdateKind.XButton2Released => true,
                _ => false,
            };
        }

        private int GetButtonCode(PointerUpdateKind kind)
        {
            return kind switch
            {
                PointerUpdateKind.LeftButtonPressed or PointerUpdateKind.LeftButtonReleased => Keycodes.MP_MBTN_LEFT_ABS,
                PointerUpdateKind.RightButtonPressed or PointerUpdateKind.RightButtonReleased => Keycodes.MP_MBTN_RIGHT_ABS,
                PointerUpdateKind.MiddleButtonPressed or PointerUpdateKind.MiddleButtonReleased => Keycodes.MP_MBTN_MID_ABS,
                PointerUpdateKind.XButton1Pressed or PointerUpdateKind.XButton1Released => Keycodes.MP_MBTN_BACK_ABS,
                PointerUpdateKind.XButton2Pressed or PointerUpdateKind.XButton2Released => Keycodes.MP_MBTN_FORWARD_ABS,
                _ => Keycodes.MP_KEY_UNMAPPED,
            };
        }

        private int GetWheelButtonCode(bool isHorizontal, int delta)
        {
            if (isHorizontal)
            {
                return delta > 0 ? Keycodes.MP_WHEEL_RIGHT : Keycodes.MP_WHEEL_LEFT;
            }

            return delta > 0 ? Keycodes.MP_WHEEL_UP : Keycodes.MP_WHEEL_DOWN;
        }

        private void SendMouseMove(int x, int y)
        {
            _mediaPlayer?.Command(["mouse", x.ToString(), y.ToString()]);

            if (_logger.IsTraceEnabled)
            {
                _logger.Debug("Send Mouse Move, x={}, y={}", x, y);
            }
        }

        private void SendMouseEnter()
        {
            SendKeydown($"0x{Keycodes.MP_KEY_MOUSE_ENTER:X}");
        }

        private void SendMouseLeave()
        {
            SendKeydown($"0x{Keycodes.MP_KEY_MOUSE_LEAVE:X}");
        }

        private void SendMouseButtonDown(int code)
        {
            SendKeydown($"0x{code:X}");
        }

        private void SendMouseButtonUp(int code)
        {
            SendKeyup($"0x{code:X}");
        }

        private void SendMouseWheel(int x, int y, int button, int delta)
        {
            int count = Math.Abs(delta) / WheelDelta;
            for (int i = 0; i < count; i++)
            {
                _mediaPlayer?.Command(["mouse", x.ToString(), y.ToString(), button.ToString()]);
            }

            if (_logger.IsTraceEnabled)
            {
                var keyName = Keycodes.mp_input_get_key_name(button);
                _logger.Debug("Send Mouse Wheel, x={}, y={}, key={}, count={}", x, y, keyName, count);
            }
        }

    }
}