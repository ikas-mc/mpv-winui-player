using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace mpv_winui.Modules.ActivationRegistration;

public sealed partial class FileAssociationControl : UserControl
{
    private static readonly Logger _logger = LogManager.GetLogger("FileAssociation");

private static readonly string[] VideoExtensions =
    [
        ".3g2", ".3gp", ".amv", ".asf", ".avi", ".dav", ".f4v", ".flv", ".ivf", ".m4v",
        ".mj2", ".mkv", ".mov", ".mp2", ".mp4", ".mp4v", ".mpeg", ".mpg", ".mts", ".rm",
        ".rmvb", ".ts", ".vob", ".webm", ".wmv",
    ];

    private static readonly string[] AudioExtensions =
    [
        ".aac", ".ac3", ".aiff", ".ape", ".au", ".dts", ".eac3", ".flac", ".m4a", ".mka",
        ".mp3", ".oga", ".ogg", ".ogm", ".opus", ".thd", ".wav", ".wma", ".wv", ".avif",
    ];

    private static readonly string[] ImageExtensions =
    [
        ".bmp", ".gif", ".heic", ".heif", ".j2k", ".jp2", ".jpeg", ".jpg", ".jxl", ".png",
        ".qoi", ".svg", ".tga", ".tif", ".tiff", ".webp",
    ];

    private static readonly string[] ArchiveExtensions =
    [
        ".7z", ".cbr", ".cbz", ".gz", ".rar", ".tar", ".zip",
    ];

    private static readonly string[] PlaylistExtensions =
    [
        ".cue", ".edl", ".m3u", ".m3u8", ".pls",
    ];

    private readonly ObservableCollection<FileAssociationItem> _videoItems = [];
    private readonly ObservableCollection<FileAssociationItem> _audioItems = [];
    private readonly ObservableCollection<FileAssociationItem> _imageItems = [];
    private readonly ObservableCollection<FileAssociationItem> _archiveItems = [];
    private readonly ObservableCollection<FileAssociationItem> _playlistItems = [];

    public FileAssociationControl()
    {
        InitializeComponent();

        InitializeItems(_videoItems, VideoExtensions);
        InitializeItems(_audioItems, AudioExtensions);
        InitializeItems(_imageItems, ImageExtensions);
        InitializeItems(_archiveItems, ArchiveExtensions);
        InitializeItems(_playlistItems, PlaylistExtensions);

        VideoList.ItemsSource = _videoItems;
        AudioList.ItemsSource = _audioItems;
        ImageList.ItemsSource = _imageItems;
        ArchiveList.ItemsSource = _archiveItems;
        PlaylistList.ItemsSource = _playlistItems;

        LoadRegistrationState();
    }

    private async void LoadRegistrationState()
    {
        try
        {
            var registered = await ActivationRegistrationService.Instance.GetRegisteredExtensionsAsync();
            var registeredSet = registered.ToHashSet();

            foreach (var items in AllGroups())
            {
                foreach (var item in items)
                {
                    var isRegistered = registeredSet.Contains(item.Extension);
                    item.IsChecked = isRegistered;
                    item.IsRegistered = isRegistered;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Load registered file types failed");
        }
    }

    private void InitializeItems(ObservableCollection<FileAssociationItem> target, IEnumerable<string> extensions)
    {
        foreach (var extension in extensions)
        {
            target.Add(new FileAssociationItem(extension) { IsChecked = true });
        }
    }

    private IEnumerable<ObservableCollection<FileAssociationItem>> AllGroups()
    {
        yield return _videoItems;
        yield return _audioItems;
        yield return _imageItems;
        yield return _archiveItems;
        yield return _playlistItems;
    }

    private void OnSelectAll(object sender, RoutedEventArgs e)
    {
        foreach (var items in AllGroups())
        {
            foreach (var item in items)
            {
                item.IsChecked = true;
            }
        }
    }

    private void OnDeselectAll(object sender, RoutedEventArgs e)
    {
        foreach (var items in AllGroups())
        {
            foreach (var item in items)
            {
                item.IsChecked = false;
            }
        }
    }

    private void OnInvert(object sender, RoutedEventArgs e)
    {
        foreach (var items in AllGroups())
        {
            foreach (var item in items)
            {
                item.IsChecked = !item.IsChecked;
            }
        }
    }

    private async void OnRegister(object sender, RoutedEventArgs e)
    {
        var selected = AllGroups().SelectMany(items => items).Where(item => item.IsChecked).Select(item => item.Extension).ToArray();
        if (selected.Length == 0)
        {
            StatusText.Text = "Please check the extensions to register.";
            return;
        }

        StatusText.Text = string.Empty;

        try
        {
            var registered = await ActivationRegistrationService.Instance.GetRegisteredExtensionsAsync();
            if (registered.Count > 0)
            {
                await ActivationRegistrationService.Instance.UnregisterAsync(registered);
            }

            await ActivationRegistrationService.Instance.RegisterAsync(selected);

            var selectedSet = selected.ToHashSet();
            foreach (var items in AllGroups())
            {
                foreach (var item in items)
                {
                    var isRegistered = selectedSet.Contains(item.Extension);
                    item.IsChecked = isRegistered;
                    item.IsRegistered = isRegistered;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Register file types failed");
            StatusText.Text = $"Register failed: {ex.Message}";
        }
    }

    private async void OnUnregisterAll(object sender, RoutedEventArgs e)
    {
        StatusText.Text = string.Empty;

        try
        {
            var registered = await ActivationRegistrationService.Instance.GetRegisteredExtensionsAsync();
            if (registered.Count > 0)
            {
                await ActivationRegistrationService.Instance.UnregisterAsync(registered);
            }

            foreach (var items in AllGroups())
            {
                foreach (var item in items)
                {
                    item.IsChecked = false;
                    item.IsRegistered = false;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Unregister file types failed");
            StatusText.Text = $"Unregister failed: {ex.Message}";
        }
    }
}