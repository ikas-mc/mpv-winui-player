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

    private void Commit()
    {
        Setting?.Setter?.Invoke(InputBox.Text);
    }

    private void InputBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        Commit();
    }
}