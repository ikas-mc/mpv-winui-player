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
            ApplyText(LabelText, DescriptionText, newValue);
            ApplyIcon(TypeIcon, newValue);

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
    public override (bool IsValid, string? ErrorMessage) Validate() => (true, null);

    private void OnToggled(object sender, RoutedEventArgs e)
    {
        if (Setting?.Setter is not null)
        {
            Setting?.Setter?.Invoke(ToggleSwitch.IsOn);
        }
    }
}