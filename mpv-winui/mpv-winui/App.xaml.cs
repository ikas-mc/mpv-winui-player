using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using mpv_winui.Modules.Common.View;
using NLog;

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
            InitializeComponent();
            AppContext.Init();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
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

            Window?.ShowWindow();
        }
    }
}