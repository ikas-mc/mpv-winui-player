using System.Runtime.InteropServices;

namespace mpv_winui.Modules.Player
{
    public sealed partial class MpvPlayerPage
    {
        private const int WH_GETMESSAGE = 3;
        private const int PM_REMOVE = 1;

        private const uint WM_KEYDOWN = 0x100;
        private const uint WM_KEYUP = 0x101;
        private const uint WM_CHAR = 0x102;
        private const uint WM_SYSKEYDOWN = 0x104;
        private const uint WM_SYSKEYUP = 0x105;
        private const uint WM_SYSCHAR = 0x106;
        private const uint WM_UNICHAR = 0x109;
        private const uint WM_APPCOMMAND = 0x319;

        private const int KF_EXTENDED = 0x0100;
        private const int UNICODE_NOCHAR = 0xFFFF;

        // mpv internal key codes (from input/keycodes.h)
        private const int MP_KEY_BASE = 1 << 21;
        private const int MP_KEY_BS = MP_KEY_BASE + 0;
        private const int MP_KEY_DEL = MP_KEY_BASE + 1;
        private const int MP_KEY_INS = MP_KEY_BASE + 2;
        private const int MP_KEY_HOME = MP_KEY_BASE + 3;
        private const int MP_KEY_END = MP_KEY_BASE + 4;
        private const int MP_KEY_PGUP = MP_KEY_BASE + 5;
        private const int MP_KEY_PGDWN = MP_KEY_BASE + 6;
        private const int MP_KEY_ESC = MP_KEY_BASE + 7;
        private const int MP_KEY_PRINT = MP_KEY_BASE + 8;
        private const int MP_KEY_CRSR = MP_KEY_BASE + 0x10;
        private const int MP_KEY_RIGHT = MP_KEY_CRSR + 0;
        private const int MP_KEY_LEFT = MP_KEY_CRSR + 1;
        private const int MP_KEY_DOWN = MP_KEY_CRSR + 2;
        private const int MP_KEY_UP = MP_KEY_CRSR + 3;
        private const int MP_KEY_MM_BASE = MP_KEY_BASE + 0x20;
        private const int MP_KEY_MENU = MP_KEY_MM_BASE + 1;
        private const int MP_KEY_PLAY = MP_KEY_MM_BASE + 2;
        private const int MP_KEY_PAUSE = MP_KEY_MM_BASE + 3;
        private const int MP_KEY_STOP = MP_KEY_MM_BASE + 5;
        private const int MP_KEY_NEXT = MP_KEY_MM_BASE + 8;
        private const int MP_KEY_PREV = MP_KEY_MM_BASE + 9;
        private const int MP_KEY_VOLUME_UP = MP_KEY_MM_BASE + 10;
        private const int MP_KEY_VOLUME_DOWN = MP_KEY_MM_BASE + 11;
        private const int MP_KEY_MUTE = MP_KEY_MM_BASE + 12;
        private const int MP_KEY_SLEEP = MP_KEY_MM_BASE + 18;
        private const int MP_KEY_GO_BACK = MP_KEY_MM_BASE + 25;
        private const int MP_KEY_GO_FORWARD = MP_KEY_MM_BASE + 26;
        private const int MP_KEY_F = MP_KEY_BASE + 0x40;
        private const int MP_KEY_KEYPAD = MP_KEY_BASE + 0x60;
        private const int MP_KEY_KPDEC = MP_KEY_KEYPAD + 10;
        private const int MP_KEY_KPINS = MP_KEY_KEYPAD + 11;
        private const int MP_KEY_KPDEL = MP_KEY_KEYPAD + 12;
        private const int MP_KEY_KPENTER = MP_KEY_KEYPAD + 13;
        private const int MP_KEY_KPHOME = MP_KEY_KEYPAD + 14;
        private const int MP_KEY_KPEND = MP_KEY_KEYPAD + 15;
        private const int MP_KEY_KPPGUP = MP_KEY_KEYPAD + 16;
        private const int MP_KEY_KPPGDWN = MP_KEY_KEYPAD + 17;
        private const int MP_KEY_KPRIGHT = MP_KEY_KEYPAD + 18;
        private const int MP_KEY_KPLEFT = MP_KEY_KEYPAD + 19;
        private const int MP_KEY_KPDOWN = MP_KEY_KEYPAD + 20;
        private const int MP_KEY_KPUP = MP_KEY_KEYPAD + 21;
        private const int MP_KEY_KPBEGIN = MP_KEY_KEYPAD + 22;
        private const int MP_KEY_KPADD = MP_KEY_KEYPAD + 23;
        private const int MP_KEY_KPSUBTRACT = MP_KEY_KEYPAD + 24;
        private const int MP_KEY_KPMULTIPLY = MP_KEY_KEYPAD + 25;
        private const int MP_KEY_KPDIVIDE = MP_KEY_KEYPAD + 26;

        // mpv modifier flags
        private const int MP_KEY_MODIFIER_SHIFT = 1 << 24;
        private const int MP_KEY_MODIFIER_CTRL = 1 << 25;
        private const int MP_KEY_MODIFIER_ALT = 1 << 26;

        // VK codes (from win32)
        private const int VK_SHIFT = 0x10;
        private const int VK_LSHIFT = 0xA0;
        private const int VK_RSHIFT = 0xA1;
        private const int VK_CONTROL = 0x11;
        private const int VK_LCONTROL = 0xA2;
        private const int VK_RCONTROL = 0xA3;
        private const int VK_MENU = 0x12;
        private const int VK_LMENU = 0xA4;
        private const int VK_RMENU = 0xA5;
        private const int VK_LWIN = 0x5B;
        private const int VK_RWIN = 0x5C;

        private nint _hHook = nint.Zero;
        private bool _suppressKeyboard;
        private Win32HookProc _hookDelegate = null!;
        private int _highSurrogate;

        [LibraryImport("user32.dll", EntryPoint = "SetWindowsHookExW", StringMarshalling = StringMarshalling.Utf16)]
        private static partial nint SetWindowsHookEx(int idHook, Win32HookProc lpfn, nint hMod, uint dwThreadId);

        [LibraryImport("user32.dll", EntryPoint = "UnhookWindowsHookEx")]
        private static partial nint UnhookWindowsHookEx(nint hhk);

        [LibraryImport("user32.dll", EntryPoint = "CallNextHookEx")]
        private static partial nint CallNextHookEx(nint hhk, int nCode, nint wParam, nint lParam);

        [LibraryImport("kernel32.dll", EntryPoint = "GetCurrentThreadId")]
        private static partial uint GetCurrentThreadId();

        [LibraryImport("user32.dll", EntryPoint = "GetKeyState")]
        private static partial short GetKeyState(int nVirtKey);

        private delegate nint Win32HookProc(int nCode, nint wParam, nint lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct MSG
        {
            public nint hwnd;
            public uint message;
            public nint wParam;
            public nint lParam;
            public uint time;
            public int ptX;
            public int ptY;
        }

        // --- Setup / Teardown ---

        private void SetupWindowHook()
        {
            _hookDelegate = MessageHookProc;
            _hHook = SetWindowsHookEx(WH_GETMESSAGE, _hookDelegate, nint.Zero, GetCurrentThreadId());
            if (_hHook == nint.Zero)
            {
                _logger.Error("SetWindowsHookEx failed");
            }
        }

        private void RemoveWindowHook()
        {
            if (_hHook == nint.Zero)
            {
                return;
            }

            UnhookWindowsHookEx(_hHook);
            _hHook = nint.Zero;
        }

        // --- Key state helpers (mirrors mpv's key_state / mod_state) ---

        private static bool KeyStateDown(int vk)
        {
            return (GetKeyState(vk) & 0x8000) != 0;
        }

        private static bool IsModifier(int vk)
        {
            return vk == VK_SHIFT || vk == VK_LSHIFT || vk == VK_RSHIFT
                || vk == VK_CONTROL || vk == VK_LCONTROL || vk == VK_RCONTROL
                || vk == VK_MENU || vk == VK_LMENU || vk == VK_RMENU
                || vk == VK_LWIN || vk == VK_RWIN;
        }

        // Mirrors mpv's mod_state() — returns MP_KEY_MODIFIER_* bits
        private int ModState()
        {
            int res = 0;
            bool altGr = KeyStateDown(VK_RMENU) && KeyStateDown(VK_LCONTROL);

            if (KeyStateDown(VK_RCONTROL) || (KeyStateDown(VK_LCONTROL) && !altGr))
            {
                res |= MP_KEY_MODIFIER_CTRL;
            }

            if (KeyStateDown(VK_SHIFT))
            {
                res |= MP_KEY_MODIFIER_SHIFT;
            }

            if (KeyStateDown(VK_LMENU) || (KeyStateDown(VK_RMENU) && !altGr))
            {
                res |= MP_KEY_MODIFIER_ALT;
            }

            return res;
        }

        // Converts modifier bits to string prefix for the "keydown"/"keyup" command
        private string ModPrefix()
        {
            int mod = ModState();
            var prefix = "";

            if ((mod & MP_KEY_MODIFIER_SHIFT) != 0)
            {
                prefix += "Shift+";
            }

            if ((mod & MP_KEY_MODIFIER_CTRL) != 0)
            {
                prefix += "Ctrl+";
            }

            if ((mod & MP_KEY_MODIFIER_ALT) != 0)
            {
                prefix += "Alt+";
            }

            if (KeyStateDown(VK_LWIN) || KeyStateDown(VK_RWIN))
            {
                prefix += "Meta+";
            }

            return prefix;
        }

        private void SendToMpv(string keyName, bool isDown)
        {
            if (string.IsNullOrEmpty(keyName))
            {
                return;
            }

            var cmd = isDown ? "keydown" : "keyup";
            _mediaPlayer?.Command([cmd, keyName]);
        }

        private void ReleaseAllKeys()
        {
            _mediaPlayer?.Command(["keyup", ""]);
        }

        // --- Key event handlers (mirrors mpv's handle_key_down / handle_key_up / handle_char) ---

        // Mirrors mpv's handle_key_down:
        //   1. Try mp_w32_vkey_to_mpkey (special keys)
        //   2. Fallback: decode_key (ToUnicode) — simplified to US layout mapping
        //   3. mp_input_put_key(mpkey | mod_state | MP_KEY_STATE_DOWN)
        private void HandleKeyDown(uint vkey, uint scancode)
        {
            bool extended = (scancode & KF_EXTENDED) != 0;
            int mpkey = VkCodeToMpvKeyCode((int)vkey, extended);

            // decode_key fallback: produce printable character from VK code
            if (mpkey == 0)
            {
                mpkey = VkCodeToUsChar((int)vkey);
                if (mpkey == 0)
                {
                    return;
                }
            }

            SendToMpv(ModPrefix() + $"0x{mpkey:X}", true);
        }

        // Mirrors mpv's handle_key_up: releases all keys for non-modifier key-ups
        private void HandleKeyUp(uint vkey)
        {
            switch (vkey)
            {
                case VK_MENU:
                case VK_CONTROL:
                case VK_SHIFT:
                    break;
                default:
                    ReleaseAllKeys();
                    break;
            }
        }

        // Mirrors mpv's handle_char:
        //   c = decode ? decode_utf16(wc) : wc
        //   if (c >= 0x20) mp_input_put_key(c | mod_state)
        private void HandleChar(int code, bool decode)
        {
            if (decode)
            {
                // UTF-16 decoding (mirrors mpv's decode_utf16)
                if (code >= 0xD800 && code <= 0xDBFF)
                {
                    _highSurrogate = code;
                    return;
                }

                if (code >= 0xDC00 && code <= 0xDFFF)
                {
                    if (_highSurrogate != 0)
                    {
                        code = 0x10000 + ((_highSurrogate - 0xD800) << 10) + (code - 0xDC00);
                        _highSurrogate = 0;
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    _highSurrogate = 0;
                }
            }

            if (code <= 0)
            {
                return;
            }

            if (code < 0x20)
            {
                return;
            }

            _mediaPlayer?.Command(["keypress", $"0x{code:X}"]);
        }

        // --- Hook procedure (mirrors mpv's WndProc keyboard section) ---

        private nint MessageHookProc(int nCode, nint wParam, nint lParam)
        {
            if (_suppressKeyboard)
            {
                return CallNextHookEx(_hHook, nCode, wParam, lParam);
            }

            if (nCode >= 0 && (int)wParam == PM_REMOVE)
            {
                var msg = Marshal.PtrToStructure<MSG>(lParam);

                // Mirrors mpv's WndProc keyboard handling order
                // (with special cases like mpv's Alt+Space / F10 handling)
                var vkey = (uint)msg.wParam;

                switch (msg.message)
                {
                    case WM_KEYDOWN:
                    case WM_SYSKEYDOWN:
                        HandleKeyDown(vkey, (uint)(msg.lParam >> 16) & 0xFFFF);
                        break;

                    case WM_KEYUP:
                    case WM_SYSKEYUP:
                        HandleKeyUp(vkey);
                        break;

                    case WM_CHAR:
                    case WM_SYSCHAR:
                        HandleChar((int)msg.wParam, true);
                        break;

                    case WM_UNICHAR:
                        if ((int)msg.wParam == UNICODE_NOCHAR)
                        {
                            return (nint)1;
                        }
                        HandleChar((int)msg.wParam, false);
                        break;
                }
            }

            return CallNextHookEx(_hHook, nCode, wParam, lParam);
        }

        // --- VK-to-mp keycode mapping (mirrors mpv's mp_w32_vkey_to_mpkey) ---
        private static int VkCodeToMpvKeyCode(int vk, bool extended)
        {
            // vk_map_ext from mpv's osdep/w32_keyboard.c
            // Extended flag is set for the navigation cluster and arrow keys,
            // so it differentiates them from the numpad.
            if (extended)
            {
                switch (vk)
                {
                    case 0x25:
                        return MP_KEY_LEFT;      // VK_LEFT
                    case 0x26:
                        return MP_KEY_UP;        // VK_UP
                    case 0x27:
                        return MP_KEY_RIGHT;     // VK_RIGHT
                    case 0x28:
                        return MP_KEY_DOWN;      // VK_DOWN
                    case 0x2D:
                        return MP_KEY_INS;       // VK_INSERT
                    case 0x2E:
                        return MP_KEY_DEL;       // VK_DELETE
                    case 0x24:
                        return MP_KEY_HOME;      // VK_HOME
                    case 0x23:
                        return MP_KEY_END;       // VK_END
                    case 0x21:
                        return MP_KEY_PGUP;      // VK_PRIOR
                    case 0x22:
                        return MP_KEY_PGDWN;     // VK_NEXT
                    case 0x0D:
                        return MP_KEY_KPENTER;   // VK_RETURN (numpad enter)
                }

                // If extended and not found, fall through to vk_map (mpv behavior)
            }

            // vk_map from mpv's osdep/w32_keyboard.c
            switch (vk)
            {
                case 0x1B:
                    return MP_KEY_ESC;           // VK_ESCAPE
                case 0x08:
                    return MP_KEY_BS;            // VK_BACK
                case 0x09:
                    return 9;                    // MP_KEY_TAB
                case 0x0D:
                    return 13;                   // MP_KEY_ENTER
                case 0x13:
                    return MP_KEY_PAUSE;         // VK_PAUSE
                case 0x5F:
                    return MP_KEY_SLEEP;         // VK_SLEEP
                case 0x2C:
                    return MP_KEY_PRINT;         // VK_SNAPSHOT
                case 0x5D:
                    return MP_KEY_MENU;          // VK_APPS
            }

            // Function keys F1-F24
            if (vk >= 0x70 && vk <= 0x87)
            {
                return MP_KEY_F + vk - 0x70 + 1;
            }

            // Numpad independent of numlock
            if (vk == 0x6D)
            {
                return MP_KEY_KPSUBTRACT;  // VK_SUBTRACT
            }

            if (vk == 0x6B)
            {
                return MP_KEY_KPADD;       // VK_ADD
            }

            if (vk == 0x6A)
            {
                return MP_KEY_KPMULTIPLY;  // VK_MULTIPLY
            }

            if (vk == 0x6F)
            {
                return MP_KEY_KPDIVIDE;    // VK_DIVIDE
            }

            // Numpad with numlock
            if (vk >= 0x60 && vk <= 0x69)
            {
                return MP_KEY_KEYPAD + (vk - 0x60);    // VK_NUMPAD0-9 → KP0-9
            }

            if (vk == 0x6E)
            {
                return MP_KEY_KPDEC;       // VK_DECIMAL
            }

            // Numpad without numlock (non-extended nav keys from numpad)
            if (vk == 0x2D)
            {
                return MP_KEY_KPINS;       // VK_INSERT
            }

            if (vk == 0x23)
            {
                return MP_KEY_KPEND;       // VK_END
            }

            if (vk == 0x28)
            {
                return MP_KEY_KPDOWN;      // VK_DOWN
            }

            if (vk == 0x22)
            {
                return MP_KEY_KPPGDWN;     // VK_NEXT
            }

            if (vk == 0x25)
            {
                return MP_KEY_KPLEFT;      // VK_LEFT
            }

            if (vk == 0x0C)
            {
                return MP_KEY_KPBEGIN;     // VK_CLEAR
            }

            if (vk == 0x27)
            {
                return MP_KEY_KPRIGHT;     // VK_RIGHT
            }

            if (vk == 0x24)
            {
                return MP_KEY_KPHOME;      // VK_HOME
            }

            if (vk == 0x26)
            {
                return MP_KEY_KPUP;        // VK_UP
            }

            if (vk == 0x21)
            {
                return MP_KEY_KPPGUP;      // VK_PRIOR
            }

            if (vk == 0x2E)
            {
                return MP_KEY_KPDEL;       // VK_DELETE
            }

            return 0;
        }

        // Simplified decode_key fallback: returns US layout printable character
        private static int VkCodeToUsChar(int vk)
        {
            if (vk >= 'A' && vk <= 'Z')
            {
                return vk + 0x20;
            }

            if (vk >= '0' && vk <= '9')
            {
                return vk;
            }

            return vk switch
            {
                0x20 => ' ',
                0xBA => ';',
                0xBB => '=',
                0xBC => ',',
                0xBD => '-',
                0xBE => '.',
                0xBF => '/',
                0xC0 => '`',
                0xDB => '[',
                0xDC => '\\',
                0xDD => ']',
                0xDE => '\'',
                0xE2 => '\\',
                _ => 0
            };
        }
    }
}
