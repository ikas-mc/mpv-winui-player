using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using mpv_winui.Modules.Common.Utils;
using mpv_winui.Modules.Settings;
using Windows.UI.ViewManagement;
using WinRT;

namespace mpv_winui
{
    public sealed partial class MainWindow : Window
    {
        private SystemBackdropConfiguration? _configurationSource;
        private DesktopAcrylicController? _acrylicController;
        private MicaController? _micaController;
        private ElementTheme _theme;

        private void SetupStyle()
        {
            _theme = GetThemeType();
            UpdateTitleBarColors(_theme);
            UpdateContentTheme(_theme);

            DispatcherQueue.EnsureSystemDispatcherQueue();
            _configurationSource = new SystemBackdropConfiguration
            {
                IsInputActive = true
            };

            switch (AppContext.AppSetting.BackdropType)
            {
                case AppSettings.BackdropType_Mica:
                {
                    if (MicaController.IsSupported())
                    {
                        _micaController = new MicaController();
                        _micaController?.AddSystemBackdropTarget(this.As<ICompositionSupportsSystemBackdrop>());
                        _micaController?.SetSystemBackdropConfiguration(_configurationSource);
                    }
                    break;
                }
                default:
                {
                    if (DesktopAcrylicController.IsSupported())
                    {
                        _acrylicController = new DesktopAcrylicController();
                        _acrylicController?.AddSystemBackdropTarget(this.As<ICompositionSupportsSystemBackdrop>());
                        _acrylicController?.SetSystemBackdropConfiguration(_configurationSource);
                    }
                    break;
                }
            }

            UpdateBackdropTheme(_theme);

            _uISettings.ColorValuesChanged += Settings_ColorValuesChanged;
        }

        private void CleanupTheme()
        {
            _uISettings?.ColorValuesChanged -= Settings_ColorValuesChanged;
            _acrylicController?.Dispose();
            _acrylicController = null;
            _micaController?.Dispose();
            _micaController = null;
            _configurationSource = null;
        }

        private void Settings_ColorValuesChanged(UISettings sender, object args)
        {
            DispatcherQueue.RunAsync(() =>
            {
                var theme = GetThemeType();
                if (_theme != theme)
                {
                    UpdateCurrentTheme(theme);
                }
                else
                {
                    _acrylicController?.TintColor = _uISettings.GetColorValue(UIColorType.AccentLight1);
                }
            });
        }

        private void UpdateContentTheme(ElementTheme theme)
        {
            Body.RequestedTheme = theme;
        }

        private void UpdateBackdropTheme(ElementTheme theme)
        {
            _configurationSource?.Theme = theme switch
            {
                ElementTheme.Dark => SystemBackdropTheme.Dark,
                _ => SystemBackdropTheme.Light
            };

            if (theme == ElementTheme.Dark)
            {
                _acrylicController?.Kind = DesktopAcrylicKind.Thin;
                _acrylicController?.TintOpacity = 0.2F;
                _acrylicController?.TintColor = _uISettings.GetColorValue(UIColorType.AccentLight1);
                _acrylicController?.LuminosityOpacity = 0.1F;
            }
            else
            {
                _acrylicController?.Kind = DesktopAcrylicKind.Thin;
                _acrylicController?.TintOpacity = 0.2F;
                _acrylicController?.TintColor = Colors.White;
                _acrylicController?.LuminosityOpacity = 0.8F;
            }
        }

        private bool UpdateTitleBarColors(ElementTheme theme)
        {
            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                AppWindowTitleBar titleBar = AppWindow.TitleBar;

                //titleBar.ForegroundColor = Colors.White;
                //titleBar.BackgroundColor = Colors.Green;
                titleBar.ButtonForegroundColor = theme == ElementTheme.Dark ? Colors.White : Colors.Black;
                titleBar.ButtonBackgroundColor = Colors.Transparent;
                //titleBar.ButtonHoverForegroundColor = Colors.Gainsboro;
                //titleBar.ButtonHoverBackgroundColor = Colors.Transparent;
                //titleBar.ButtonPressedForegroundColor = Colors.Gray;
                //titleBar.ButtonPressedBackgroundColor = Colors.LightGreen;

                //titleBar.InactiveForegroundColor = Colors.Gainsboro;
                //titleBar.InactiveBackgroundColor = Colors.SeaGreen;
                //titleBar.ButtonInactiveForegroundColor = Colors.Gainsboro;
                titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

                return true;
            }
            return false;
        }

        public ElementTheme GetThemeType()
        {
            return AppContext.AppSetting.ThemeType switch
            {
                AppSettings.ThemeType_Dark => ElementTheme.Dark,
                AppSettings.ThemeType_Light => ElementTheme.Light,
                _ => App.Current.RequestedTheme == ApplicationTheme.Light ? ElementTheme.Light : ElementTheme.Dark,
            };
        }

        public void UpdateCurrentTheme(ElementTheme theme)
        {
            _theme = theme;
            UpdateContentTheme(theme);
            UpdateBackdropTheme(theme);
            UpdateTitleBarColors(theme);
        }
    }
}