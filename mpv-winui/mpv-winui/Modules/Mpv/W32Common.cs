/*
 * This file is part of mpv.
 *
 * mpv is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * mpv is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with mpv.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using static mpv.Keycodes;
using static mpv.WinUser;
using static Windows.Win32.PInvoke;

namespace mpv
{
    public partial class W32Common
    {
        private const uint MAPVK_VK_TO_VSC = 0;

        private static bool key_state(int vk)
        {
            return (GetKeyState(vk) & 0x8000) != 0;
        }

        public static int mod_state()
        {
            int res = 0;

            // AltGr is represented as LCONTROL+RMENU on Windows
            bool alt_gr = key_state(VK_RMENU) && key_state(VK_LCONTROL);

            if (key_state(VK_RCONTROL) || (key_state(VK_LCONTROL) && !alt_gr))
            {
                res |= MP_KEY_MODIFIER_CTRL;
            }

            if (key_state(VK_SHIFT))
            {
                res |= MP_KEY_MODIFIER_SHIFT;
            }

            if (key_state(VK_LMENU) || (key_state(VK_RMENU) && !alt_gr))
            {
                res |= MP_KEY_MODIFIER_ALT;
            }

            return res;
        }

        private static int decode_surrogate_pair(char lead, char trail)
        {
            return 0x10000 + (((lead & 0x3ff) << 10) | (trail & 0x3ff));
        }

        private static int high_surrogate = 0;

        public static int decode_utf16(char c)
        {
            // Decode UTF-16, keeping state in high_surrogate
            if (char.IsHighSurrogate(c))
            {
                high_surrogate = c;
                return 0;
            }
            if (char.IsLowSurrogate(c))
            {
                if (high_surrogate == 0)
                {
                    // MP_ERR: Invalid UTF-16 input
                    return 0;
                }
                int codepoint = decode_surrogate_pair((char)high_surrogate, c);
                high_surrogate = 0;
                return codepoint;
            }
            if (high_surrogate != 0)
            {
                high_surrogate = 0;
                // MP_ERR: Invalid UTF-16 input
                return 0;
            }

            return c;
        }

        private static void clear_keyboard_buffer()
        {
            uint vkey = VK_DECIMAL;
            Span<byte> keys = stackalloc byte[256];
            uint scancode = MapVirtualKey(vkey, MAPVK_VK_TO_VSC);
            Span<char> buf = stackalloc char[10];
            int ret = 0;

            // Use the method suggested by Michael Kaplan to clear any pending dead
            // keys from the current keyboard layout.
            do
            {
                ret = ToUnicode(vkey, scancode, keys, buf, 0);
            } while (ret < 0);
        }

        public static int to_unicode(uint vkey, uint scancode, Span<byte> keys)
        {
            // This wraps ToUnicode to be stateless and to return only one character

            // Make the buffer 10 code units long to be safe
            Span<char> buf = stackalloc char[10];

            // Dead keys aren't useful for key shortcuts, so clear the keyboard state
            clear_keyboard_buffer();

            int len = ToUnicode(vkey, scancode, keys, buf, 0);

            // Return the last complete UTF-16 code point. A negative return value
            // indicates a dead key, however there should still be a non-combining
            // version of the key in the buffer.
            if (len < 0)
            {
                len = -len;
            }

            if (len >= 2 && char.IsHighSurrogate(buf[len - 2]) && char.IsLowSurrogate(buf[len - 1]))
            {
                return decode_surrogate_pair(buf[len - 2], buf[len - 1]);
            }

            if (len >= 1)
            {
                return buf[len - 1];
            }

            return 0;
        }

        public static int decode_key(uint vkey, uint scancode)
        {
            Span<byte> keys = stackalloc byte[256];
            GetKeyboardState(keys);

            // If mp_input_use_alt_gr is false, detect and remove AltGr so normal
            // characters are generated. Note that AltGr is represented as
            // LCONTROL+RMENU on Windows.
            if ((keys[VK_RMENU] & 0x80) != 0 && (keys[VK_LCONTROL] & 0x80) != 0)
            {
                keys[VK_RMENU] = keys[VK_LCONTROL] = 0;
                keys[VK_MENU] = keys[VK_LMENU];
                keys[VK_CONTROL] = keys[VK_RCONTROL];
            }

            int c = to_unicode(vkey, scancode, keys);

            // Some shift states prevent ToUnicode from working or cause it to produce
            // control characters. If this is detected, remove modifiers until it
            // starts producing normal characters.
            if (c < 0x20 && (keys[VK_MENU] & 0x80) != 0)
            {
                keys[VK_LMENU] = keys[VK_RMENU] = keys[VK_MENU] = 0;
                c = to_unicode(vkey, scancode, keys);
            }
            if (c < 0x20 && (keys[VK_CONTROL] & 0x80) != 0)
            {
                keys[VK_LCONTROL] = keys[VK_RCONTROL] = keys[VK_CONTROL] = 0;
                c = to_unicode(vkey, scancode, keys);
            }
            if (c < 0x20)
            {
                return 0;
            }

            // Decode lone UTF-16 surrogates (VK_PACKET can generate these)
            if (c < 0x10000)
            {
                return decode_utf16((char)c);
            }

            return c;
        }

        public static bool handle_appcommand(int cmd)
        {
            int mpkey = W32Keyboard.mp_w32_appcmd_to_mpkey(cmd);
            if (mpkey == 0)
            {
                return false;
            }
            // mp_input_put_key(w32->input_ctx, mpkey | mod_state(w32));
            // In C# we would call the input context's put_key method
            return true;
        }

        public static int handle_char(uint wc, bool decode)
        {
            int c = decode ? decode_utf16((char)wc) : (int)wc;
            return c;
        }
    }
}
