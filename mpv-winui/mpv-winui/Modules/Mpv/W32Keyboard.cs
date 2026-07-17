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

using static mpv.WinUser;
using static mpv.Keycodes;

namespace mpv
{
    public static class W32Keyboard
    {
        private record Keymap(int From, int To);

        private static readonly Keymap[] vk_map_ext =
        [
            // cursor keys
            new(VK_LEFT, MP_KEY_LEFT), new(VK_UP, MP_KEY_UP),
            new(VK_RIGHT, MP_KEY_RIGHT), new(VK_DOWN, MP_KEY_DOWN),

            // navigation block
            new(VK_INSERT, MP_KEY_INSERT), new(VK_DELETE, MP_KEY_DELETE),
            new(VK_HOME, MP_KEY_HOME), new(VK_END, MP_KEY_END),
            new(VK_PRIOR, MP_KEY_PAGE_UP), new(VK_NEXT, MP_KEY_PAGE_DOWN),

            // numpad independent of numlock
            new(VK_RETURN, MP_KEY_KPENTER),
        ];

        private static readonly Keymap[] vk_map =
        [
            // special keys
            new(VK_ESCAPE, MP_KEY_ESC), new(VK_BACK, MP_KEY_BS),
            new(VK_TAB, MP_KEY_TAB), new(VK_RETURN, MP_KEY_ENTER),
            new(VK_PAUSE, MP_KEY_PAUSE), new(VK_SLEEP, MP_KEY_SLEEP),
            new(VK_SNAPSHOT, MP_KEY_PRINT), new(VK_APPS, MP_KEY_MENU),

            // F-keys
            new(VK_F1, MP_KEY_F+1), new(VK_F2, MP_KEY_F+2),
            new(VK_F3, MP_KEY_F+3), new(VK_F4, MP_KEY_F+4),
            new(VK_F5, MP_KEY_F+5), new(VK_F6, MP_KEY_F+6),
            new(VK_F7, MP_KEY_F+7), new(VK_F8, MP_KEY_F+8),
            new(VK_F9, MP_KEY_F+9), new(VK_F10, MP_KEY_F+10),
            new(VK_F11, MP_KEY_F+11), new(VK_F12, MP_KEY_F+12),
            new(VK_F13, MP_KEY_F+13), new(VK_F14, MP_KEY_F+14),
            new(VK_F15, MP_KEY_F+15), new(VK_F16, MP_KEY_F+16),
            new(VK_F17, MP_KEY_F+17), new(VK_F18, MP_KEY_F+18),
            new(VK_F19, MP_KEY_F+19), new(VK_F20, MP_KEY_F+20),
            new(VK_F21, MP_KEY_F+21), new(VK_F22, MP_KEY_F+22),
            new(VK_F23, MP_KEY_F+23), new(VK_F24, MP_KEY_F+24),

            // numpad independent of numlock
            new(VK_SUBTRACT, MP_KEY_KPSUBTRACT),
            new(VK_ADD, MP_KEY_KPADD),
            new(VK_MULTIPLY, MP_KEY_KPMULTIPLY),
            new(VK_DIVIDE, MP_KEY_KPDIVIDE),

            // numpad with numlock
            new(VK_NUMPAD0, MP_KEY_KP0), new(VK_NUMPAD1, MP_KEY_KP1),
            new(VK_NUMPAD2, MP_KEY_KP2), new(VK_NUMPAD3, MP_KEY_KP3),
            new(VK_NUMPAD4, MP_KEY_KP4), new(VK_NUMPAD5, MP_KEY_KP5),
            new(VK_NUMPAD6, MP_KEY_KP6), new(VK_NUMPAD7, MP_KEY_KP7),
            new(VK_NUMPAD8, MP_KEY_KP8), new(VK_NUMPAD9, MP_KEY_KP9),
            new(VK_DECIMAL, MP_KEY_KPDEC),

            // numpad without numlock
            new(VK_INSERT, MP_KEY_KPINS), new(VK_END, MP_KEY_KPEND),
            new(VK_DOWN, MP_KEY_KPDOWN), new(VK_NEXT, MP_KEY_KPPGDOWN),
            new(VK_LEFT, MP_KEY_KPLEFT), new(VK_CLEAR, MP_KEY_KPBEGIN),
            new(VK_RIGHT, MP_KEY_KPRIGHT), new(VK_HOME, MP_KEY_KPHOME),
            new(VK_UP, MP_KEY_KPUP), new(VK_PRIOR, MP_KEY_KPPGUP),
            new(VK_DELETE, MP_KEY_KPDEL),
        ];

        private static readonly Keymap[] appcmd_map =
        [
            new(APPCOMMAND_MEDIA_NEXTTRACK, MP_KEY_NEXT),
            new(APPCOMMAND_MEDIA_PREVIOUSTRACK, MP_KEY_PREV),
            new(APPCOMMAND_MEDIA_STOP, MP_KEY_STOP),
            new(APPCOMMAND_MEDIA_PLAY_PAUSE, MP_KEY_PLAYPAUSE),
            new(APPCOMMAND_MEDIA_PLAY, MP_KEY_PLAY),
            new(APPCOMMAND_MEDIA_PAUSE, MP_KEY_PAUSE),
            new(APPCOMMAND_MEDIA_RECORD, MP_KEY_RECORD),
            new(APPCOMMAND_MEDIA_FAST_FORWARD, MP_KEY_FORWARD),
            new(APPCOMMAND_MEDIA_REWIND, MP_KEY_REWIND),
            new(APPCOMMAND_MEDIA_CHANNEL_UP, MP_KEY_CHANNEL_UP),
            new(APPCOMMAND_MEDIA_CHANNEL_DOWN, MP_KEY_CHANNEL_DOWN),
            new(APPCOMMAND_VOLUME_MUTE, MP_KEY_MUTE),
            new(APPCOMMAND_VOLUME_DOWN, MP_KEY_VOLUME_DOWN),
            new(APPCOMMAND_VOLUME_UP, MP_KEY_VOLUME_UP),
            new(APPCOMMAND_BROWSER_HOME, MP_KEY_HOMEPAGE),
            new(APPCOMMAND_LAUNCH_MAIL, MP_KEY_MAIL),
            new(APPCOMMAND_BROWSER_FAVORITES, MP_KEY_FAVORITES),
            new(APPCOMMAND_BROWSER_SEARCH, MP_KEY_SEARCH),
            new(APPCOMMAND_BROWSER_BACKWARD, MP_KEY_GO_BACK),
            new(APPCOMMAND_BROWSER_FORWARD, MP_KEY_GO_FORWARD),
        ];

        private static int lookup_keymap(Keymap[] map, int key)
        {
            foreach (var entry in map)
            {
                if (entry.From == key)
                {
                    return entry.To;
                }
            }
            return 0;
        }

        public static int mp_w32_vkey_to_mpkey(int vkey, bool extended)
        {
            int mpkey = lookup_keymap(extended ? vk_map_ext : vk_map, vkey);
            if (extended && mpkey == 0)
            {
                mpkey = lookup_keymap(vk_map, vkey);
            }

            return mpkey;
        }

        public static int mp_w32_appcmd_to_mpkey(int appcmd)
        {
            return lookup_keymap(appcmd_map, appcmd);
        }
    }
}
