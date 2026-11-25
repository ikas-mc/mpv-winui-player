using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using mpv_winui.Modules.AppModel;
using mpv_winui.Modules.Common.View;
using mpv_winui.Modules.Language;
using NLog;
using System;

namespace mpv_winui
{
    public partial class App : Application
    {
        private static readonly Logger _logger = LogManager.GetLogger("App");

        public static Window? Window
        {
            get; private set;
        }

        public App()
        {
            UpdatePrimaryLanguage();
            InitializeComponent();
            AppContext.Init();
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            await AppContext.WaitAll();

            var window = new MainWindow();
            Window = window;
            window.Open();
            window.Activate();
        }

        public void OnActivated(AppActivationArguments args)
        {
            if (_logger.IsDebugEnabled)
            {
                _logger.Debug("OnActivated, kind={}", args.Kind);
            }

            if (Window is MainWindow mainWindow)
            {
                mainWindow?.Refresh(args);
            }

            Window?.ShowAndForegroundWindow();
        }

        private void UpdatePrimaryLanguage()
        {
            if (!PackageHelper.IsPackaged)
            {
                try
                {
                    AppLanguageService.UpdatePrimaryLanguage();
                }
                catch (Exception)
                {
                    //
                }
            }
        }

    }
}
