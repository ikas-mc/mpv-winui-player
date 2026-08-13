using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NLog;
using System;
using System.Linq;

namespace mpv_winui.Modules.ActivationRegistration;

public sealed partial class ProtocolAssociationControl : UserControl
{
    private static readonly Logger _logger = LogManager.GetLogger("ProtocolAssociation");

    private const string ProtocolScheme = "mpvw";

    public ProtocolAssociationControl()
    {
        InitializeComponent();
        LoadRegistrationState();
    }

    private async void LoadRegistrationState()
    {
        try
        {
            var registered = await ActivationRegistrationService.Instance.GetRegisteredProtocolsAsync();
            var isRegistered = registered.Any(name => string.Equals(name, ProtocolScheme, StringComparison.OrdinalIgnoreCase));
            SetRegisteredState(isRegistered);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Load registered protocol failed");
        }
    }

    private void SetRegisteredState(bool isRegistered)
    {
        RegisteredIcon.Visibility = isRegistered ? Visibility.Visible : Visibility.Collapsed;
    }

    private async void OnRegister(object sender, RoutedEventArgs e)
    {
        StatusText.Text = string.Empty;

        try
        {
            await ActivationRegistrationService.Instance.RegisterProtocolAsync(ProtocolScheme);
            SetRegisteredState(true);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Register protocol failed");
            StatusText.Text = $"Register failed: {ex.Message}";
        }
    }

    private async void OnUnregisterAll(object sender, RoutedEventArgs e)
    {
        StatusText.Text = string.Empty;

        try
        {
            await ActivationRegistrationService.Instance.UnregisterProtocolAsync(ProtocolScheme);
            SetRegisteredState(false);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Unregister protocol failed");
            StatusText.Text = $"Unregister failed: {ex.Message}";
        }
    }
}
