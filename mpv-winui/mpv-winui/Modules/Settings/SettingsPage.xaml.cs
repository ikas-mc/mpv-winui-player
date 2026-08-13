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
                Type = OptionType.StringList,
                Options = [AppSettings.ThemeType_Auto, AppSettings.ThemeType_Light, AppSettings.ThemeType_Dark],
                Getter = () => AppContext.AppSetting.ThemeType,
                Setter = v =>{
                    AppContext.AppSetting.ThemeType = (string)v;
                    UpdateTheme((string)v);
                }
            },

            new Option
            {
                Key =  nameof(AppContext.AppSetting.BackdropType),
                Label = "Backdrop (Restart Required)",
                Type = OptionType.StringList,
                Options = [AppSettings.BackdropType_Acrylic, AppSettings.BackdropType_Mica],
                Getter = () => AppContext.AppSetting.BackdropType,
                Setter = v => AppContext.AppSetting.BackdropType = (string)v
            },

            new Option
            {
                Key =  nameof(AppContext.AppSetting.EnableDebugLog),
                Label = "Debug Log (Restart Required)",
                Type = OptionType.Boolean,
                Getter = () => AppContext.AppSetting.EnableDebugLog,
                Setter = v => AppContext.AppSetting.EnableDebugLog = (bool)v!
            },

            new Option
            {
                Key =  nameof(AppContext.AppSetting.EnableVideoPreview),
                Label = "Preview (Restart Required)",
                Type = OptionType.Boolean,
                Getter = () => AppContext.AppSetting.EnableVideoPreview,
                Setter = v => AppContext.AppSetting.EnableVideoPreview = (bool)v!
            },

            new Option
            {
                Key =  nameof(AppContext.AppSetting.EnableVideoBuiltInPreview),
                Label = "Built-in Preview (Restart Required)",
                Type = OptionType.Boolean,
                Getter = () => AppContext.AppSetting.EnableVideoBuiltInPreview,
                Setter = v => AppContext.AppSetting.EnableVideoBuiltInPreview = (bool)v!
            },

            new Option
            {
                Key =  nameof(AppContext.AppSetting.KeepVideoBuiltInPreviewAlive),
                Label = "Built-in Preview Keep Alive (Restart Required)",
                Type = OptionType.Boolean,
                Getter = () => AppContext.AppSetting.KeepVideoBuiltInPreviewAlive,
                Setter = v => AppContext.AppSetting.KeepVideoBuiltInPreviewAlive = (bool)v!
            },

            new Option
            {
                Key = nameof(AppContext.AppSetting.BuiltInPreviewAliveTimeout),
                Label = "Built-in Preview Alive Timeout(s) (Restart Required)",
                Type = OptionType.Integer,
                Min = 0,
                Max = int.MaxValue,
                Step = 10,
                Getter = () => AppContext.AppSetting.BuiltInPreviewAliveTimeout,
                Setter = v => AppContext.AppSetting.BuiltInPreviewAliveTimeout = (int)v!
            },
        ];
    }

    private void UpdateTheme(string theme)
    {
        DispatcherQueue.RunAsync(() =>
        {
            if (App.Window is MainWindow mainWindow)
            {
                mainWindow.UpdateCurrentTheme();
            }
        });
    }
}
