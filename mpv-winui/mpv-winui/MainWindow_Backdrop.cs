using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Windows.UI.ViewManagement;
using WinRT;

namespace mpv_winui
{
    public sealed partial class MainWindow : Window
    {
        private SystemBackdropConfiguration? _configurationSource;
        private DesktopAcrylicController? _acrylicController;
        private MicaController? _micaController;
        private UISettings? _uISettings;

        private void TrySetBackdrop()
        {
            DispatcherQueue.EnsureSystemDispatcherQueue();
            _configurationSource = new SystemBackdropConfiguration();

            Activated += Window_Activated;
            Closed += Window_Closed;
            ((FrameworkElement)Content).ActualThemeChanged += Window_ThemeChanged;
            _configurationSource.IsInputActive = true;
            SetConfigurationSourceTheme();

            switch (AppContext.AppSetting.BackdropType)
            {
                case 1:
                {
                    if (MicaController.IsSupported())
                    {
                        _micaController = new MicaController
                        {
                        };

                        _micaController?.AddSystemBackdropTarget(this.As<ICompositionSupportsSystemBackdrop>());
                        _micaController?.SetSystemBackdropConfiguration(_configurationSource);
                    }
                    break;
                }
                default:
                {
                    if (DesktopAcrylicController.IsSupported())
                    {
                        var uISettings = new UISettings();
                        uISettings.ColorValuesChanged += Settings_ColorValuesChanged;
                        _uISettings = uISettings;

                        _acrylicController = new DesktopAcrylicController
                        {
                            Kind = DesktopAcrylicKind.Thin,
                            TintOpacity = 0.2F,
                            TintColor = uISettings.GetColorValue(UIColorType.AccentLight1),
                            LuminosityOpacity = 0.1F
                        };

                        _acrylicController?.AddSystemBackdropTarget(this.As<ICompositionSupportsSystemBackdrop>());
                        _acrylicController?.SetSystemBackdropConfiguration(_configurationSource);
                    }
                    break;
                }
            }
        }

        private void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
            //TODO config
            // configurationSource?.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            Closed -= Window_Closed;
            ((FrameworkElement)Content).ActualThemeChanged -= Window_ThemeChanged;
            Activated -= Window_Activated;
            _uISettings?.ColorValuesChanged -= Settings_ColorValuesChanged;
            _acrylicController?.Dispose();
            _acrylicController = null;
            _micaController?.Dispose();
            _acrylicController = null;
            _configurationSource = null;
        }

        private void Window_ThemeChanged(FrameworkElement sender, object args)
        {
            if (_configurationSource != null)
            {
                SetConfigurationSourceTheme();
            }
        }

        private void SetConfigurationSourceTheme()
        {
            _configurationSource?.Theme = (SystemBackdropTheme)((FrameworkElement)Content).ActualTheme;
        }

        private void Settings_ColorValuesChanged(UISettings sender, object args)
        {
            _acrylicController?.TintColor = sender.GetColorValue(UIColorType.AccentLight1);
        }
    }
}