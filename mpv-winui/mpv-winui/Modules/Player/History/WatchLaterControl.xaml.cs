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
    public sealed partial class WatchLaterControl : UserControl
    {
        private string? _directoryPath;
        private Action<Exception>? _onException;
        private Logger? _logger;

        public ObservableCollection<WatchLaterItem> Items { get; } = [];

        public event EventHandler<string>? ItemClick;

        public WatchLaterControl()
        {
            this.InitializeComponent();
            Loaded += WatchLaterControl_Loaded;
        }

        public void Initialize(string? directory, Action<Exception>? onException, Logger logger)
        {
            _directoryPath = directory;
            _onException = onException;
            _logger = logger;
        }

        private void WatchLaterControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAsync().FireAndForget(_onException);
        }

        public async Task LoadAsync()
        {
            var items = await Task.Run(() => WatchLaterParser.Parse(_directoryPath));

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
                    _logger?.Debug("watch later loaded, count={}", items.Count);
                }
            });
        }

        private void UpdateEmptyState()
        {
            var isEmpty = Items.Count == 0;
            if (isEmpty)
            {
                WatchLaterListView.Visibility = Visibility.Collapsed;
                EmptyTextBlock.Visibility = Visibility.Visible;

                if (string.IsNullOrEmpty(_directoryPath) || !Directory.Exists(_directoryPath))
                {
                    EmptyTextBlock.Text = "No watch later files found.";
                }
                else
                {
                    EmptyTextBlock.Text = "Enable --write-filename-in-watch-later-config to select recent files.";
                }
            }
            else
            {
                WatchLaterListView.Visibility = Visibility.Visible;
                EmptyTextBlock.Visibility = Visibility.Collapsed;
            }
        }

        private void WatchLaterListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is WatchLaterItem item)
            {
                ItemClick?.Invoke(this, item.Path);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadAsync().FireAndForget(_onException);
        }

        private async void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_directoryPath))
            {
                return;
            }

            try
            {
                var path = Path.GetFullPath(_directoryPath);
                if (!Directory.Exists(path))
                {
                    return;
                }

                var folder = await StorageFolder.GetFolderFromPathAsync(path);
                await FileLauncher.LaunchFolderAsync(folder);
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
                await DeleteWatchLaterAsync();
                await LoadAsync();
            }
            catch (Exception ex)
            {
                _onException?.Invoke(ex);
            }
        }

        private async ValueTask DeleteWatchLaterAsync()
        {
            if (string.IsNullOrEmpty(_directoryPath))
            {
                return;
            }

            await Task.Run(() =>
            {
                if (!Directory.Exists(_directoryPath))
                {
                    return;
                }

                foreach (var file in Directory.EnumerateFiles(_directoryPath))
                {
                    var name = Path.GetFileName(file);
                    if (string.IsNullOrEmpty(name) || name.Length != 32 || name.Contains('.'))
                    {
                        continue;
                    }

                    File.Delete(file);
                }
            });
        }
    }
}
