using mpv_winui.Modules.Common.View;

namespace mpv_winui
{
    public sealed partial class MainWindow : BaseWindow
    {
        public override void UpdateTheme()
        {
            base.UpdateTheme();
            _windowsManager.UpdateTheme();
        }

        public override void UpdateBackdrop()
        {
            base.UpdateBackdrop();
            _windowsManager.UpdateBackdrop();
        }
    }
}
