using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace mpv_winui.Modules.Settings.Controls;

public sealed partial class OptionDoubleControl : OptionControlBase
{
    public OptionDoubleControl()
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
            else
            {
                NumberBox.SmallChange = 0.1;
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

    public override (bool IsValid, string? ErrorMessage) Validate()
    {
        var val = NumberBox.Value;
        if (Setting is not null)
        {
            if (Setting.Min.HasValue && val < Setting.Min.Value)
            {
                return (false, $"Minimum value is {Setting.Min.Value}");
            }

            if (Setting.Max.HasValue && val > Setting.Max.Value)
            {
                return (false, $"Maximum value is {Setting.Max.Value}");
            }
        }
        return (true, null);
    }

    private bool TryCommit()
    {
        var (valid, error) = Validate();
        if (!valid)
        {
            ErrorText.Text = error;
            ErrorText.Visibility = Visibility.Visible;
            return false;
        }
        ErrorText.Visibility = Visibility.Collapsed;
        Setting?.Setter?.Invoke(NumberBox.Value);
        return true;
    }

    private void OnValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        TryCommit();
    }

    private void OnLostFocus(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        TryCommit();
    }
}