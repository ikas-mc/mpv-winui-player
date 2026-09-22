using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using mpv_winui.Modules.Common.Utils;
using mpv_winui.Modules.FileSystem;
using NLog;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace mpv_winui.Modules.Player.History
{
    public sealed partial class WatchHistoryControl : UserControl
    {
        private string? _filePath;
        private bool _saveWatchHistoryEnabled;
        private Action<Exception>? _onException;
        private Logger? _logger;

        public ObservableCollection<WatchHistoryItem> Items { get; } = [];

        public event EventHandler<string>? ItemClick;

        public WatchHistoryControl()
        {
            this.InitializeComponent();
            Loaded += WatchHistoryControl_Loaded;
        }

        public void Initialize(string? path, bool saveWatchHistoryEnabled, Action<Exception>? onException, Logger logger)
        {
            _filePath = path;
            _saveWatchHistoryEnabled = saveWatchHistoryEnabled;
            _onException = onException;
            _logger = logger;
        }

        private void WatchHistoryControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAsync().FireAndForget(_onException);
        }

        public async Task LoadAsync()
        {
            var items = await Task.Run(() => WatchHistoryParser.Parse(_filePath));

            DispatcherQueue.RunAsync(() =>
            {
                Items.Clear();
                foreach (var item in items)
                {
                    Items.Add(item);
                }

                UpdateEmptyState();
                if (_logger?.IsDebugEnabled == true)
                {
                    _logger?.Debug("watch history loaded, count={}", items.Count);
                }
            });
        }

        private void UpdateEmptyState()
        {
            var isEmpty = Items.Count == 0;
            if (isEmpty)
            {
                HistoryListView.Visibility = Visibility.Collapsed;
                EmptyTextBlock.Visibility = Visibility.Visible;

                if (string.IsNullOrEmpty(_filePath) || !System.IO.File.Exists(_filePath))
                {
                    if (_saveWatchHistoryEnabled)
                    {
                        EmptyTextBlock.Text = "The watch history file is empty.";
                    }
                    else
                    {
                        EmptyTextBlock.Text = "Enable --save-watch-history to jump to recently played files.";
                    }
                }
                else
                {
                    EmptyTextBlock.Text = "No history entries.";
                }
            }
            else
            {
                HistoryListView.Visibility = Visibility.Visible;
                EmptyTextBlock.Visibility = Visibility.Collapsed;
            }
        }

        private void HistoryListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is WatchHistoryItem item)
            {
                ItemClick?.Invoke(this, item.Path);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadAsync().FireAndForget(_onException);
        }

        private async void OpenFileLocationButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_filePath))
            {
                return;
            }

            try
            {
                if (File.Exists(_filePath))
                {
                    await FileLauncher.ShellLaunchFileAsync(Path.GetFullPath(_filePath));
                    return;
                }

                var parent = Path.GetDirectoryName(_filePath);
                if (Directory.Exists(parent))
                {
                    var folder = await StorageFolder.GetFolderFromPathAsync(parent);
                    await FileLauncher.LaunchFolderAsync(folder);
                }
            }
            catch (Exception ex)
            {
                _onException?.Invoke(ex);
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearTeachingTip.IsOpen = true;
        }

        private async void ClearTeachingTip_ActionButtonClick(TeachingTip sender, object args)
        {
            sender.IsOpen = false;

            try
            {
                await DeleteHistoryAsync();
                await LoadAsync();
            }
            catch (Exception ex)
            {
                _onException?.Invoke(ex);
            }
        }

        private async ValueTask DeleteHistoryAsync()
        {
            if (string.IsNullOrEmpty(_filePath))
            {
                return;
            }

            await Task.Run(() =>
            {
                if (!File.Exists(_filePath))
                {
                    return;
                }

                File.Delete(_filePath);
            });
        }
    }
}
