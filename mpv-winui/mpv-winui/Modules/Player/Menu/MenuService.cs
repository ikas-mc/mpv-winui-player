using mpv_winui.Modules.FileSystem;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace mpv_winui.Modules.Player.Menu
{
    public class MenuService
    {
        private static readonly Lazy<MenuService> _lazyValue = new(() => new MenuService(), true);
        public static MenuService Instance => _lazyValue.Value;

        public const string DefaultFileName = "menu.json";

        private static readonly Logger _logger = LogManager.GetLogger(nameof(MenuService));

        private readonly string _filePath;

        public MenuService()
        {
            _filePath = AppData.Current.ResolveLocalData(DefaultFileName);
        }

        public async ValueTask<List<CustomMenuItem>?> TryLoadAsync()
        {
            if (_logger.IsDebugEnabled)
            {
                _logger.Debug("load custom menus, path={}", _filePath);
            }

            if (string.IsNullOrEmpty(_filePath))
            {
                return null;
            }

            return await Task.Run(async () =>
            {
                if (!File.Exists(_filePath))
                {
                    return null;
                }

                using var stream = new FileStream(_filePath, FileMode.Open);
                return await JsonSerializer.DeserializeAsync(stream, MenuJsonContext.Default.ListCustomMenuItem);
            });
        }
    }
}
