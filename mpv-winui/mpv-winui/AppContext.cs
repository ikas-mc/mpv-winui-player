using mpv_winui.Modules.Language;
using mpv_winui.Modules.Settings;
using NLog;
using System.Threading.Tasks;

namespace mpv_winui
{
    public class AppContext
    {
        public static readonly Logger AppLogger = LogManager.GetLogger("App");

        public static AppLang AppLang { get; } = new();

        public static AppSettings AppSetting { get; } = new();

        private static Task? _task;

        public static void Init()
        {
            if (AppLogger.IsDebugEnabled)
            {
                AppLogger.Debug("\n\nAppContext Start");
            }

            _task = Task.WhenAll([
                Task.Run(() => { AppSetting.EnableLog = true; }),
                AppBootstrap.RunAsync()
            ]);
        }

        public static async Task WaitAll()
        {
            if (_task != null)
            {
                await _task;
            }

            _task = null;
        }
    }

    public static class AppBootstrap
    {
        public static async Task RunAsync()
        {
            await Task.Run(() => { });
        }
    }
}