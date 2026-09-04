namespace mpv_winui.Modules.FileSystem
{
    public class AppData
    {
        private static readonly Lazy<AppData> _lazy = new(() => new AppData());

        public static AppData Current => _lazy.Value;

        public static string Root { get; set; } = Path.Combine(Path.GetTempPath(), "mpv-winui-test-appdata");

        public string ResolveLocalData(string path)
        {
            return Path.Combine(Root, path);
        }
    }
}