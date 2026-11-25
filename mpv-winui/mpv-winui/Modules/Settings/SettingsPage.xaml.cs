using Microsoft.UI.Xaml.Controls;
using mpv_winui.Modules.AppModel;
using mpv_winui.Modules.Common.Utils;
using mpv_winui.Modules.Settings.Controls;
using System.Collections.Generic;

namespace mpv_winui.Modules.Settings;

public sealed partial class SettingsPage : Page
{
    public List<Option> Settings { get; } = [];

    private readonly bool _isUnpackaged;

    public SettingsPage()
    {
        _isUnpackaged = !PackageHelper.IsPackaged;
        InitializeComponent();
        Settings.AddRange(BuildSettings());
    }

    private List<Option> BuildSettings()
    {
        return
        [
            new Option
            {
                Key = nameof(AppContext.AppSetting.ThemeType),
                Label = AppContext.AppLang.SettingsTheme,
                Description = AppContext.AppLang.SettingsThemeNote,
                Icon = "\uE790",
                GroupKey="Style",
                GroupLabel = AppContext.AppLang.SettingsGroupStyle,
                Type = OptionType.StringList,
                Options = [AppSettings.ThemeType_Auto, AppSettings.ThemeType_Light, AppSettings.ThemeType_Dark],
                Getter = () => AppContext.AppSetting.ThemeType,
                Setter = v =>{
                    if ((string)v != AppContext.AppSetting.ThemeType)
                    {
                        AppContext.AppSetting.ThemeType = (string)v;
                        UpdateTheme();
                    }
                }
            },

            new Option
            {
                Key =  nameof(AppContext.AppSetting.BackdropType),
                Label = AppContext.AppLang.SettingsBackdrop,
                Description = AppContext.AppLang.SettingsBackdropNote,
                Icon="\uE91B",
                GroupKey="Style",
                GroupLabel = AppContext.AppLang.SettingsGroupStyle,
                Type = OptionType.StringList,
                Options = [AppSettings.BackdropType_Acrylic, AppSettings.BackdropType_Mica],
                Getter = () => AppContext.AppSetting.BackdropType,
                Setter = v =>{
                    if ((string)v != AppContext.AppSetting.BackdropType)
                    {
                        AppContext.AppSetting.BackdropType = (string)v;
                        UpdateBackdrop();
                    }
                }
            },

            new Option
            {
                Key = nameof(AppContext.AppSetting.PlayerStyle),
                Label = AppContext.AppLang.SettingsPlayerStyle,
                Description = AppContext.AppLang.SettingsPlayerStyleNote,
                Icon = "\uE768",
                GroupKey = "Style",
                GroupLabel = AppContext.AppLang.SettingsGroupStyle,
                Type = OptionType.StringList,
                Options = [AppSettings.PlayerStyle_Default, AppSettings.PlayerStyle_Center, AppSettings.PlayerStyle_Compact],
                Getter = () => AppContext.AppSetting.PlayerStyle,
                Setter = v =>{
                    if ((string)v != AppContext.AppSetting.PlayerStyle)
                    {
                        AppContext.AppSetting.PlayerStyle = (string)v;
                        UpdatePlayControlStyle();
                    }
                }
            },

            new Option
            {
                Key =  nameof(AppContext.AppSetting.EnableVideoPreview),
                Label = AppContext.AppLang.SettingsThumbnailPreview,
                Description = AppContext.AppLang.SettingsThumbnailPreviewNote,
                Icon = "\uE8B2",
                GroupKey="Preview",
                GroupLabel = AppContext.AppLang.SettingsGroupPreview,
                Type = OptionType.Boolean,
                Getter = () => AppContext.AppSetting.EnableVideoPreview,
                Setter = v => AppContext.AppSetting.EnableVideoPreview = (bool)v!
            },

            new Option
            {
                Key =  nameof(AppContext.AppSetting.EnableVideoBuiltInPreview),
                Label = AppContext.AppLang.SettingsBuiltInPreview,
                Description = AppContext.AppLang.SettingsBuiltInPreviewNote,
                Icon = "\uE8B2",
                GroupKey="Preview",
                GroupLabel = AppContext.AppLang.SettingsGroupPreview,
                Type = OptionType.Boolean,
                Getter = () => AppContext.AppSetting.EnableVideoBuiltInPreview,
                Setter = v => AppContext.AppSetting.EnableVideoBuiltInPreview = (bool)v!
            },

            new Option
            {
                Key =  nameof(AppContext.AppSetting.KeepVideoBuiltInPreviewAlive),
                Label = AppContext.AppLang.SettingsBuiltInPreviewKeepAlive,
                Description = AppContext.AppLang.SettingsBuiltInPreviewKeepAliveNote,
                Icon = "\uE8B2",
                GroupKey="Preview",
                GroupLabel = AppContext.AppLang.SettingsGroupPreview,
                Type = OptionType.Boolean,
                Getter = () => AppContext.AppSetting.KeepVideoBuiltInPreviewAlive,
                Setter = v => AppContext.AppSetting.KeepVideoBuiltInPreviewAlive = (bool)v!
            },

            new Option
            {
                Key =  nameof(AppContext.AppSetting.BuiltInPreviewAliveTimeout),
                Label = AppContext.AppLang.SettingsBuiltInPreviewTimeout,
                Description = AppContext.AppLang.SettingsBuiltInPreviewTimeoutNote,
                Icon = "\uE8B2",
                GroupKey="Preview",
                GroupLabel = AppContext.AppLang.SettingsGroupPreview,
                Type = OptionType.Integer,
                Min = 0,
                Max = int.MaxValue,
                Step = 10,
                Getter = () => AppContext.AppSetting.BuiltInPreviewAliveTimeout,
                Setter = v => AppContext.AppSetting.BuiltInPreviewAliveTimeout = (int)v!
            },

            new Option
            {
                Key =  nameof(AppContext.AppSetting.EnableMouseInput),
                Label = AppContext.AppLang.SettingsMouseInput,
                Description = AppContext.AppLang.SettingsMouseInputNote,
                Icon = "\uE961",
                GroupKey="Input",
                GroupLabel = AppContext.AppLang.SettingsGroupInput,
                Type = OptionType.Boolean,
                Getter = () => AppContext.AppSetting.EnableMouseInput,
                Setter = v => AppContext.AppSetting.EnableMouseInput = (bool)v!
            },

            new Option
            {
                Key =  nameof(AppContext.AppSetting.EnableMouseInputDiscNavOnly),
                Label = AppContext.AppLang.SettingsMouseInputDiscNavOnly,
                Description = AppContext.AppLang.SettingsMouseInputDiscNavOnlyNote,
                Icon = "\uE961",
                GroupKey="Input",
                GroupLabel = AppContext.AppLang.SettingsGroupInput,
                Type = OptionType.Boolean,
                Getter = () => AppContext.AppSetting.EnableMouseInputDiscNavOnly,
                Setter = v => AppContext.AppSetting.EnableMouseInputDiscNavOnly = (bool)v!
            },

            new Option
            {
                Key = nameof(AppContext.AppSetting.PlayerControlToggleButton),
                Label = AppContext.AppLang.SettingsPlayerControlToggle,
                Description = AppContext.AppLang.SettingsPlayerControlToggleNote,
                Icon = "\uE961",
                GroupKey = "PlayerControl",
                GroupLabel = AppContext.AppLang.SettingsGroupPlayerControl,
                Type = OptionType.StringList,
                Options = [AppSettings.PlayerControlToggleButton_LeftMiddle, AppSettings.PlayerControlToggleButton_Middle, AppSettings.PlayerControlToggleButton_LeftDouble, AppSettings.PlayerControlToggleButton_LeftDoubleMiddle],
                Getter = () => AppContext.AppSetting.PlayerControlToggleButton,
                Setter = v => AppContext.AppSetting.PlayerControlToggleButton = (string)v
            },

            new Option
            {
                Key = nameof(AppContext.AppSetting.EnablePlayerControlAutoHide),
                Label = AppContext.AppLang.SettingsPlayerControlAutoHide,
                Description = AppContext.AppLang.SettingsPlayerControlAutoHideNote,
                Icon = "\uE917",
                GroupKey = "PlayerControl",
                GroupLabel = AppContext.AppLang.SettingsGroupPlayerControl,
                Type = OptionType.Boolean,
                Getter = () => AppContext.AppSetting.EnablePlayerControlAutoHide,
                Setter = v => AppContext.AppSetting.EnablePlayerControlAutoHide = (bool)v!
            },

            new Option
            {
                Key = nameof(AppContext.AppSetting.PlayerControlAutoHideDelay),
                Label = AppContext.AppLang.SettingsPlayerControlAutoHideDelay,
                Description = AppContext.AppLang.SettingsPlayerControlAutoHideDelayNote,
                Icon = "\uE917",
                GroupKey = "PlayerControl",
                GroupLabel = AppContext.AppLang.SettingsGroupPlayerControl,
                Type = OptionType.Integer,
                Min = 200,
                Max = 60000,
                Step = 100,
                Getter = () => AppContext.AppSetting.PlayerControlAutoHideDelay,
                Setter = v => AppContext.AppSetting.PlayerControlAutoHideDelay = (int)v!
            },

            new Option
            {
                Key =  nameof(AppContext.AppSetting.KeepDisplayAwake),
                Label = AppContext.AppLang.SettingsKeepDisplayAwake,
                Description = AppContext.AppLang.SettingsKeepDisplayAwakeNote,
                Icon = "\uE945",
                GroupKey="Power",
                GroupLabel = AppContext.AppLang.SettingsGroupPower,
                Type = OptionType.Boolean,
                Getter = () => AppContext.AppSetting.KeepDisplayAwake,
                Setter = v => AppContext.AppSetting.KeepDisplayAwake = (bool)v!
            },

            new Option
            {
                Key =  nameof(AppContext.AppSetting.SingleAppInstance),
                Label = AppContext.AppLang.SettingsSingleAppInstance,
                Description = AppContext.AppLang.SettingsSingleAppInstanceNote,
                Icon = "\uECAA",
                GroupKey="App",
                GroupLabel = AppContext.AppLang.SettingsGroupApp,
                Type = OptionType.Boolean,
                Getter = () => AppContext.AppSetting.SingleAppInstance,
                Setter = v => AppContext.AppSetting.SingleAppInstance = (bool)v!
            },

            new Option
            {
                Key =  nameof(AppContext.AppSetting.EnableDebugLog),
                Label = AppContext.AppLang.SettingsDebugLog,
                Description = AppContext.AppLang.SettingsDebugLogNote,
                Icon = "\uE946",
                GroupKey="Data",
                GroupLabel = AppContext.AppLang.SettingsGroupData,
                Type = OptionType.Boolean,
                Getter = () => AppContext.AppSetting.EnableDebugLog,
                Setter = v => AppContext.AppSetting.EnableDebugLog = (bool)v!
            },
        ];
    }

    private void UpdateTheme()
    {
        DispatcherQueue.RunAsync(() =>
        {
            if (App.Window is MainWindow mainWindow)
            {
                mainWindow.UpdateTheme();
            }
        });
    }

    private void UpdateBackdrop()
    {
        DispatcherQueue.RunAsync(() =>
        {
            if (App.Window is MainWindow mainWindow)
            {
                mainWindow.UpdateBackdrop();
            }
        });
    }

    private void UpdatePlayControlStyle()
    {
        DispatcherQueue.RunAsync(() =>
        {
            if (App.Window is MainWindow mainWindow)
            {
                mainWindow.UpdatePlayControlStyle();
            }
        });
    }
}
