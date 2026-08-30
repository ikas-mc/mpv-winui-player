using mpv_winui.Modules.AppModel;

namespace mpv_winui.Modules.Settings
{
    public class AppSettings
    {
        private readonly IDataSetting _dataSetting;

        public AppSettings()
        {
            _dataSetting = PackageHelper.IsPackaged ? new AppDataSetting("app-settings") : new UnpackageAppDataSetting("app");
        }

        public const string ThemeType_Auto = "Auto";
        public const string ThemeType_Light = "Light";
        public const string ThemeType_Dark = "Dark";
        public string ThemeType
        {
            get => _dataSetting.GetValue(nameof(ThemeType), ThemeType_Auto);
            set => _dataSetting.SetValue(nameof(ThemeType), value);
        }

        public const string BackdropType_Acrylic = "Acrylic";
        public const string BackdropType_Mica = "Mica";
        public string BackdropType
        {
            get => _dataSetting.GetValue(nameof(BackdropType), BackdropType_Acrylic);
            set => _dataSetting.SetValue(nameof(BackdropType), value);
        }

        public bool EnableDebugLog
        {
            get => _dataSetting.GetValue(nameof(EnableDebugLog), false);
            set => _dataSetting.SetValue(nameof(EnableDebugLog), value);
        }

        public string CurrentLanguage
        {
            get => _dataSetting.GetValue(nameof(CurrentLanguage), string.Empty);
            set => _dataSetting.SetValue(nameof(CurrentLanguage), value);
        }

        public ulong AppVersion
        {
            get => _dataSetting.GetValue(nameof(AppVersion), (ulong)0);
            set => _dataSetting.SetValue(nameof(AppVersion), value);
        }

        public int PatchVersion
        {
            get => _dataSetting.GetValue(nameof(PatchVersion), 0);
            set => _dataSetting.SetValue(nameof(PatchVersion), value);
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

        public string WindowPositionAndSize
        {
            get => _dataSetting.GetValue(nameof(WindowPositionAndSize), string.Empty);
            set => _dataSetting.SetValue(nameof(WindowPositionAndSize), value);
        }

        public bool EnableVideoPreview
        {
            get => _dataSetting.GetValue(nameof(EnableVideoPreview), false);
            set => _dataSetting.SetValue(nameof(EnableVideoPreview), value);
        }

        public bool EnableVideoBuiltInPreview
        {
            get => _dataSetting.GetValue(nameof(EnableVideoBuiltInPreview), false);
            set => _dataSetting.SetValue(nameof(EnableVideoBuiltInPreview), value);
        }

        public bool KeepVideoBuiltInPreviewAlive
        {
            get => _dataSetting.GetValue(nameof(KeepVideoBuiltInPreviewAlive), false);
            set => _dataSetting.SetValue(nameof(KeepVideoBuiltInPreviewAlive), value);
        }

        public int BuiltInPreviewAliveTimeout
        {
            get => _dataSetting.GetValue(nameof(BuiltInPreviewAliveTimeout), 20);
            set => _dataSetting.SetValue(nameof(BuiltInPreviewAliveTimeout), value);
        }

        public bool EnableMouseInput
        {
            get => _dataSetting.GetValue(nameof(EnableMouseInput), false);
            set => _dataSetting.SetValue(nameof(EnableMouseInput), value);
        }

        public bool EnableMouseInputDiscNavOnly
        {
            get => _dataSetting.GetValue(nameof(EnableMouseInputDiscNavOnly), false);
            set => _dataSetting.SetValue(nameof(EnableMouseInputDiscNavOnly), value);
        }
    }
}