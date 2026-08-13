using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace mpv_winui.Modules.ActivationRegistration;

public sealed class FileAssociationItem : INotifyPropertyChanged
{
    public string Extension
    {
        get;
    }

    private bool _isChecked;

    public bool IsChecked
    {
        get => _isChecked;
        set
        {
            if (_isChecked != value)
            {
                _isChecked = value;
                OnPropertyChanged();
            }
        }
    }

    private bool _isRegistered;

    public bool IsRegistered
    {
        get => _isRegistered;
        set
        {
            if (_isRegistered != value)
            {
                _isRegistered = value;
                OnPropertyChanged();
            }
        }
    }

    public FileAssociationItem(string extension)
    {
        Extension = extension;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}