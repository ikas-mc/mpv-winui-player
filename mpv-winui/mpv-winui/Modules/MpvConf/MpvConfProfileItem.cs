using System.ComponentModel;
using Windows.UI.Text;

namespace mpv_winui.Modules.MpvConf;

public sealed partial class MpvConfProfileItem(string name, bool isDeleted = false) : INotifyPropertyChanged
{
    private bool _isDeleted = isDeleted;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Name
    {
        get;
        internal set;
    } = name;

    public bool IsDeleted
    {
        get => _isDeleted;
        internal set
        {
            if (_isDeleted != value)
            {
                _isDeleted = value;
                OnPropertyChanged(nameof(IsDeleted));
                OnPropertyChanged(nameof(Decorations));
            }
        }
    }

    public TextDecorations Decorations => _isDeleted ? TextDecorations.Strikethrough : TextDecorations.None;

    private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
