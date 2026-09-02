using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace mpv_winui.Modules.Menu.MenuEditor;

public enum MenuTreeItemKind
{
    CmdMenu,
    Menu,
    Separator,
}

public sealed class MenuTreeItem : INotifyPropertyChanged
{
    private string? _name = string.Empty;
    private string _command = string.Empty;
    private string? _hidden;
    private string? _disabled;
    private string? _checked;
    private bool _isSeparator;
    private MenuTreeItemKind _kind = MenuTreeItemKind.Menu;

    public MenuTreeItem()
    {
        Children = [];
    }

    public MenuTreeItem? Parent
    {
        get; set;
    }

    public string Name
    {
        get => _name ?? string.Empty;
        set
        {
            if (SetField(ref _name, value))
            {
                Notify(nameof(DisplayName));
            }
        }
    }

    public string DisplayName => IsSeparator ? "--" : Name;

    public string Command
    {
        get => _command;
        set
        {
            if (_command == value)
            {
                return;
            }

            _command = value;
            Notify(nameof(Command));
            Notify(nameof(HasCommand));
        }
    }

    public bool HasCommand => _command.Length > 0;

    public string? Hidden
    {
        get => _hidden;
        set => SetField(ref _hidden, value);
    }

    public string? Disabled
    {
        get => _disabled;
        set => SetField(ref _disabled, value);
    }

    public string? Checked
    {
        get => _checked;
        set => SetField(ref _checked, value);
    }

    public bool IsSeparator
    {
        get => _isSeparator;
        set
        {
            if (_isSeparator == value)
            {
                return;
            }

            _isSeparator = value;
            Notify(nameof(IsSeparator));
            Notify(nameof(DisplayName));
        }
    }

    public MenuTreeItemKind Kind
    {
        get => _kind;
        set => SetField(ref _kind, value);
    }

    public ObservableCollection<MenuTreeItem> Children
    {
        get;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        Notify(name);
        return true;
    }

    private void Notify(string? name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
