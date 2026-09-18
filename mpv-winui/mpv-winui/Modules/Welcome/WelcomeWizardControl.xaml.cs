using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace mpv_winui.Modules.Welcome
{
    public sealed partial class WelcomeWizardControl : UserControl
    {
        private WelcomeWizardControl()
        {
            this.InitializeComponent();
        }

        public static WelcomeWizardControl Create()
        {
            var control = new WelcomeWizardControl();
            return control;
        }

        public event EventHandler? ImportRequested;

        private void ImportSampleDataButton_Click(object sender, RoutedEventArgs e)
        {
            ImportRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}