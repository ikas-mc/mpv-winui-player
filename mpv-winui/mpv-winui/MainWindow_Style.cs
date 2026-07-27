using Microsoft.UI.Xaml;
using mpv_winui.Modules.Common.View;

namespace mpv_winui
{
    public sealed partial class MainWindow : Window
    {
        private WindowStyleManager? _styleManager;

        private void SetupStyle()
        {
            _styleManager = new WindowStyleManager(this);
            _styleManager?.Setup();
        }

        private void CleanupStyle()
        {
            _styleManager?.Dispose();
            _styleManager = null;
        }

        public void UpdateCurrentTheme()
        {
            var theme = _styleManager?.GetThemeType();
            if (theme is not null)
            {
                _styleManager?.UpdateTheme(theme.Value);
            }

            _settingsWindow?.UpdateCurrentTheme();
        }
    }
}