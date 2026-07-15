using Microsoft.UI.Xaml;
using mpv_winui.Modules.FileSystem;
using NLog;
using System.Text;

namespace mpv_winui
{
    public partial class App : Application
    {
        private static readonly Logger _logger = LogManager.GetLogger("App");

        static App()
        {
            LogManager.Setup().LoadConfiguration(builder =>
            {
#if DEBUG
                var level = LogLevel.Trace;
                builder.ForLogger().FilterMinLevel(level).WriteToDebug();
#else
                var level = LogLevel.Error;
#endif
                builder.ForLogger()
                    .FilterMinLevel(level)
                    .WriteToFile(fileName: AppData.Current.ResolveLocalData("logs\\mpv-winui.${shortdate}.log.txt"), encoding: Encoding.UTF8, keepFileOpen: false, maxArchiveDays: 15);
            });
        }

        public static Window? Window
        {
            get; private set;
        }

        public App()
        {
            InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            AppContext.Init();
            var window = new MainWindow();
            Window = window;
            window.Open();
            window.Activate();
        }
    }
}