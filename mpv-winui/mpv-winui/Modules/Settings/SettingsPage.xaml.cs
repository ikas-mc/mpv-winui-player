using Microsoft.UI.Xaml.Controls;
using mpv_winui.Modules.Settings.Controls;
using System.Collections.Generic;

namespace mpv_winui.Modules.Settings;

public sealed partial class SettingsPage : Page
{
    public List<Option> Settings { get; } = [];

    public SettingsPage()
    {
        InitializeComponent();
        Settings.AddRange(BuildSettings());
    }

    private static List<Option> BuildSettings()
    {
        return
        [
/*            new Option
            {
                Key = nameof(AppContext.AppSetting.ThemeType),
                Label = "Theme",
                Type = OptionType.TextList,
                Options = [AppSettings.ThemeType_Auto, AppSettings.ThemeType_Light, AppSettings.ThemeType_Dark],
                Getter = () => AppContext.AppSetting.ThemeType,
                Setter = v => AppContext.AppSetting.ThemeType = (string)v
            },*/

            new Option
            {
                Key =  nameof(AppContext.AppSetting.BackdropType),
                Label = "Backdrop",
                Type = OptionType.StringList,
                Options = [AppSettings.BackdropType_Acrylic, AppSettings.BackdropType_Mica],
                Getter = () => AppContext.AppSetting.BackdropType,
                Setter = v => AppContext.AppSetting.BackdropType = (string)v
            },

            new Option
            {
                Key =  nameof(AppContext.AppSetting.EnableDebugLog),
                Label = "Debug Log",
                Type = OptionType.Boolean,
                Getter = () => AppContext.AppSetting.EnableDebugLog,
                Setter = v => AppContext.AppSetting.EnableDebugLog = (bool)v!
            },
        ];
    }
}