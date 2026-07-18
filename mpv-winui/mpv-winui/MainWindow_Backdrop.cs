using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using WinRT;

namespace mpv_winui
{
    public sealed partial class MainWindow : Window
    {
        private SystemBackdropConfiguration? _configurationSource;
        private DesktopAcrylicController? _acrylicController;

        private void TrySetBackdrop()
        {
            DispatcherQueue.EnsureSystemDispatcherQueue();
            _configurationSource = new SystemBackdropConfiguration();

            Activated += Window_Activated;
            Closed += Window_Closed;
            ((FrameworkElement)Content).ActualThemeChanged += Window_ThemeChanged;

            _configurationSource.IsInputActive = true;
            SetConfigurationSourceTheme();

            if (DesktopAcrylicController.IsSupported())
            {
                _acrylicController = new DesktopAcrylicController
                {
                    Kind = DesktopAcrylicKind.Thin,
                    //TintOpacity = 0.15F,
                    //TintColor = Windows.UI.Color.FromArgb(255, 255, 255, 255),
                    //LuminosityOpacity = 0.2F
                };

                _acrylicController?.AddSystemBackdropTarget(this.As<ICompositionSupportsSystemBackdrop>());
                _acrylicController?.SetSystemBackdropConfiguration(_configurationSource);
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

            _acrylicController?.Dispose();
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
    }
}