namespace mpv_winui.Modules.Settings
{
    public class AppSettings
    {
        private readonly AppDataSetting _mainSettingDao = new("app-settings");

        public bool EnableLog
        {
            get => _mainSettingDao.GetValue(nameof(EnableLog), false);
            set => _mainSettingDao.SetValue(nameof(EnableLog), value);
        }

        public bool EnableMica
        {
            get => _mainSettingDao.GetValue(nameof(EnableMica), false);
            set => _mainSettingDao.SetValue(nameof(EnableMica), value);
        }

        public int PatchVersion
        {
            get => _mainSettingDao.GetValue(nameof(PatchVersion), 0);
            set => _mainSettingDao.SetValue(nameof(PatchVersion), value);
        }

        public ulong AppVersion
        {
            get => _mainSettingDao.GetValue(nameof(AppVersion), (ulong)0);
            set => _mainSettingDao.SetValue(nameof(AppVersion), value);
        }

        public string CurrentLanguage
        {
            get => _mainSettingDao.GetValue(nameof(CurrentLanguage), "");
            set => _mainSettingDao.SetValue(nameof(CurrentLanguage), value);
        }

        public int ThemeType
        {
            get => _mainSettingDao.GetValue(nameof(ThemeType), 0);
            set => _mainSettingDao.SetValue(nameof(ThemeType), value);
        }

        public bool EnableUISound
        {
            get => _mainSettingDao.GetValue(nameof(EnableUISound), false);
            set => _mainSettingDao.SetValue(nameof(EnableUISound), value);
        }

        public int LastVideoVolume
        {
            get => _mainSettingDao.GetValue(nameof(LastVideoVolume), 50);
            set => _mainSettingDao.SetValue(nameof(LastVideoVolume), value);
        }

        public int LastAudioVolume
        {
            get => _mainSettingDao.GetValue(nameof(LastAudioVolume), 50);
            set => _mainSettingDao.SetValue(nameof(LastAudioVolume), value);
        }
    }
}