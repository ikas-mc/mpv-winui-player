using mpv_winui.Modules.AppModel;

namespace mpv_winui.Modules.Settings
{
    public class AppSettings
    {
        private readonly IDataSetting _dataSetting;

        public AppSettings()
        {
            _dataSetting = PackageHelper.IsPackaged ? new AppDataSetting("app-settings") : new FileSetting("config.ini");
        }

        public bool EnableDebugLog
        {
            get => _dataSetting.GetValue(nameof(EnableDebugLog), false);
            set => _dataSetting.SetValue(nameof(EnableDebugLog), value);
        }

        public bool EnableMica
        {
            get => _dataSetting.GetValue(nameof(EnableMica), false);
            set => _dataSetting.SetValue(nameof(EnableMica), value);
        }

        public int PatchVersion
        {
            get => _dataSetting.GetValue(nameof(PatchVersion), 0);
            set => _dataSetting.SetValue(nameof(PatchVersion), value);
        }

        public ulong AppVersion
        {
            get => _dataSetting.GetValue(nameof(AppVersion), (ulong)0);
            set => _dataSetting.SetValue(nameof(AppVersion), value);
        }

        public string CurrentLanguage
        {
            get => _dataSetting.GetValue(nameof(CurrentLanguage), "");
            set => _dataSetting.SetValue(nameof(CurrentLanguage), value);
        }

        public int ThemeType
        {
            get => _dataSetting.GetValue(nameof(ThemeType), 0);
            set => _dataSetting.SetValue(nameof(ThemeType), value);
        }

        public bool EnableUISound
        {
            get => _dataSetting.GetValue(nameof(EnableUISound), false);
            set => _dataSetting.SetValue(nameof(EnableUISound), value);
        }

        public int LastVideoVolume
        {
            get => _dataSetting.GetValue(nameof(LastVideoVolume), 50);
            set => _dataSetting.SetValue(nameof(LastVideoVolume), value);
        }

        public int LastAudioVolume
        {
            get => _dataSetting.GetValue(nameof(LastAudioVolume), 50);
            set => _dataSetting.SetValue(nameof(LastAudioVolume), value);
        }
    }
}