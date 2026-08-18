using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using mpv_winui.Modules.FileSystem;
using mpv_winui.Modules.MpvConf.Conf;
using mpv_winui.Modules.MpvConf.Option;
using mpv_winui.Modules.MpvConf.Schema;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace mpv_winui.Modules.MpvConf;

public sealed partial class MpvConfEditorPage : Page
{
    private static readonly Logger _logger = LogManager.GetLogger("MpvConfEditor");

    private const string DefaultProfileLabel = "default";
    private const string AllGroupsLabel = "all";
    private const string UnknownGroupLabel = "unknown";

    private MpvConfManager _manager = null!;
    private MpvConfSchema _schema = null!;
    private readonly List<MpvConfOptionItem> _all = [];

    private string _profile = string.Empty;
    private string? _group;
    private MpvConfOptionIncludeType _mode = MpvConfOptionIncludeType.All;
    private bool _suppressSelection;

    public MpvConfEditorPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is not string configPath || configPath.Length == 0)
        {
            return;
        }

        try
        {
            if (_logger.IsDebugEnabled)
            {
                _logger.Debug("load conf file, conf={}", configPath);
            }
            _manager = new MpvConfManager(configPath);
            _manager.Load();
        }
        catch (Exception ex)
        {
            ShowMessage($"failed to load conf file : {ex.Message}");
            _logger.Error(ex, "failed to load conf file");
        }

        try
        {
            string definitionDirectory = AppData.Current.ResolveLocalData(MpvConfSchemaService.DefinitionDirectoryName);
            _schema = MpvConfSchemaService.LoadFromDirectory(definitionDirectory);
        }
        catch (Exception ex)
        {
            _schema = MpvConfSchema.Empty;
            ShowMessage($"failed to load data : {ex.Message}");
            _logger.Error(ex, "failed to load data");
        }

        BuildProfiles();
        BuildGroups();
        RebuildItems();
    }

    public string ConfigPath => _manager.FilePath;

    public ObservableCollection<string> Profiles
    {
        get;
    } = [];

    public ObservableCollection<string> Groups
    {
        get;
    } = [];

    public ObservableCollection<MpvConfOptionItem> Items
    {
        get;
    } = [];

    private void BuildProfiles()
    {
        Profiles.Clear();
        Profiles.Add(DefaultProfileLabel);
        foreach (string section in _manager.Sections)
        {
            Profiles.Add(section);
        }

        _profile = string.Empty;
        _suppressSelection = true;
        ProfileList.SelectedIndex = 0;
        _suppressSelection = false;
    }

    private void BuildGroups()
    {
        string? currentGroup = _group;
        string currentLabel = GroupLabel(currentGroup);

        Groups.Clear();
        Groups.Add(AllGroupsLabel);
        foreach (string group in MpvConfOptionService.GetGroups(_manager, _schema, _profile, _mode))
        {
            Groups.Add(GroupLabel(group));
        }

        int index = Groups.IndexOf(currentLabel);
        if (index < 0)
        {
            index = 0;
        }

        _suppressSelection = true;
        GroupList.SelectedIndex = index;
        _suppressSelection = false;
        _group = GroupValue(Groups[index]);
    }

    private static string GroupLabel(string? group) => group == MpvConfOptionService.UnknownGroup ? UnknownGroupLabel : group ?? AllGroupsLabel;

    private static string? GroupValue(string label) =>
        label switch
        {
            AllGroupsLabel => null,
            UnknownGroupLabel => MpvConfOptionService.UnknownGroup,
            _ => label,
        };

    private void RebuildItems()
    {
        _all.Clear();
        _all.AddRange(MpvConfOptionService.GetOptions(_manager, _schema, _profile, _group, _mode));
        ApplySearch();
    }

    private void ApplySearch()
    {
        string query = SearchBox.Text.Trim();
        Items.Clear();
        foreach (MpvConfOptionItem item in _all)
        {
            if (query.Length == 0 || item.Key.Contains(query, StringComparison.OrdinalIgnoreCase))
            {
                Items.Add(item);
            }
        }
    }

    private void OnContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
    {
        if (args.InRecycleQueue)
        {
            return;
        }

        if (args.Item is not MpvConfOptionItem item)
        {
            return;
        }

        if (args.ItemContainer.ContentTemplateRoot is MpvConfOptionControlBase control)
        {
            control.Item = item;
        }

        args.Handled = true;
    }

    private void OnOptionApplyRequested(object? sender, EventArgs e)
    {
        if (sender is MpvConfOptionControl { Item: MpvConfOptionItem item })
        {
            ApplyToPlayer(item);
        }
    }

    private void ApplyToPlayer(MpvConfOptionItem item)
    {
        if (item.Profile.Length > 0)
        {
            ShowMessage("Options in a named profile cannot be applied to the player individually.");
            return;
        }

        string value = item.Value;
        if (string.IsNullOrEmpty(value))
        {
            ShowMessage($"No value to apply for '{item.Key}'.");
            return;
        }

        try
        {
            if (App.Window is MainWindow window)
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug("option applied to player, key={}, value={}", item.Key, value);
                }

                window.ApplyMpvOption(item.Key, value);
                ShowMessage($"Applied {item.Key}={value} to player.");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "apply option to player failed, key={}, value={}", item.Key, value);
            ShowMessage($"Apply failed: {ex.Message}");
        }
    }

    private void ProfileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressSelection || ProfileList.SelectedItem is not string label)
        {
            return;
        }

        _profile = label == DefaultProfileLabel ? string.Empty : label;
        BuildGroups();
        RebuildItems();
    }

    private void GroupList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressSelection || GroupList.SelectedItem is not string label)
        {
            return;
        }

        _group = GroupValue(label);
        RebuildItems();
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ApplySearch();
    }

    private async void AddButton_Click(object sender, RoutedEventArgs e)
    {
        string currentLabel = _profile.Length == 0 ? DefaultProfileLabel : _profile;
        var profileBox = new TextBox { Text = currentLabel, PlaceholderText = "profile name (new names create a profile)", Header = "Profile" };
        var keyBox = new TextBox { PlaceholderText = "option name (optional for a new profile)", Header = "Key" };
        var valueBox = new TextBox { PlaceholderText = "value (optional)", Header = "Value" };
        var panel = new StackPanel { Spacing = 8 };
        panel.Children.Add(profileBox);
        panel.Children.Add(keyBox);
        panel.Children.Add(valueBox);

        var dialog = new ContentDialog
        {
            Title = "Add option",
            Content = panel,
            PrimaryButtonText = "Add",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = XamlRoot
        };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            string profile = profileBox.Text.Trim();
            if (string.Equals(profile, DefaultProfileLabel, StringComparison.Ordinal))
            {
                profile = string.Empty;
            }

            string key = keyBox.Text.Trim();
            bool isNewProfile = profile.Length > 0 && !_manager.ContainsSection(profile);

            if (key.Length > 0 && !MpvConfParser.IsValidOptionKey(key))
            {
                ShowMessage($"Invalid option key: '{key}'");
                return;
            }

            if (isNewProfile)
            {
                if (key.Length > 0)
                {
                    _manager.InsertOption(key, valueBox.Text, profile);
                }
                else
                {
                    _manager.InsertSection(profile);
                }

                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug("new profile created, profile={}, key={}", profile, key);
                }

                BuildProfiles();
                _profile = profile;
                _suppressSelection = true;
                ProfileList.SelectedIndex = Profiles.IndexOf(profile);
                _suppressSelection = false;
                BuildGroups();
                RebuildItems();
                return;
            }

            if (key.Length == 0)
            {
                return;
            }

            _manager.InsertOption(key, valueBox.Text, profile);
            RebuildItems();
        }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _manager.Save();
            RebuildItems();
            ShowMessage($"Saved to {_manager.FilePath}");

            if (_logger.IsDebugEnabled)
            {
                _logger.Debug("config saved, path={}", _manager.FilePath);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "config save failed, path={}", _manager.FilePath);
            ShowMessage($"Save failed: {ex.Message}");
        }
    }

    private void ReloadButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _manager.Load();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "config reload failed, path={}", _manager.FilePath);
            ShowMessage("Reload failed: " + ex.Message);
            return;
        }

        BuildProfiles();
        BuildGroups();
        RebuildItems();
        ShowMessage("Reloaded from disk");
    }

    private void ShowMessage(string message)
    {
        MessageBar.Text = message;
    }

    private void SourceFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.FirstOrDefault() is ComboBoxItem item && item.Tag is string tag)
        {
            var mode = tag switch
            {
                "FromConfig" => MpvConfOptionIncludeType.FromConfig,
                "Enabled" => MpvConfOptionIncludeType.Effective,
                "Modified" => MpvConfOptionIncludeType.Modified,
                _ => MpvConfOptionIncludeType.All,
            };

            if (mode != _mode)
            {
                _mode = mode;
                BuildGroups();
                RebuildItems();
            }
        }
    }
}
