using Microsoft.UI.Xaml;
using mpv_winui.Modules.FileSystem;
using NLog;
using System.Text;

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
    }
}