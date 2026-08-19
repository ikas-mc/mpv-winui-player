using Microsoft.UI.Xaml.Controls;
using mpv_winui.Modules.AppModel;

namespace mpv_winui.Modules.About
{
    public sealed partial class AboutControl : UserControl
    {
        private AboutControl()
        {
            this.InitializeComponent();
        }

        public static AboutControl Create(string? mpvVersion)
        {
            var control = new AboutControl();
            control.AppNameTextBlock.Text = PackageHelper.AppName;
            control.AppVersionTextBlock.Text = PackageHelper.AppVersion;
            control.MpvVersionTextBlock.Text = string.IsNullOrEmpty(mpvVersion) ? "mpv" : mpvVersion;
            return control;
        }
    }
}