using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppLifecycle;
using mpv_winui.Modules.FileSystem;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace mpv_winui.Modules.AppConfData;

public sealed partial class AppConfDataImportDialogControl : UserControl
{
    private static readonly Logger _logger = LogManager.GetLogger("AppConfData");

    public ObservableCollection<AppConfDataNode> Nodes { get; } = [];

    private string? _archivePath;

    public AppConfDataImportDialogControl()
    {
        InitializeComponent();
    }

    private async void ImportButton_Click(object sender, RoutedEventArgs e)
    {
        _archivePath = null;

        try
        {
            var file = await FilePickerHelper.PickSingleFileAsync(picker =>
            {
                picker.FileTypeFilter.Add(".zip");
            });
            if (file is null)
            {
                return;
            }

            await LoadArchiveAsync(file.Path);
            _archivePath = file.Path;
            SelectAll();
        }
        catch (Exception ex)
        {
            SetStatus($"Failed to read file: {ex.Message}");
            _logger.Error(ex, "Failed to read file");
        }
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(_archivePath))
        {
            if (!ConfDataTree.SelectedNodes.IsReadOnly)
            {
                ConfDataTree.SelectedNodes.Clear();
            }

            await LoadArchiveAsync(_archivePath);
        }
    }

    private async Task LoadArchiveAsync(string archivePath)
    {
        ImportButton.IsEnabled = false;
        RefreshButton.IsEnabled = false;
        ApplyButton.IsEnabled = false;
        SetBusy(true);

        try
        {
            var roots = await AppConfDataImportService.Instance.ReadArchiveAsync(archivePath);

            Nodes.Clear();
            foreach (var root in roots)
            {
                Nodes.Add(root);
            }

            SetStatus($"Loaded: {archivePath}");
        }
        catch (Exception ex)
        {
            Nodes.Clear();
            SetStatus($"Failed to read archive: {ex.Message}");
            _logger.Error(ex, "Failed to read archive");
        }
        finally
        {
            ImportButton.IsEnabled = true;
            RefreshButton.IsEnabled = true;
            ApplyButton.IsEnabled = true;
            SetBusy(false);
        }
    }

    private async void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        SetStatus(string.Empty);
        if (string.IsNullOrEmpty(_archivePath))
        {
            return;
        }

        var selectedPaths = CollectSelectedPaths();
        if (selectedPaths.Count == 0)
        {
            SetStatus("No files selected.");
            return;
        }

        ApplyButton.IsEnabled = false;
        SetBusy(true);
        try
        {
            var overwrite = ForceOverwriteCheckBox.IsChecked == true;
            var count = await AppConfDataImportService.Instance.ImportAsync(_archivePath, selectedPaths, overwrite);
            await Task.Delay(1000);
            SetStatus($"Imported {count} file(s). Restart to take effect");
        }
        catch (Exception ex)
        {
            SetStatus($"Import failed: {ex.Message}");
            _logger.Error(ex, "Failed Import");
        }
        finally
        {
            ApplyButton.IsEnabled = true;
            SetBusy(false);
        }
    }

    private void SetBusy(bool busy)
    {
        ImportProgressRing.IsActive = busy;
        ImportProgressRing.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
    }

    private void SetStatus(string message)
    {
        StatusText.Text = message;
    }

    private List<string> CollectSelectedPaths()
    {
        var paths = new List<string>();
        foreach (var item in ConfDataTree.SelectedItems)
        {
            if (item is AppConfDataNode node)
            {
                if (node.IsDirectory)
                {
                    CollectFilePaths(node, paths);
                }
                else
                {
                    paths.Add(node.RelativePath);
                }
            }
        }

        return paths.Distinct().ToList();
    }

    private void CollectFilePaths(AppConfDataNode node, List<string> paths)
    {
        if (!node.IsDirectory)
        {
            paths.Add(node.RelativePath);
            return;
        }

        foreach (var child in node.Children)
        {
            CollectFilePaths(child, paths);
        }
    }

    private void ToggleSelectButton_Click(object sender, RoutedEventArgs e)
    {
        if (ConfDataTree.SelectedNodes.Count == 0)
        {
            SelectAll();
        }
        else
        {
            UnSelectAll();
        }
    }

    private void SelectAll()
    {
        var allNodes = new List<TreeViewNode>();
        CollectAllNodes(ConfDataTree.RootNodes, allNodes);
        foreach (var node in allNodes)
        {
            if (!ConfDataTree.SelectedNodes.IsReadOnly)
            {
                ConfDataTree.SelectedNodes.Add(node);
            }
        }
    }

    private void UnSelectAll()
    {
        if (!ConfDataTree.SelectedNodes.IsReadOnly)
        {
            ConfDataTree.SelectedNodes.Clear();
        }
    }

    private void CollectAllNodes(IEnumerable<TreeViewNode> nodes, List<TreeViewNode> all)
    {
        foreach (var node in nodes)
        {
            all.Add(node);
            CollectAllNodes(node.Children, all);
        }
    }

    private async void OpenTargetFolderButton_Click(object sender, RoutedEventArgs e)
    {
        SetStatus(string.Empty);
        try
        {
            var folder = await AppConfDataImportService.Instance.GetTargetFolderAsync();
            await FileLauncher.LaunchFolderAsync(folder);
        }
        catch (Exception ex)
        {
            SetStatus(ex.Message);
            _logger.Error(ex, "Failed to Get Target Folder");
        }
    }

    private void RestartButton_Click(object sender, RoutedEventArgs e)
    {
        //TODO
        if (App.Window is MainWindow mainWindow)
        {
            mainWindow.SaveWindowPositionAndSize();
        }
        AppInstance.Restart(string.Empty);
    }
}