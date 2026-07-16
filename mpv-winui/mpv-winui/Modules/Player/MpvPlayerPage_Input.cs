using NLog;
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

        // VK codes
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
        private const int VK_PACKET = 0xE7;
        private const int VK_PROCESSKEY = 0xE5;

        private nint _hHook = nint.Zero;
        private Win32HookProc _hookDelegate = null!;
        private int _highSurrogate;

        private const string MOD_SHIFT = "Shift+";
        private const string MOD_CTRL = "Ctrl+";
        private const string MOD_ALT = "Alt+";
        private const string MOD_META = "Meta+";

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

        [LibraryImport("user32.dll", EntryPoint = "MapVirtualKeyW")]
        private static partial uint MapVirtualKey(uint uCode, uint uMapType);

        [LibraryImport("user32.dll", EntryPoint = "ToUnicode", StringMarshalling = StringMarshalling.Utf16)]
        private static partial int ToUnicode(uint wVirtKey, uint wScanCode, byte[] lpKeyState, char[] pwszBuff, int cchBuff, uint wFlags);

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

        private bool IsModifier(int vk)
        {
            return vk == VK_SHIFT || vk == VK_LSHIFT || vk == VK_RSHIFT
                || vk == VK_CONTROL || vk == VK_LCONTROL || vk == VK_RCONTROL
                || vk == VK_MENU || vk == VK_LMENU || vk == VK_RMENU
                || vk == VK_LWIN || vk == VK_RWIN;
        }

        private static bool KeyStateDown(int vk)
        {
            return (GetKeyState(vk) & 0x8000) != 0;
        }

        private string BuildModifierPrefix()
        {
            bool altGr = KeyStateDown(VK_RMENU) && KeyStateDown(VK_LCONTROL);
            var prefix = "";

            if (KeyStateDown(VK_SHIFT))
                prefix += MOD_SHIFT;
            if (KeyStateDown(VK_RCONTROL) || (KeyStateDown(VK_LCONTROL) && !altGr))
                prefix += MOD_CTRL;
            if (KeyStateDown(VK_LMENU) || (KeyStateDown(VK_RMENU) && !altGr))
                prefix += MOD_ALT;
            if (KeyStateDown(VK_LWIN) || KeyStateDown(VK_RWIN))
                prefix += MOD_META;

            return prefix;
        }

        private void SendToMpv(string keyName, bool isDown)
        {
            if (string.IsNullOrEmpty(keyName))
                return;

            var cmd = isDown ? "keydown" : "keyup";
            _mediaPlayer?.Command([cmd, keyName]);
        }

        private void ReleaseAllKeys()
        {
            _mediaPlayer?.Command(["keyup", ""]);
        }

        private nint MessageHookProc(int nCode, nint wParam, nint lParam)
        {
            if (nCode >= 0 && (int)wParam == PM_REMOVE)
            {
                var msg = Marshal.PtrToStructure<MSG>(lParam);

                if (msg.message == WM_CHAR || msg.message == WM_SYSCHAR)
                {
                    HandleChar((int)msg.wParam);
                }
                else if (msg.message == WM_UNICHAR)
                {
                    if ((int)msg.wParam == UNICODE_NOCHAR)
                        return (nint)1;
                    HandleChar((int)msg.wParam);
                }
                else if (msg.message == WM_KEYDOWN || msg.message == WM_SYSKEYDOWN)
                {
                    var vk = (int)msg.wParam;
                    if (vk == VK_PACKET)
                    {
                        HandleChar((int)msg.wParam);
                    }
                    else if (vk != VK_PROCESSKEY && !IsModifier(vk))
                    {
                        if (!HandleKeyDown(vk))
                        {
                            var extended = (((int)msg.lParam >> 16) & KF_EXTENDED) != 0;
                            var keyName = VkCodeToMpvKeyName(vk, extended);
                            if (keyName != null)
                            {
                                SendToMpv(BuildModifierPrefix() + keyName, true);
                            }
                        }
                    }
                }
                else if (msg.message == WM_KEYUP || msg.message == WM_SYSKEYUP)
                {
                    var vk = (int)msg.wParam;
                    if (vk != VK_PACKET && !IsModifier(vk))
                    {
                        if (!HandleKeyUp(vk))
                        {
                            if (vk == VK_LWIN || vk == VK_RWIN)
                            {
                                ReleaseAllKeys();
                            }
                            else
                            {
                                var extended = (((int)msg.lParam >> 16) & KF_EXTENDED) != 0;
                                var keyName = VkCodeToMpvKeyName(vk, extended);
                                if (keyName != null)
                                {
                                    SendToMpv(BuildModifierPrefix() + keyName, false);
                                }
                            }
                        }
                    }
                }
            }

            return CallNextHookEx(_hHook, nCode, wParam, lParam);
        }

        private void HandleChar(int code)
        {
            if (code <= 0)
                return;

            if (code >= 0x10000)
            {
                var hi = (code - 0x10000) >> 10;
                var lo = (code - 0x10000) & 0x3FF;
                HandleChar(0xD800 | hi);
                HandleChar(0xDC00 | lo);
                return;
            }

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
                    SendCharToMpv(code);
                }
                return;
            }

            _highSurrogate = 0;

            if (code >= 0x20)
            {
                SendCharToMpv(code);
            }
        }

        private void SendCharToMpv(int code)
        {
            var prefix = BuildModifierPrefix();

            if (prefix.Length == 0)
            {
                // Direct Unicode character without modifiers
                var utf8 = char.ConvertFromUtf32(code);
                _mediaPlayer?.Command(["keypress", utf8]);
            }
            else
            {
                // With modifiers, use hex keycode to avoid UTF-8 + modifier issues
                _mediaPlayer?.Command(["keydown", $"{prefix}0x{code:X}"]);
            }
        }

        private bool HandleKeyDown(int vkCode)
        {
            switch (vkCode)
            {
                case 0x7A: //F11
                    return true;
                default:
                    return false;
            }
        }

        private bool HandleKeyUp(int vkCode)
        {
            switch (vkCode)
            {
                case 0x7A: //F11
                {
                    PlayerControl.ToggleFullScreen();
                    return true;
                }
                default:
                    return false;
            }
        }

        private static string? VkCodeToMpvKeyName(int vk, bool extended)
        {
            // Only numpad Enter has extended flag and needs KP_ prefix;
            // nav keys (arrows, pgup/pgdn, home/end, ins/del) use non-KP names regardless.
            if (extended && vk == 0x0D)
                return "KP_ENTER";

            // Letter keys
            if (vk >= 'A' && vk <= 'Z')
                return ((char)(vk + 0x20)).ToString();

            // Number keys (above letter keys, not numpad)
            if (vk >= '0' && vk <= '9')
                return ((char)vk).ToString();

            // Function keys F1-F24
            if (vk >= 0x70 && vk <= 0x87)
                return "F" + (vk - 0x70 + 1);

            // Numpad 0-9
            if (vk >= 0x60 && vk <= 0x69)
                return "KP" + ((char)('0' + (vk - 0x60))).ToString();

            return vk switch
            {
                0x08 => "BS",           // Backspace
                0x09 => "TAB",          // Tab
                0x0C => "KP_BEGIN",     // VK_CLEAR (numpad 5 with numlock off)
                0x0D => "ENTER",        // Enter (regular)
                0x1B => "ESC",          // Escape
                0x20 => "SPACE",        // Spacebar
                0x21 => "PGUP",         // Page Up
                0x22 => "PGDWN",        // Page Down
                0x23 => "END",          // End
                0x24 => "HOME",         // Home
                0x25 => "LEFT",         // Left Arrow
                0x26 => "UP",           // Up Arrow
                0x27 => "RIGHT",        // Right Arrow
                0x28 => "DOWN",         // Down Arrow
                0x2D => "INS",          // Insert
                0x2E => "DEL",          // Delete
                0x5D => "MENU",         // Context Menu

                0x6A => "KP_MULTIPLY",  // Numpad *
                0x6B => "KP_ADD",       // Numpad +
                0x6C => "KP_DEC",       // Numpad Separator (often same as decimal)
                0x6D => "KP_SUBTRACT",  // Numpad -
                0x6E => "KP_DEC",       // Numpad .
                0x6F => "KP_DIVIDE",    // Numpad /

                // OEM keys (US layout mapping; actual char may vary by layout)
                0xBA => ";",
                0xBB => "=",
                0xBC => ",",
                0xBD => "-",
                0xBE => ".",
                0xBF => "/",
                0xC0 => "`",
                0xDB => "[",
                0xDC => "\\",
                0xDD => "]",
                0xDE => "'",
                0xE2 => "\\",

                0x90 => "NUMLOCK",
                0x91 => "SCROLLLOCK",
                0x14 => "CAPSLOCK",
                0x13 => "PAUSE",
                0x2A => "PRINT",

                // Media / special keys
                0xAD => "MUTE",
                0xAE => "VOLUME_DOWN",
                0xAF => "VOLUME_UP",
                0xB0 => "NEXT",
                0xB1 => "PREV",
                0xB2 => "STOP",
                0xB3 => "PLAY",
                0xA6 => "GO_BACK",       // VK_BROWSER_BACK
                0xA7 => "GO_FORWARD",    // VK_BROWSER_FORWARD

                _ => null
            };
        }
    }
}
