using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace mpv_winui.Modules.Settings.Controls;

public sealed partial class OptionNumberControl : OptionControlBase
{
    public OptionNumberControl()
    {
        InitializeComponent();
    }

    protected override void OnSettingChanged(Option? oldValue, Option? newValue)
    {
        if (newValue is not null)
        {
            LabelText.Text = newValue.Label;

            if (newValue.Min.HasValue)
            {
                NumberBox.Minimum = newValue.Min.Value;
            }

            if (newValue.Max.HasValue)
            {
                NumberBox.Maximum = newValue.Max.Value;
            }

            if (newValue.Step.HasValue)
            {
                NumberBox.SmallChange = newValue.Step.Value;
            }

            if (newValue.Getter is Func<object?> func)
            {
                if (func() is double value)
                {
                    NumberBox.Value = value;
                }
                else
                {
                    NumberBox.Value = 0;
                }
            }
        }
    }

    private void OnValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        Setting?.Setter?.Invoke(args.NewValue);
    }
}