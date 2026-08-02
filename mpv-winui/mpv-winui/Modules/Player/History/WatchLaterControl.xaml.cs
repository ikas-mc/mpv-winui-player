using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using mpv_winui.Modules.Common.Utils;
using NLog;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace mpv_winui.Modules.Player.History
{
    public sealed partial class WatchLaterControl : UserControl
    {
        private string? _directory;
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
            _directory = directory;
            _onException = onException;
            _logger = logger;
        }

        private void WatchLaterControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAsync().FireAndForget(_onException);
        }

        public async Task LoadAsync()
        {
            var items = await Task.Run(() => WatchLaterParser.Parse(_directory));

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

                if (string.IsNullOrEmpty(_directory) || !System.IO.Directory.Exists(_directory))
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
    }
}
