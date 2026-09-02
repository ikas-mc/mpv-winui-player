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
                Label = "Theme",
                Description = "App theme",
                Icon = "\uE790",
                GroupKey="Style",
                GroupLabel="Style",
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
                Label = "Backdrop",
                Description = "Window background. Restart required.",
                Icon="\uE91B",
                GroupKey="Style",
                GroupLabel="Style",
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
                Label = "Player Control Style",
                Description = "Player control style.",
                Icon = "\uE768",
                GroupKey = "Style",
                GroupLabel = "Style",
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
                Label = "Thumbnails Preview",
                Description = "Using a plugin that supports the mpv osc-preview-api.",
                Icon = "\uE8B2",
                GroupKey="Preview",
                GroupLabel="Preview",
                Type = OptionType.Boolean,
                Getter = () => AppContext.AppSetting.EnableVideoPreview,
                Setter = v => AppContext.AppSetting.EnableVideoPreview = (bool)v!
            },

            new Option
            {
                Key =  nameof(AppContext.AppSetting.EnableVideoBuiltInPreview),
                Label = "Built-in Thumbnails Preview",
                Description = "Built-in preview. Restart required.",
                Icon = "\uE8B2",
                GroupKey="Preview",
                GroupLabel="Preview",
                Type = OptionType.Boolean,
                Getter = () => AppContext.AppSetting.EnableVideoBuiltInPreview,
                Setter = v => AppContext.AppSetting.EnableVideoBuiltInPreview = (bool)v!
            },

            new Option
            {
                Key =  nameof(AppContext.AppSetting.KeepVideoBuiltInPreviewAlive),
                Label = "Built-in Preview Keep Alive",
                Description = "Keep the preview mpv instance alive. Restart required.",
                Icon = "\uE8B2",
                GroupKey="Preview",
                GroupLabel="Preview",
                Type = OptionType.Boolean,
                Getter = () => AppContext.AppSetting.KeepVideoBuiltInPreviewAlive,
                Setter = v => AppContext.AppSetting.KeepVideoBuiltInPreviewAlive = (bool)v!
            },

            new Option
            {
                Key =  nameof(AppContext.AppSetting.BuiltInPreviewAliveTimeout),
                Label = "Built-in Preview Alive Timeout (s)",
                Description = "Seconds the preview mpv instance stays alive.",
                Icon = "\uE8B2",
                GroupKey="Preview",
                GroupLabel="Preview",
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
                Label = "Mouse Input",
                Description = "Enable mouse input to mpv. Restart required.",
                Icon = "\uE961",
                GroupKey="Input",
                GroupLabel="Input",
                Type = OptionType.Boolean,
                Getter = () => AppContext.AppSetting.EnableMouseInput,
                Setter = v => AppContext.AppSetting.EnableMouseInput = (bool)v!
            },

            new Option
            {
                Key =  nameof(AppContext.AppSetting.EnableMouseInputDiscNavOnly),
                Label = "Mouse Input Disc Nav Only",
                Description = "When mouse input is enabled, send events to mpv only when the disc menu is active. Restart required.",
                Icon = "\uE961",
                GroupKey="Input",
                GroupLabel="Input",
                Type = OptionType.Boolean,
                Getter = () => AppContext.AppSetting.EnableMouseInputDiscNavOnly,
                Setter = v => AppContext.AppSetting.EnableMouseInputDiscNavOnly = (bool)v!
            },

            new Option
            {
                Key =  nameof(AppContext.AppSetting.EnableDebugLog),
                Label = "Debug Log",
                Description = "Enable app and mpv debug log. Restart required.",
                Icon = "\uE946",
                GroupKey="Data",
                GroupLabel="Data",
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
