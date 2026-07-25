using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace mpv_winui.Modules.Settings.Controls;

public sealed partial class OptionBooleanControl : OptionControlBase
{
    public OptionBooleanControl()
    {
        InitializeComponent();
    }

    protected override void OnSettingChanged(Option? oldValue, Option? newValue)
    {
        if (newValue is not null)
        {
            LabelText.Text = newValue.Label;

            if (newValue.Getter is Func<object?> func)
            {
                if (func() is bool value)
                {
                    ToggleSwitch.IsOn = value;
                }
                else
                {
                    ToggleSwitch.IsOn = false;
                }
            }
        }
    }

    private void OnToggled(object sender, RoutedEventArgs e)
    {
        Setting?.Setter?.Invoke(ToggleSwitch.IsOn);
    }
}