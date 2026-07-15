using System.Runtime.InteropServices;

namespace mpv_winui.Modules.Player
{
    public sealed partial class MpvPlayerPage
    {
        private const int WH_GETMESSAGE = 3;
        private const int PM_REMOVE = 1;
        private const uint WM_KEYDOWN = 0x100;
        private const uint WM_KEYUP = 0x101;
        private const uint WM_SYSKEYDOWN = 0x104;
        private const uint WM_SYSKEYUP = 0x105;

        private nint _hHook = nint.Zero;
        private Win32HookProc _hookDelegate = null!;

        [LibraryImport("user32.dll", EntryPoint = "SetWindowsHookExW", StringMarshalling = StringMarshalling.Utf16)]
        private static partial nint SetWindowsHookEx(int idHook, Win32HookProc lpfn, nint hMod, uint dwThreadId);

        [LibraryImport("user32.dll", EntryPoint = "UnhookWindowsHookEx")]
        private static partial nint UnhookWindowsHookEx(nint hhk);

        [LibraryImport("user32.dll", EntryPoint = "CallNextHookEx")]
        private static partial nint CallNextHookEx(nint hhk, int nCode, nint wParam, nint lParam);

        [LibraryImport("kernel32.dll", EntryPoint = "GetCurrentThreadId")]
        private static partial uint GetCurrentThreadId();

        [LibraryImport("user32.dll", EntryPoint = "GetAsyncKeyState")]
        private static partial short GetAsyncKeyState(int vKey);

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

        private nint MessageHookProc(int nCode, nint wParam, nint lParam)
        {
            if (nCode >= 0 && (int)wParam == PM_REMOVE)
            {
                var msg = Marshal.PtrToStructure<MSG>(lParam);

                if (msg.message == WM_KEYDOWN || msg.message == WM_SYSKEYDOWN)
                {
                    if (!HandleKeyDown((int)msg.wParam))
                    {
                        MpvHandleKeyDown((int)msg.wParam);
                    }
                }
                else if (msg.message == WM_KEYUP || msg.message == WM_SYSKEYUP)
                {
                    if (!HandleKeyUp((int)msg.wParam))
                    {
                        MpvHandleKeyUp((int)msg.wParam);
                    }
                }
            }

            return CallNextHookEx(_hHook, nCode, wParam, lParam);
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

        private void MpvHandleKeyDown(int vkCode)
        {
            var mpvKey = VkCodeToMpvKey(vkCode);
            if (mpvKey != null)
            {
                _mediaPlayer?.Command(["keydown", mpvKey]);
            }
        }

        private void MpvHandleKeyUp(int vkCode)
        {
            var mpvKey = VkCodeToMpvKey(vkCode);
            if (mpvKey != null)
            {
                _mediaPlayer?.Command(["keyup", mpvKey]);
            }
        }

        private bool _isCtrlDown;
        private bool _isAltDown;
        private bool _isShiftDown;
        private bool _isMetaDown;

        public void OnKeyboardEvent(int vkCode, bool isKeyDown)
        {
            switch (vkCode)
            {
                case 0x10:
                case 0xA0:
                case 0xA1:
                    _isShiftDown = isKeyDown;
                    return;
                case 0x11:
                case 0xA2:
                case 0xA3:
                    _isCtrlDown = isKeyDown;
                    return;
                case 0x12:
                case 0xA4:
                case 0xA5:
                    _isAltDown = isKeyDown;
                    return;
                case 0x5B:
                case 0x5C:
                    _isMetaDown = isKeyDown;
                    return;
            }

            if (!isKeyDown)
            {
                MpvHandleKeyUp(vkCode);
            }
        }

        private string? VkCodeToMpvKey(int vk)
        {
            var keyName = VkCodeToMpvKeyName(vk);
            if (keyName == null)
            {
                return null;
            }

            var isLetter = vk >= 'A' && vk <= 'Z';

            if (isLetter && _isShiftDown)
            {
                keyName = keyName.ToUpperInvariant();
            }

            var prefix = "";
            if (_isCtrlDown)
            {
                prefix += "ctrl+";
            }

            if (_isAltDown)
            {
                prefix += "alt+";
            }

            if (_isShiftDown && !isLetter)
            {
                prefix += "shift+";
            }

            if (_isMetaDown)
            {
                prefix += "meta+";
            }

            return prefix + keyName;
        }

        private static string? VkCodeToMpvKeyName(int k)
        {
            if (k >= 'A' && k <= 'Z')
            {
                return ((char)(k + 0x20)).ToString();
            }

            if (k >= '0' && k <= '9')
            {
                return ((char)k).ToString();
            }

            if (k >= 0x70 && k <= 0x7B)
            {
                return "f" + (k - 0x70 + 1);
            }

            if (k >= 0x60 && k <= 0x69)
            {
                return "kp" + ((char)('0' + (k - 0x60))).ToString();
            }

            return k switch
            {
                0x08 => "bs", // Backspace
                0x09 => "tab", // Tab
                0x0D => "enter", // Enter
                0x1B => "esc", // Escape
                0x20 => "space", // Spacebar
                0x21 => "pgup", // Page Up
                0x22 => "pgdwn", // Page Down
                0x23 => "end", // End
                0x24 => "home", // Home
                0x25 => "left", // Left Arrow
                0x26 => "up", // Up Arrow
                0x27 => "right", // Right Arrow
                0x28 => "down", // Down Arrow
                0x2D => "ins", // Insert
                0x2E => "del", // Delete
                0x5D => "menu", // Context Menu

                0x6A => "*", // Numpad *
                0x6B => "+", // Numpad +
                0x6D => "-", // Numpad -
                0x6E => "del", // Numpad . (Decimal)
                0x6F => "/", // Numpad /

                0xBA => ";",
                0xBB => "+",
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

                0x90 => "numlock",
                0x91 => "scrolllock",
                0x14 => "capslock",
                0x13 => "pause",
                0x2A => "printscreen",

                0xAD => "mute",
                0xAE => "volume_down",
                0xAF => "volume_up",
                0xB0 => "next",
                0xB1 => "prev",
                0xB2 => "stop",
                0xB3 => "play",
                0xA6 => "mbtn_back",
                0xA7 => "mbtn_forward",

                _ => null
            };
        }
    }
}