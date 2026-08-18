using Microsoft.UI.Xaml;
using mpv;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using static Windows.Win32.PInvoke;

namespace mpv_winui.Modules.Player
{
    public sealed partial class MpvPlayerPage
    {
        private HHOOK? _hHook;
        private static bool _suppressKeyboard = false;
        private static bool _windowActivated = false;

        private unsafe void SetupKeyboardInput()
        {
            _hHook = SetWindowsHookEx(WINDOWS_HOOK_ID.WH_KEYBOARD, &MessageHookProc, HINSTANCE.Null, GetCurrentThreadId());

            //TODO
            App.Window?.Activated += Window_Activated;
            _windowActivated = true;
        }

        private void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
            if (args.WindowActivationState == WindowActivationState.Deactivated)
            {
                _windowActivated = false;
                SendAllKeyUp();
            }
            else
            {
                _windowActivated = true;
            }
        }

        private void CleanupKeyboardInput()
        {
            App.Window?.Activated -= Window_Activated;

            if (_hHook is { IsNull: false } hHook)
            {
                UnhookWindowsHookEx(hHook);
            }
            _hHook = null;
            _selfWeakReference = null;
        }

        private static void SendKeydown(string keyName)
        {
            if (string.IsNullOrEmpty(keyName))
            {
                return;
            }

            if (_logger.IsTraceEnabled)
            {
                var code = Keycodes.mp_input_get_key_from_name(keyName);
                _logger.Debug("keydown: mpv-name={}, mpv-code={}", keyName, code);
            }

            if (_selfWeakReference?.TryGetTarget(out var self) == true)
            {
                self?._mediaPlayer?.Command(["keydown", keyName]);
            }
        }

        private static void SendKeyup(string keyName)
        {
            if (string.IsNullOrEmpty(keyName))
            {
                return;
            }

            if (_selfWeakReference?.TryGetTarget(out var self) == true)
            {
                self?._mediaPlayer?.Command(["keyup", keyName]);
            }
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
        private static LRESULT MessageHookProc(int nCode, WPARAM wParam, LPARAM lParam)
        {
            if (!_suppressKeyboard && _windowActivated && nCode == 0)
            {
                uint flags = (uint)lParam.Value;
                uint vkey = (uint)wParam.Value;

                if ((flags & (1U << 31)) != 0)
                {
                    HandleKeyUp(vkey);

                    if (vkey == WinUser.VK_F10)
                    {
                        return (LRESULT)1;
                    }
                }
                else
                {
                    uint scancode = HIWORD(flags);
                    bool isSystemKey = (flags & (1U << 29)) != 0;
                    if (isSystemKey)
                    {
                        if (vkey == WinUser.VK_SPACE)
                        {
                            return (LRESULT)0;
                        }

                        HandleKeyDown(vkey, scancode);

                        if (vkey == WinUser.VK_F10)
                        {
                            return (LRESULT)1;
                        }
                    }
                    else
                    {
                        HandleKeyDown(vkey, scancode);
                    }
                }
            }

            if (_selfWeakReference?.TryGetTarget(out var self) == true)
            {
                if (self?._hHook is { IsNull: false } hHook)
                {
                    return CallNextHookEx(hHook, nCode, wParam, lParam);
                }
            }

            return (LRESULT)0;
        }


        private static void HandleKeyDown(uint vkey, uint scancode)
        {
            int mpkey = W32Keyboard.mp_w32_vkey_to_mpkey((int)vkey, (scancode & KF_EXTENDED) != 0);
            if (mpkey == 0)
            {
                mpkey = W32Common.decode_key(vkey, scancode & (0xff | KF_EXTENDED));
                if (mpkey == 0)
                {
                    return;
                }
            }

            if (_logger.IsTraceEnabled)
            {
                var keyName = Keycodes.mp_input_get_key_name(mpkey);
                _logger.Debug("keydown: key={}, mpv-key={}, key-name={}", vkey, mpkey, keyName);
            }

            SendKeydown(ModPrefix() + $"0x{mpkey:X}");
        }

        private static void HandleKeyUp(uint key)
        {
            if (_logger.IsTraceEnabled)
            {
                _logger.Debug("keyup: key={}", key);
            }

            switch (key)
            {
                case WinUser.VK_MENU:
                case WinUser.VK_CONTROL:
                case WinUser.VK_SHIFT:
                    break;
                default:
                {
                    SendKeyup($"0x{Keycodes.MP_INPUT_RELEASE_ALL:X}");
                    break;
                }
            }
        }

        private void SendAllKeyUp()
        {
            SendKeyup($"0x{Keycodes.MP_INPUT_RELEASE_ALL:X}");
        }

        private static string ModPrefix()
        {
            int mod = W32Common.mod_state();
            var prefix = "";

            if ((mod & Keycodes.MP_KEY_MODIFIER_SHIFT) != 0)
            {
                prefix += "Shift+";
            }

            if ((mod & Keycodes.MP_KEY_MODIFIER_CTRL) != 0)
            {
                prefix += "Ctrl+";
            }

            if ((mod & Keycodes.MP_KEY_MODIFIER_ALT) != 0)
            {
                prefix += "Alt+";
            }

            return prefix;
        }

    }
}
