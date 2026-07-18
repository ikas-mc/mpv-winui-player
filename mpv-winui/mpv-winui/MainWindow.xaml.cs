using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using mpv_winui.Modules.Activation;
using mpv_winui.Modules.AppModel;
using mpv_winui.Modules.Player;

namespace mpv_winui
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            TrySetBackdrop();

            ExtendsContentIntoTitleBar = true;
            ShellTitleBar.Title = PackageHelper.AppName;
            SetTitleBarColors();
            SetTitleBar(ShellTitleBar);

            AppWindow.Changed += AppWindow_Changed;
            AppWindow.Title = PackageHelper.AppName;
            AppWindow.SetIcon("App.ico");

            SetupWindowSize();
        }

        private bool SetTitleBarColors()
        {
            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                AppWindowTitleBar m_TitleBar = AppWindow.TitleBar;

                // Set active window colors.
                // Note: No effect when app is running on Windows 10
                // because color customization is not supported.
                m_TitleBar.ForegroundColor = Colors.White;
                //m_TitleBar.BackgroundColor = Colors.Green;
                m_TitleBar.ButtonForegroundColor = Colors.White;
                //m_TitleBar.ButtonBackgroundColor = Colors.SeaGreen;
                //m_TitleBar.ButtonHoverForegroundColor = Colors.Gainsboro;
                //m_TitleBar.ButtonHoverBackgroundColor = Colors.DarkSeaGreen;
                //m_TitleBar.ButtonPressedForegroundColor = Colors.Gray;
                //m_TitleBar.ButtonPressedBackgroundColor = Colors.LightGreen;

                // Set inactive window colors.
                // Note: No effect when app is running on Windows 10
                // because color customization is not supported.
                //m_TitleBar.InactiveForegroundColor = Colors.Gainsboro;
                //m_TitleBar.InactiveBackgroundColor = Colors.SeaGreen;
                //m_TitleBar.ButtonInactiveForegroundColor = Colors.Gainsboro;
                //m_TitleBar.ButtonInactiveBackgroundColor = Colors.SeaGreen;
                return true;
            }
            return false;
        }

        //https://learn.microsoft.com/en-us/windows/apps/develop/title-bar
        private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args)
        {
            if (args.DidPresenterChange)
            {
                switch (sender.Presenter.Kind)
                {
                    case AppWindowPresenterKind.CompactOverlay:
                        // Compact overlay - hide custom title bar
                        // and use the default system title bar instead.
                        ShellTitleBar.Visibility = Visibility.Collapsed;
                        sender.TitleBar.ResetToDefault();
                        break;

                    case AppWindowPresenterKind.FullScreen:
                        // Full screen - hide the custom title bar
                        // and the default system title bar.
                        ShellTitleBar.Visibility = Visibility.Collapsed;
                        sender.TitleBar.ExtendsContentIntoTitleBar = true;
                        break;

                    case AppWindowPresenterKind.Overlapped:
                        // Normal - hide the system title bar
                        // and use the custom title bar instead.
                        ShellTitleBar.Visibility = Visibility.Visible;
                        sender.TitleBar.ExtendsContentIntoTitleBar = true;
                        break;

                    default:
                        // Use the default system title bar.
                        sender.TitleBar.ResetToDefault();
                        break;
                }
            }
        }

        public void Open()
        {
            var activatedArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
            var pathList = ActivationService.Instance.Parse(activatedArgs);

            ShellFrame.Navigate(typeof(MpvPlayerPage), pathList);
        }

        public void ChangeFullWindow(bool full)
        {
            if (full)
            {
                TitleBarRow.Height = new GridLength(0);
            }
            else
            {
                TitleBarRow.Height = GridLength.Auto;
            }
        }

    }
}