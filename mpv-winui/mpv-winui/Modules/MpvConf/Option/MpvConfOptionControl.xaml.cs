using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using mpv_winui.Modules.MpvConf.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Globalization.NumberFormatting;

namespace mpv_winui.Modules.MpvConf.Option;

public sealed partial class MpvConfOptionControl : MpvConfOptionControlBase
{
    private readonly List<MpvConfSchemaItemValue> _types = [];

    public MpvConfOptionControl()
    {
        InitializeComponent();

        var formatter = new DecimalFormatter
        {
            FractionDigits = 0,
            IsGrouped = false,
            NumberRounder = new IncrementNumberRounder
            {
                Increment = 1,
                RoundingAlgorithm = RoundingAlgorithm.RoundHalfUp,
            }
        };
        IntInput.NumberFormatter = formatter;
    }

    protected override void UpdateInfo()
    {
        KeyText.Text = Item?.Key ?? string.Empty;
        GroupText.Text = Item?.Group ?? string.Empty;

        bool isDefaultProfile = string.IsNullOrEmpty(Item?.Profile);
        ApplyButton.IsEnabled = isDefaultProfile;

        if (string.IsNullOrEmpty(Item?.Description))
        {
            DescriptionText.Text = null;
            ToolTipService.SetToolTip(DescriptionText, null);
            DescriptionText.Visibility = Visibility.Collapsed;
        }
        else
        {
            DescriptionText.Text = Item?.Description;
            ToolTipService.SetToolTip(DescriptionText, Item?.Description);
            DescriptionText.Visibility = Visibility.Visible;
        }
    }

    protected override void UpdateModifiedState()
    {
        bool isModified = Item?.IsModified == true;
        ModifiedIcon.Visibility = isModified ? Visibility.Visible : Visibility.Collapsed;
    }

    protected override void UpdateEnabledState()
    {
        MpvOptionState state = Item?.State ?? MpvOptionState.NotInFile;
        UsingSuppressEdit(() =>
        {
            EnableCheck.IsChecked = state != MpvOptionState.NotInFile;
            DisableCheck.IsChecked = state == MpvOptionState.Disabled;
        });
    }

    protected override void UpdateOptionValue()
    {
        MpvConfOptionItem? item = Item;

        _types.Clear();
        if (item?.Definition?.Types is { } declared)
        {
            _types.AddRange(declared);
        }

        var index = GuessTypeIndex(_types, item);

        if (_types.All(t => t.Type != MpvConfSchemaItemValue.Raw))
        {
            _types.Add(new MpvConfSchemaItemValue { Type = MpvConfSchemaItemValue.Raw });
        }

        UsingSuppressEdit(() =>
        {
            TypeBox.ItemsSource = null;
            TypeBox.ItemsSource = _types;
            TypeBox.SelectedIndex = index;
        });

        PopulateEditor(index);
    }

    private void PopulateEditor(int index)
    {
        if (index < 0 || index >= _types.Count)
        {
            return;
        }

        MpvConfSchemaItemValue type = _types[index];
        string? raw = Item?.Value;

        switch (MpvConfOptionHelper.ResolveEditorKind(type))
        {
            case MpvOptionEditorKind.Bool:
                ShowEditor(BoolSwitch);
                UsingSuppressEdit(() => BoolSwitch.IsOn = MpvConfOptionHelper.ParseBool(raw) ?? false);
                break;

            case MpvOptionEditorKind.Enum:
                ShowEditor(EnumBox);
                PopulateEnumBox(type, raw);
                break;

            case MpvOptionEditorKind.Int:
                ShowEditor(IntInput);
                IntInput.Minimum = type.Minimum ?? long.MinValue;
                IntInput.Maximum = type.Maximum ?? long.MaxValue;
                UsingSuppressEdit(() => IntInput.Value = raw is null ? double.NaN : MpvConfOptionHelper.ParseInt(raw));
                break;

            case MpvOptionEditorKind.Float:
                ShowEditor(FloatInput);
                FloatInput.Minimum = type.Minimum ?? double.MinValue;
                FloatInput.Maximum = type.Maximum ?? double.MaxValue;
                UsingSuppressEdit(() => FloatInput.Value = raw is null ? double.NaN : MpvConfOptionHelper.ParseFloat(raw));
                break;

            default:
                ShowEditor(TextInput);
                TextInput.Text = raw ?? string.Empty;
                break;
        }
    }

    private void ShowEditor(FrameworkElement active)
    {
        BoolSwitch.Visibility = ReferenceEquals(active, BoolSwitch) ? Visibility.Visible : Visibility.Collapsed;
        EnumBox.Visibility = ReferenceEquals(active, EnumBox) ? Visibility.Visible : Visibility.Collapsed;
        IntInput.Visibility = ReferenceEquals(active, IntInput) ? Visibility.Visible : Visibility.Collapsed;
        FloatInput.Visibility = ReferenceEquals(active, FloatInput) ? Visibility.Visible : Visibility.Collapsed;
        TextInput.Visibility = ReferenceEquals(active, TextInput) ? Visibility.Visible : Visibility.Collapsed;
    }

    private void PopulateEnumBox(MpvConfSchemaItemValue type, string? raw)
    {
        var choices = new List<MpvConfEnumItem>();
        if (type.EnumValues is { } values)
        {
            foreach (string value in values)
            {
                choices.Add(new MpvConfEnumItem(value, value));
            }
        }

        MpvConfEnumItem? selected = raw is not null ? choices.FirstOrDefault(c => string.Equals(c.Value, raw, StringComparison.Ordinal)) : null;
        if (selected is null && raw is { Length: > 0 })
        {
            selected = new MpvConfEnumItem(raw, raw);
            choices.Insert(0, selected);
        }

        UsingSuppressEdit(() =>
        {
            EnumBox.ItemsSource = choices;
            EnumBox.SelectedItem = selected;
        });
    }

    private static int GuessTypeIndex(IReadOnlyList<MpvConfSchemaItemValue> types, MpvConfOptionItem? item)
    {
        if (item is null || types.Count == 0)
        {
            return 0;
        }

        string raw = item.Value;
        for (int i = 0; i < types.Count; i++)
        {
            MpvConfSchemaItemValue type = types[i];
            if (type.HasEnum && type.EnumValues!.Contains(raw, StringComparer.Ordinal))
            {
                return i;
            }

            if (type.Type == MpvConfSchemaItemValue.Bool && MpvConfOptionHelper.ParseBool(raw) is not null)
            {
                return i;
            }

            if (type.Type == MpvConfSchemaItemValue.Int && !double.IsNaN(MpvConfOptionHelper.ParseInt(raw)))
            {
                return i;
            }

            if (type.Type == MpvConfSchemaItemValue.Float && !double.IsNaN(MpvConfOptionHelper.ParseFloat(raw)))
            {
                return i;
            }
        }

        return 0;
    }

    private void OnTypeSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SuppressEdit || TypeBox.SelectedIndex < 0 || Item is null)
        {
            return;
        }

        PopulateEditor(TypeBox.SelectedIndex);
    }

    private void OnEnableCheckChanged(object sender, bool isChecked)
    {
        if (SuppressEdit || Item is null)
        {
            return;
        }

        if (!isChecked)
        {
            DisableCheck.IsChecked = false;
            RaiseStateChangeRequested(Item, MpvOptionState.NotInFile);
        }
        else
        {
            RaiseStateChangeRequested(Item, DisableCheck.IsChecked == true ? MpvOptionState.Disabled : MpvOptionState.Enabled);
        }

        UpdateModifiedState();
    }

    private void OnDisableCheckChanged(object sender, bool isChecked)
    {
        if (SuppressEdit || Item is null)
        {
            return;
        }

        if (isChecked)
        {
            if (EnableCheck.IsChecked == false)
            {
                EnableCheck.IsChecked = true;
            }

            RaiseStateChangeRequested(Item, MpvOptionState.Disabled);
        }
        else
        {
            RaiseStateChangeRequested(Item, EnableCheck.IsChecked == true ? MpvOptionState.Enabled : MpvOptionState.NotInFile);
        }

        UpdateModifiedState();
    }

    private void OnBoolToggled(object sender, RoutedEventArgs e)
    {
        if (!SuppressEdit && Item is not null)
        {
            RaiseValueChangeRequested(Item, MpvConfOptionHelper.FormatBool(BoolSwitch.IsOn));
            UpdateModifiedState();
        }
    }

    private void OnEnumSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!SuppressEdit && Item is not null && EnumBox.SelectedItem is MpvConfEnumItem choice)
        {
            RaiseValueChangeRequested(Item, choice.Value);
            UpdateModifiedState();
        }
    }

    private void OnIntChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (!SuppressEdit && Item is not null && !double.IsNaN(args.NewValue))
        {
            RaiseValueChangeRequested(Item, MpvConfOptionHelper.FormatNumber(args.NewValue));
            UpdateModifiedState();
        }
    }

    private void OnFloatChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (!SuppressEdit && Item is not null && !double.IsNaN(args.NewValue))
        {
            RaiseValueChangeRequested(Item, MpvConfOptionHelper.FormatNumber(args.NewValue));
            UpdateModifiedState();
        }
    }

    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        //TODO TextBox SuppressEdit
        if (!SuppressEdit && Item is not null && TextInput.Text != Item.Value)
        {
            RaiseValueChangeRequested(Item, TextInput.Text);
            UpdateModifiedState();
        }
    }

    private void OnApplyClicked(object sender, RoutedEventArgs e)
    {
        if (Item is not null)
        {
            RaiseApplyRequested(Item);
        }
    }

    private void OnCopyClicked(object sender, RoutedEventArgs e)
    {
        if (Item is not null)
        {
            SetLineToClipboard(Item);
        }
    }
}
