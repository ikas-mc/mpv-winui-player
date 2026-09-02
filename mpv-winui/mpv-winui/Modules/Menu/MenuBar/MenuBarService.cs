using mpv_winui.Modules.FileSystem;
using mpv_winui.Modules.Menu.MpvMenu;
using NLog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace mpv_winui.Modules.Menu.MenuBar
{
    public class MenuBarService
    {
        private static readonly Logger _logger = LogManager.GetLogger(nameof(MenuBarService));
        private static readonly Lazy<MenuBarService> _lazyValue = new(() => new MenuBarService(), true);
        public static MenuBarService Instance => _lazyValue.Value;

        public const string DefaultFileName = "mpvw-menu.conf";
        private readonly string _filePath;

        public MenuBarService()
        {
            _filePath = AppData.Current.ResolveLocalData(DefaultFileName);
        }

        public string FilePath => _filePath;

        public async ValueTask<List<MpvMenuItem>?> TryLoadAsync()
        {
            if (_logger.IsDebugEnabled)
            {
                _logger.Debug("load custom menus, path={}", _filePath);
            }

            if (string.IsNullOrEmpty(_filePath))
            {
                return null;
            }

            try
            {
                return await Task.Run(() => MenuConfParser.Parse(_filePath));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "load custom menus failed, path={}", _filePath);
                return null;
            }
        }
    }
}
