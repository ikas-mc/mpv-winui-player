using mpv;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using static Windows.Win32.PInvoke;

namespace mpv_winui.Modules.Player
{
    public sealed partial class MpvPlayerPage
    {

        private HHOOK? _hHook;
        private bool _suppressKeyboard = false;
        private HOOKPROC? _hookDelegate;

        private void SetupWindowHook()
        {
            _hookDelegate = MessageHookProc;
            _hHook = SetWindowsHookEx(WINDOWS_HOOK_ID.WH_KEYBOARD, _hookDelegate, HINSTANCE.Null, GetCurrentThreadId());
        }

        private void RemoveWindowHook()
        {
            if (_hHook is HHOOK hHook && !hHook.IsNull)
            {
                UnhookWindowsHookEx(hHook);
            }
            _hHook = null;
            _hookDelegate = null;
        }

        private void SendKeydown(string keyName)
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

            _mediaPlayer?.Command(["keydown", keyName]);
        }

        private void SendKeyup(string keyName)
        {
            if (string.IsNullOrEmpty(keyName))
            {
                return;
            }

            _mediaPlayer?.Command(["keyup", keyName]);
        }

        private LRESULT MessageHookProc(int nCode, WPARAM wParam, LPARAM lParam)
        {
            if (!_suppressKeyboard && nCode == 0)
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
                    uint scancode = PInvoke.HIWORD(flags);
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


            if (_hHook is HHOOK hHook && !hHook.IsNull)
            {
                return CallNextHookEx(hHook, nCode, wParam, lParam);
            }

            return (LRESULT)0;
        }


        public void HandleKeyDown(uint vkey, uint scancode)
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

        public void HandleKeyUp(uint vkey)
        {
            if (_logger.IsTraceEnabled)
            {
                _logger.Debug("keyup: key={}", vkey);
            }

            switch (vkey)
            {
                case WinUser.VK_MENU:
                case WinUser.VK_CONTROL:
                case WinUser.VK_SHIFT:
                    break;
                default:
                {
                    // Releasing all keys on key-up is simpler and ensures no keys can be
                    // get "stuck." This matches the behaviour of other VOs.
                    SendKeyup(ModPrefix() + $"0x{Keycodes.MP_INPUT_RELEASE_ALL:X}");
                    break;
                }
            }
        }

        private string ModPrefix()
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
