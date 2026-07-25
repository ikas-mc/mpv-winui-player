using Microsoft.UI.Xaml.Controls;
using System;

namespace mpv_winui.Modules.Settings.Controls;

public sealed partial class OptionStringControl : OptionControlBase
{
    public OptionStringControl()
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
                if (func() is string value)
                {
                    InputBox.Text = value;
                }
                else
                {
                    InputBox.Text = string.Empty;
                }
            }
        }
    }

    public override (bool IsValid, string? ErrorMessage) Validate()
    {
        if (!(Setting?.AllowEmpty ?? false) && string.IsNullOrWhiteSpace(InputBox.Text))
        {
            return (false, "Value cannot be empty");
        }

        return (true, null);
    }

    private bool TryCommit()
    {
        var (valid, error) = Validate();
        if (!valid)
        {
            ErrorText.Text = error;
            ErrorText.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            return false;
        }
        ErrorText.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        Setting?.Setter?.Invoke(InputBox.Text);
        return true;
    }

    private void InputBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        TryCommit();
    }

    private void OnLostFocus(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        TryCommit();
    }
}