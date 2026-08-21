using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using mpv_winui.Modules.Common.Utils;
using System;

namespace mpv_winui.Modules.MpvConf.Option;

public abstract class MpvConfOptionControlBase : UserControl
{
    public event EventHandler<MpvConfOptionItemEventArgs>? ApplyRequested;

    public event EventHandler<MpvConfOptionStateChangeEventArgs>? StateChangeRequested;

    public event EventHandler<MpvConfOptionValueChangeEventArgs>? ValueChangeRequested;

    protected void RaiseApplyRequested(MpvConfOptionItem item)
    {
        ApplyRequested?.Invoke(this, new MpvConfOptionItemEventArgs(item));
    }

    protected void RaiseStateChangeRequested(MpvConfOptionItem item, MpvOptionState state)
    {
        StateChangeRequested?.Invoke(this, new MpvConfOptionStateChangeEventArgs(item, state));
    }

    protected void RaiseValueChangeRequested(MpvConfOptionItem item, string value)
    {
        ValueChangeRequested?.Invoke(this, new MpvConfOptionValueChangeEventArgs(item, value));
    }

    public MpvConfOptionItem? Item
    {
        get => (MpvConfOptionItem?)GetValue(ItemProperty);
        set => SetValue(ItemProperty, value);
    }

    public static readonly DependencyProperty ItemProperty = DependencyProperty.Register(nameof(Item), typeof(MpvConfOptionItem), typeof(MpvConfOptionControlBase), new PropertyMetadata(null, OnItemChanged));

    private static void OnItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MpvConfOptionControlBase control)
        {
            control.RefreshValue();
        }
    }

    protected bool SuppressEdit
    {
        get;
        private set;
    }

    protected void UsingSuppressEdit(Action action)
    {
        SuppressEdit = true;
        try
        {
            action();
        }
        finally
        {
            SuppressEdit = false;
        }
    }

    private void RefreshValue()
    {
        UpdateInfo();
        UpdateEnabledState();
        UpdateModifiedState();
        UpdateOptionValue();
    }

    protected abstract void UpdateInfo();

    protected abstract void UpdateModifiedState();

    protected abstract void UpdateOptionValue();

    protected abstract void UpdateEnabledState();

    protected void SetLineToClipboard(MpvConfOptionItem item)
    {
        ClipboardHelper.SetCopyText($"{item.Key}={item.Value}");
    }
}

public sealed class MpvConfOptionItemEventArgs : EventArgs
{
    public MpvConfOptionItemEventArgs(MpvConfOptionItem item)
    {
        Item = item;
    }

    public MpvConfOptionItem Item
    {
        get;
    }
}

public sealed class MpvConfOptionStateChangeEventArgs : EventArgs
{
    public MpvConfOptionStateChangeEventArgs(MpvConfOptionItem item, MpvOptionState state)
    {
        Item = item;
        State = state;
    }

    public MpvConfOptionItem Item
    {
        get;
    }

    public MpvOptionState State
    {
        get;
    }
}

public sealed class MpvConfOptionValueChangeEventArgs : EventArgs
{
    public MpvConfOptionValueChangeEventArgs(MpvConfOptionItem item, string value)
    {
        Item = item;
        Value = value;
    }

    public MpvConfOptionItem Item
    {
        get;
    }

    public string Value
    {
        get;
    }
}
