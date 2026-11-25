namespace mpv_winui.Modules.Language
{
    public partial class AppLang
    {
        public string SettingsGroupStyle { get; set; } = "Style";
        public string SettingsGroupPreview { get; set; } = "Preview";
        public string SettingsGroupInput { get; set; } = "Input";
        public string SettingsGroupPlayerControl { get; set; } = "Player Control";
        public string SettingsGroupPower { get; set; } = "Power";
        public string SettingsGroupApp { get; set; } = "App";
        public string SettingsGroupData { get; set; } = "Data";

        public string SettingsTheme { get; set; } = "Theme";
        public string SettingsThemeNote { get; set; } = "App theme";
        public string SettingsBackdrop { get; set; } = "Backdrop";
        public string SettingsBackdropNote { get; set; } = "Window background. Restart required.";
        public string SettingsPlayerStyle { get; set; } = "Player Control Style";
        public string SettingsPlayerStyleNote { get; set; } = "Player control style.";

        public string SettingsThumbnailPreview { get; set; } = "Thumbnails Preview";
        public string SettingsThumbnailPreviewNote { get; set; } = "Using a plugin that supports the mpv osc-preview-api.";
        public string SettingsBuiltInPreview { get; set; } = "Built-in Thumbnails Preview";
        public string SettingsBuiltInPreviewNote { get; set; } = "Built-in preview. Restart required.";
        public string SettingsBuiltInPreviewKeepAlive { get; set; } = "Built-in Preview Keep Alive";
        public string SettingsBuiltInPreviewKeepAliveNote { get; set; } = "Keep the preview mpv instance alive. Restart required.";
        public string SettingsBuiltInPreviewTimeout { get; set; } = "Built-in Preview Alive Timeout (s)";
        public string SettingsBuiltInPreviewTimeoutNote { get; set; } = "Seconds the preview mpv instance stays alive.";

        public string SettingsMouseInput { get; set; } = "Mouse Input";
        public string SettingsMouseInputNote { get; set; } = "Enable mouse input to mpv. Restart required.";
        public string SettingsMouseInputDiscNavOnly { get; set; } = "Mouse Input Disc Nav Only";
        public string SettingsMouseInputDiscNavOnlyNote { get; set; } = "When mouse input is enabled, send events to mpv only when the disc menu is active. Restart required.";

        public string SettingsPlayerControlToggle { get; set; } = "Player Control Toggle Button";
        public string SettingsPlayerControlToggleNote { get; set; } = "Mouse buttons to toggle player control, Ignored while auto hide is enabled.\nL: left, M: middle, D: double-click.";
        public string SettingsPlayerControlAutoHide { get; set; } = "Auto Hide Player Control";
        public string SettingsPlayerControlAutoHideNote { get; set; } = "Auto hide the player control, Disables the manual toggle. Restart required.";
        public string SettingsPlayerControlAutoHideDelay { get; set; } = "Auto Hide Delay (ms)";
        public string SettingsPlayerControlAutoHideDelayNote { get; set; } = "Idle time before the player control hide. Restart required.";

        public string SettingsKeepDisplayAwake { get; set; } = "Keep Display Awake";
        public string SettingsKeepDisplayAwakeNote { get; set; } = "Prevent the display from dimming or sleeping while playing. Restart required.";

        public string SettingsSingleAppInstance { get; set; } = "Single App Instance";
        public string SettingsSingleAppInstanceNote { get; set; } = "Enable single app instance. Restart required.";
        public string SettingsDebugLog { get; set; } = "Debug Log";
        public string SettingsDebugLogNote { get; set; } = "Enable app and mpv debug log. Restart required.";
    }
}
