using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Linq;

namespace mpv_winui.Modules.Settings.Controls;

public sealed partial class OptionListControl : UserControl
{
    public OptionListControl()
    {
        InitializeComponent();
    }

    private void OnContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
    {
        if (args.InRecycleQueue)
        {
            return;
        }

        if (args.Item is not Option option)
        {
            return;
        }

        if (args.ItemContainer.ContentTemplateRoot is OptionControlBase control)
        {
            control.Setting = option;
        }
        args.Handled = true;
    }

    public List<Option> OptionList
    {
        get => (List<Option>)GetValue(SettingProperty);
        set => SetValue(SettingProperty, value);
    }

    public static readonly DependencyProperty SettingProperty = DependencyProperty.Register(
            nameof(OptionList),
            typeof(List<Option>),
            typeof(OptionListControl),
            new PropertyMetadata((List<Option>)[], OnOptionListChanged)
            );

    private static void OnOptionListChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is OptionListControl self)
        {
            if (e.NewValue is List<Option> list)
            {
                self.OptionListSource.Source = BuildGroups(list);
            }
            else
            {
                self.OptionListSource.Source = (List<Option>)[];
            }
        }
    }

    private static IReadOnlyList<OptionGroup> BuildGroups(List<Option> options)
    {
        return options.GroupBy(
                o => string.IsNullOrEmpty(o.GroupKey) ? Option.GroupOtherKey : o.GroupKey,
                (k, r) => new OptionGroup(k, r.First()?.GroupLabel, r.ToArray())
               ).ToArray();
    }

    public object? Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    public static readonly DependencyProperty FooterProperty = DependencyProperty.Register(
            nameof(Footer),
            typeof(object),
            typeof(OptionListControl),
            new PropertyMetadata(null)
            );
}
