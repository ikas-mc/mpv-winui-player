namespace mpv
{
    public static class WinUser
    {
        public const int KF_EXTENDED = 0x0100;
        public const int KF_DLGMODE = 0x0800;
        public const int KF_MENUMODE = 0x1000;
        public const int KF_ALTDOWN = 0x2000;
        public const int KF_REPEAT = 0x4000;
        public const int KF_UP = 0x8000;

        /* Virtual Keys, Standard Set */
        public const int VK_LBUTTON = 0x01;
        public const int VK_RBUTTON = 0x02;
        public const int VK_CANCEL = 0x03;
        public const int VK_MBUTTON = 0x04;

        public const int VK_XBUTTON1 = 0x05;
        public const int VK_XBUTTON2 = 0x06;

        public const int VK_BACK = 0x08;
        public const int VK_TAB = 0x09;

        public const int VK_CLEAR = 0x0C;
        public const int VK_RETURN = 0x0D;

        public const int VK_SHIFT = 0x10;
        public const int VK_CONTROL = 0x11;
        public const int VK_MENU = 0x12;
        public const int VK_PAUSE = 0x13;
        public const int VK_CAPITAL = 0x14;

        public const int VK_KANA = 0x15;
        public const int VK_HANGEUL = 0x15;
        public const int VK_HANGUL = 0x15;
        public const int VK_IME_ON = 0x16;
        public const int VK_JUNJA = 0x17;
        public const int VK_FINAL = 0x18;
        public const int VK_HANJA = 0x19;
        public const int VK_KANJI = 0x19;
        public const int VK_IME_OFF = 0x1A;

        public const int VK_ESCAPE = 0x1B;

        public const int VK_CONVERT = 0x1C;
        public const int VK_NONCONVERT = 0x1D;
        public const int VK_ACCEPT = 0x1E;
        public const int VK_MODECHANGE = 0x1F;

        public const int VK_SPACE = 0x20;
        public const int VK_PRIOR = 0x21;
        public const int VK_NEXT = 0x22;
        public const int VK_END = 0x23;
        public const int VK_HOME = 0x24;
        public const int VK_LEFT = 0x25;
        public const int VK_UP = 0x26;
        public const int VK_RIGHT = 0x27;
        public const int VK_DOWN = 0x28;
        public const int VK_SELECT = 0x29;
        public const int VK_PRINT = 0x2A;
        public const int VK_EXECUTE = 0x2B;
        public const int VK_SNAPSHOT = 0x2C;
        public const int VK_INSERT = 0x2D;
        public const int VK_DELETE = 0x2E;
        public const int VK_HELP = 0x2F;

        /*
         * VK_0 - VK_9 are the same as ASCII '0' - '9' (0x30 - 0x39)
         * 0x3A - 0x40 : unassigned
         * VK_A - VK_Z are the same as ASCII 'A' - 'Z' (0x41 - 0x5A)
         */

        public const int VK_LWIN = 0x5B;
        public const int VK_RWIN = 0x5C;
        public const int VK_APPS = 0x5D;

        public const int VK_SLEEP = 0x5F;

        public const int VK_NUMPAD0 = 0x60;
        public const int VK_NUMPAD1 = 0x61;
        public const int VK_NUMPAD2 = 0x62;
        public const int VK_NUMPAD3 = 0x63;
        public const int VK_NUMPAD4 = 0x64;
        public const int VK_NUMPAD5 = 0x65;
        public const int VK_NUMPAD6 = 0x66;
        public const int VK_NUMPAD7 = 0x67;
        public const int VK_NUMPAD8 = 0x68;
        public const int VK_NUMPAD9 = 0x69;
        public const int VK_MULTIPLY = 0x6A;
        public const int VK_ADD = 0x6B;
        public const int VK_SEPARATOR = 0x6C;
        public const int VK_SUBTRACT = 0x6D;
        public const int VK_DECIMAL = 0x6E;
        public const int VK_DIVIDE = 0x6F;
        public const int VK_F1 = 0x70;
        public const int VK_F2 = 0x71;
        public const int VK_F3 = 0x72;
        public const int VK_F4 = 0x73;
        public const int VK_F5 = 0x74;
        public const int VK_F6 = 0x75;
        public const int VK_F7 = 0x76;
        public const int VK_F8 = 0x77;
        public const int VK_F9 = 0x78;
        public const int VK_F10 = 0x79;
        public const int VK_F11 = 0x7A;
        public const int VK_F12 = 0x7B;
        public const int VK_F13 = 0x7C;
        public const int VK_F14 = 0x7D;
        public const int VK_F15 = 0x7E;
        public const int VK_F16 = 0x7F;
        public const int VK_F17 = 0x80;
        public const int VK_F18 = 0x81;
        public const int VK_F19 = 0x82;
        public const int VK_F20 = 0x83;
        public const int VK_F21 = 0x84;
        public const int VK_F22 = 0x85;
        public const int VK_F23 = 0x86;
        public const int VK_F24 = 0x87;

        /* UI navigation */
        public const int VK_NAVIGATION_VIEW = 0x88;
        public const int VK_NAVIGATION_MENU = 0x89;
        public const int VK_NAVIGATION_UP = 0x8A;
        public const int VK_NAVIGATION_DOWN = 0x8B;
        public const int VK_NAVIGATION_LEFT = 0x8C;
        public const int VK_NAVIGATION_RIGHT = 0x8D;
        public const int VK_NAVIGATION_ACCEPT = 0x8E;
        public const int VK_NAVIGATION_CANCEL = 0x8F;

        public const int VK_NUMLOCK = 0x90;
        public const int VK_SCROLL = 0x91;

        public const int VK_OEM_NEC_EQUAL = 0x92;
        public const int VK_OEM_FJ_JISHO = 0x92;
        public const int VK_OEM_FJ_MASSHOU = 0x93;
        public const int VK_OEM_FJ_TOUROKU = 0x94;
        public const int VK_OEM_FJ_LOYA = 0x95;
        public const int VK_OEM_FJ_ROYA = 0x96;

        /* Left/Right modifier keys */
        public const int VK_LSHIFT = 0xA0;
        public const int VK_RSHIFT = 0xA1;
        public const int VK_LCONTROL = 0xA2;
        public const int VK_RCONTROL = 0xA3;
        public const int VK_LMENU = 0xA4;
        public const int VK_RMENU = 0xA5;

        /* Browser keys */
        public const int VK_BROWSER_BACK = 0xA6;
        public const int VK_BROWSER_FORWARD = 0xA7;
        public const int VK_BROWSER_REFRESH = 0xA8;
        public const int VK_BROWSER_STOP = 0xA9;
        public const int VK_BROWSER_SEARCH = 0xAA;
        public const int VK_BROWSER_FAVORITES = 0xAB;
        public const int VK_BROWSER_HOME = 0xAC;

        /* Volume keys */
        public const int VK_VOLUME_MUTE = 0xAD;
        public const int VK_VOLUME_DOWN = 0xAE;
        public const int VK_VOLUME_UP = 0xAF;

        /* Media keys */
        public const int VK_MEDIA_NEXT_TRACK = 0xB0;
        public const int VK_MEDIA_PREV_TRACK = 0xB1;
        public const int VK_MEDIA_STOP = 0xB2;
        public const int VK_MEDIA_PLAY_PAUSE = 0xB3;
        public const int VK_LAUNCH_MAIL = 0xB4;
        public const int VK_LAUNCH_MEDIA_SELECT = 0xB5;
        public const int VK_LAUNCH_APP1 = 0xB6;
        public const int VK_LAUNCH_APP2 = 0xB7;

        /* OEM keys */
        public const int VK_OEM_1 = 0xBA;
        public const int VK_OEM_PLUS = 0xBB;
        public const int VK_OEM_COMMA = 0xBC;
        public const int VK_OEM_MINUS = 0xBD;
        public const int VK_OEM_PERIOD = 0xBE;
        public const int VK_OEM_2 = 0xBF;
        public const int VK_OEM_3 = 0xC0;

        /* Gamepad input */
        public const int VK_GAMEPAD_A = 0xC3;
        public const int VK_GAMEPAD_B = 0xC4;
        public const int VK_GAMEPAD_X = 0xC5;
        public const int VK_GAMEPAD_Y = 0xC6;
        public const int VK_GAMEPAD_RIGHT_SHOULDER = 0xC7;
        public const int VK_GAMEPAD_LEFT_SHOULDER = 0xC8;
        public const int VK_GAMEPAD_LEFT_TRIGGER = 0xC9;
        public const int VK_GAMEPAD_RIGHT_TRIGGER = 0xCA;
        public const int VK_GAMEPAD_DPAD_UP = 0xCB;
        public const int VK_GAMEPAD_DPAD_DOWN = 0xCC;
        public const int VK_GAMEPAD_DPAD_LEFT = 0xCD;
        public const int VK_GAMEPAD_DPAD_RIGHT = 0xCE;
        public const int VK_GAMEPAD_MENU = 0xCF;
        public const int VK_GAMEPAD_VIEW = 0xD0;
        public const int VK_GAMEPAD_LEFT_THUMBSTICK_BUTTON = 0xD1;
        public const int VK_GAMEPAD_RIGHT_THUMBSTICK_BUTTON = 0xD2;
        public const int VK_GAMEPAD_LEFT_THUMBSTICK_UP = 0xD3;
        public const int VK_GAMEPAD_LEFT_THUMBSTICK_DOWN = 0xD4;
        public const int VK_GAMEPAD_LEFT_THUMBSTICK_RIGHT = 0xD5;
        public const int VK_GAMEPAD_LEFT_THUMBSTICK_LEFT = 0xD6;
        public const int VK_GAMEPAD_RIGHT_THUMBSTICK_UP = 0xD7;
        public const int VK_GAMEPAD_RIGHT_THUMBSTICK_DOWN = 0xD8;
        public const int VK_GAMEPAD_RIGHT_THUMBSTICK_RIGHT = 0xD9;
        public const int VK_GAMEPAD_RIGHT_THUMBSTICK_LEFT = 0xDA;

        public const int VK_OEM_4 = 0xDB;
        public const int VK_OEM_5 = 0xDC;
        public const int VK_OEM_6 = 0xDD;
        public const int VK_OEM_7 = 0xDE;
        public const int VK_OEM_8 = 0xDF;

        public const int VK_OEM_AX = 0xE1;
        public const int VK_OEM_102 = 0xE2;
        public const int VK_ICO_HELP = 0xE3;
        public const int VK_ICO_00 = 0xE4;

        public const int VK_PROCESSKEY = 0xE5;
        public const int VK_ICO_CLEAR = 0xE6;
        public const int VK_PACKET = 0xE7;

        public const int VK_OEM_RESET = 0xE9;
        public const int VK_OEM_JUMP = 0xEA;
        public const int VK_OEM_PA1 = 0xEB;
        public const int VK_OEM_PA2 = 0xEC;
        public const int VK_OEM_PA3 = 0xED;
        public const int VK_OEM_WSCTRL = 0xEE;
        public const int VK_OEM_CUSEL = 0xEF;
        public const int VK_OEM_ATTN = 0xF0;
        public const int VK_OEM_FINISH = 0xF1;
        public const int VK_OEM_COPY = 0xF2;
        public const int VK_OEM_AUTO = 0xF3;
        public const int VK_OEM_ENLW = 0xF4;
        public const int VK_OEM_BACKTAB = 0xF5;

        public const int VK_ATTN = 0xF6;
        public const int VK_CRSEL = 0xF7;
        public const int VK_EXSEL = 0xF8;
        public const int VK_EREOF = 0xF9;
        public const int VK_PLAY = 0xFA;
        public const int VK_ZOOM = 0xFB;
        public const int VK_NONAME = 0xFC;
        public const int VK_PA1 = 0xFD;
        public const int VK_OEM_CLEAR = 0xFE;

        /* WM_APPCOMMAND values */
        public const int APPCOMMAND_MEDIA_NEXTTRACK = 11;
        public const int APPCOMMAND_MEDIA_PREVIOUSTRACK = 12;
        public const int APPCOMMAND_MEDIA_STOP = 13;
        public const int APPCOMMAND_MEDIA_PLAY_PAUSE = 14;
        public const int APPCOMMAND_MEDIA_PLAY = 46;
        public const int APPCOMMAND_MEDIA_PAUSE = 47;
        public const int APPCOMMAND_MEDIA_RECORD = 48;
        public const int APPCOMMAND_MEDIA_FAST_FORWARD = 49;
        public const int APPCOMMAND_MEDIA_REWIND = 50;
        public const int APPCOMMAND_MEDIA_CHANNEL_UP = 51;
        public const int APPCOMMAND_MEDIA_CHANNEL_DOWN = 52;
        public const int APPCOMMAND_VOLUME_MUTE = 8;
        public const int APPCOMMAND_VOLUME_DOWN = 9;
        public const int APPCOMMAND_VOLUME_UP = 10;
        public const int APPCOMMAND_BROWSER_HOME = 36;
        public const int APPCOMMAND_LAUNCH_MAIL = 43;
        public const int APPCOMMAND_BROWSER_FAVORITES = 44;
        public const int APPCOMMAND_BROWSER_SEARCH = 33;
        public const int APPCOMMAND_BROWSER_BACKWARD = 34;
        public const int APPCOMMAND_BROWSER_FORWARD = 35;
    }
}
