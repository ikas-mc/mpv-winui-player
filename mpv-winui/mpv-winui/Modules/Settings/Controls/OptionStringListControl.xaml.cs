using Microsoft.UI.Xaml.Controls;
using System;

namespace mpv_winui.Modules.Settings.Controls;

public sealed partial class OptionStringListControl : OptionControlBase
{
    public OptionStringListControl()
    {
        InitializeComponent();
    }

    protected override void OnSettingChanged(Option? oldValue, Option? newValue)
    {
        if (newValue is not null)
        {
            ApplyText(LabelText, DescriptionText, newValue);
            ApplyIcon(TypeIcon, newValue);

            Combo.Items.Clear();
            if (newValue.Options is not null)
            {
                foreach (var opt in newValue.Options)
                {
                    Combo.Items.Add(opt);
                }
            }

            if (newValue.Getter is Func<object?> func)
            {
                if (func() is string current && !string.IsNullOrEmpty(current))
                {
                    var index = Combo.Items.IndexOf(current);
                    if (index >= 0)
                    {
                        Combo.SelectedIndex = index;
                    }
                }
            }
        }
    }

    public override (bool IsValid, string? ErrorMessage) Validate() => (true, null);

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (Setting?.Setter is not null && Combo.SelectedItem is string val)
        {
            Setting?.Setter?.Invoke(val);
        }
    }
}