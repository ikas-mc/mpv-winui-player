using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
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
using System.Threading.Tasks;
using WinRT;

namespace mpv_winui.Modules.MpvConf;

public sealed partial class MpvConfEditorPage : Page
{
    private static readonly Logger _logger = LogManager.GetLogger("MpvConfEditor");

    private const string DefaultProfileLabel = "default";
    private const string AllGroupsLabel = "all";
    private const string UnknownGroupLabel = "unknown";

    private MpvConfManager _manager = null!;
    private MpvConfSchema _schema = null!;
    private MpvConfOptionService _editor = null!;
    private readonly List<MpvConfOptionItem> _all = [];

    private string _profile = string.Empty;
    private string? _group;
    private MpvConfOptionIncludeType _mode = MpvConfOptionIncludeType.All;
    private bool _suppressSelection;

    public MpvConfEditorPage()
    {
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is not string configPath || configPath.Length == 0)
        {
            return;
        }

        _manager = new MpvConfManager(configPath);
        try
        {
            if (_logger.IsDebugEnabled)
            {
                _logger.Debug("load conf file, conf={}", configPath);
            }
            await Task.Run(_manager.Load);
        }
        catch (Exception ex)
        {
            ShowMessage($"failed to load conf file : {ex.Message}");
            _logger.Error(ex, "failed to load conf file");
        }

        try
        {
            _schema = await Task.Run(() =>
            {
                var definitionDirectory = AppData.Current.ResolveLocalData(MpvConfSchemaService.DefinitionDirectoryName);
                return MpvConfSchemaService.LoadFromDirectory(definitionDirectory);
            });
        }
        catch (Exception ex)
        {
            _schema = MpvConfSchema.Empty;
            ShowMessage($"failed to load data : {ex.Message}");
            _logger.Error(ex, "failed to load data");
        }

        _editor = new MpvConfOptionService(_manager, _schema);

        SaveButton.IsEnabled = true;
        ReloadButton.IsEnabled = true;

        BuildProfiles();
        BuildGroups();
        RebuildItems();
    }

    public string ConfigPath => _manager.FilePath;

    public ObservableCollection<MpvConfProfileItem> Profiles
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
        Profiles.Add(new MpvConfProfileItem(DefaultProfileLabel));

        string? profile = _profile;
        var selectedIndex = 0;
        var index = 0;
        foreach (string section in _manager.Sections)
        {
            index++;
            Profiles.Add(new MpvConfProfileItem(section, _manager.IsSectionDeleted(section)));
            if (string.Equals(profile, section, StringComparison.OrdinalIgnoreCase))
            {
                selectedIndex = index;
                profile = section;
            }
        }

        _profile = selectedIndex > 0 ? profile : string.Empty;
        _suppressSelection = true;
        ProfileList.SelectedIndex = selectedIndex;
        _suppressSelection = false;
    }

    private void BuildGroups()
    {
        string? currentGroup = _group;
        string currentLabel = GroupLabel(currentGroup);

        Groups.Clear();
        Groups.Add(AllGroupsLabel);
        foreach (string group in _editor.GetGroups(_profile, _mode))
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
        _all.AddRange(_editor.GetOptions(_profile, _group, _mode));
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

    private void OnOptionApplyRequested(object? sender, MpvConfOptionItemEventArgs e)
    {
        ApplyToPlayer(e.Item);
    }

    private void OnOptionStateChangeRequested(object? sender, MpvConfOptionStateChangeEventArgs e)
    {
        _editor.SetState(e.Item, e.State);
    }

    private void OnOptionValueChangeRequested(object? sender, MpvConfOptionValueChangeEventArgs e)
    {
        _editor.SetValue(e.Item, e.Value);
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
        if (_suppressSelection || ProfileList.SelectedItem is not MpvConfProfileItem profile)
        {
            return;
        }
        _profile = profile.Name == DefaultProfileLabel ? string.Empty : profile.Name;
        BuildGroups();
        RebuildItems();
    }

    private void ProfileList_ContextRequested(UIElement sender, ContextRequestedEventArgs args)
    {
        if (args.OriginalSource is FrameworkElement fe && fe.DataContext is MpvConfProfileItem profile)
        {
            ProfileList.SelectedItem = profile;

            bool isDefault = profile.Name == DefaultProfileLabel;
            bool isDeleted = profile.IsDeleted;

            if (Resources["ProfileContextMenu"] is MenuFlyout flyout)
            {
                foreach (var flyoutItem in flyout.Items)
                {
                    if (flyoutItem is MenuFlyoutItem menuItem && menuItem.Tag is string tag)
                    {
                        menuItem.IsEnabled = tag switch
                        {
                            "rename" or "delete" => !isDefault && !isDeleted,
                            "restore" => isDeleted,
                            _ => true,
                        };
                    }
                }

                if (args.TryGetPosition(ProfileList, out var point))
                {
                    flyout.ShowAt(ProfileList, point);
                }
                else
                {
                    flyout.ShowAt(ProfileList);
                }

                args.Handled = true;
            }
        }
    }

    private async void ProfileMenu_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem { Tag: string action } || ProfileList.SelectedItem is not MpvConfProfileItem profile)
        {
            return;
        }

        switch (action)
        {
            case "rename":
                await RenameProfileAsync(profile);
                break;
            case "delete":
                await DeleteProfileAsync(profile);
                break;
            case "restore":
                RestoreProfile(profile);
                break;
        }
    }

    private async Task RenameProfileAsync(MpvConfProfileItem profile)
    {
        string oldName = profile.Name;

        var nameBox = new TextBox { Text = oldName, Header = "New name" };
        var dialog = new ContentDialog
        {
            Title = $"Rename profile '{oldName}'",
            Content = nameBox,
            PrimaryButtonText = "Rename",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = XamlRoot
        };

        if (await dialog.ShowAsync() != ContentDialogResult.Primary)
        {
            return;
        }

        string newName = nameBox.Text.Trim();
        if (!_manager.RenameSection(oldName, newName))
        {
            ShowMessage($"Cannot rename '{oldName}' to '{newName}': name is empty, reserved ('{MpvConfManager.DefaultSectionName}'), duplicated, or unchanged.");
            return;
        }

        profile.Name = newName;
        if (_profile == oldName)
        {
            _profile = newName;
            RebuildItems();
        }

        ShowMessage($"Renamed '{oldName}' to '{newName}'. Save to persist.");
    }

    private async Task DeleteProfileAsync(MpvConfProfileItem profile)
    {
        string label = profile.Name;

        var dialog = new ContentDialog
        {
            Title = $"Delete profile '{label}'",
            Content = "All options in this profile will be removed when the file is saved.",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = XamlRoot
        };

        if (await dialog.ShowAsync() != ContentDialogResult.Primary)
        {
            return;
        }

        bool changed = _manager.RemoveSection(label);

        if (_logger.IsDebugEnabled)
        {
            _logger.Debug("profile deleted, profile={}, changed={}", label, changed);
        }

        profile.IsDeleted = true;
        ShowMessage($"Deleted profile '{label}'. Save to persist, Reload to undo.");
    }

    private void RestoreProfile(MpvConfProfileItem profile)
    {
        bool changed = _manager.RestoreSection(profile.Name);

        if (_logger.IsDebugEnabled)
        {
            _logger.Debug("profile restored, profile={}, changed={}", profile.Name, changed);
        }

        profile.IsDeleted = false;
        ShowMessage($"Restored profile '{profile.Name}'.");
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

            if (profile.Length > 0 && _manager.IsSectionDeleted(profile))
            {
                ShowMessage($"Profile '{profile}' was deleted but not saved yet. Save or reload before reusing the name.");
                return;
            }

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

                var newItem = new MpvConfProfileItem(profile);
                Profiles.Add(newItem);
                _profile = profile;
                _suppressSelection = true;
                ProfileList.SelectedItem = newItem;
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

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            SaveButton.IsEnabled = false;
            try
            {
                await _manager.SaveAsync(AppContext.AppSetting.EnableSaveBackup);

                BuildProfiles();
                BuildGroups();
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
        finally
        {
            SaveButton.IsEnabled = true;
        }
    }

    private async void ReloadButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            ReloadButton.IsEnabled = false;

            try
            {
                await Task.Run(_manager.Load);
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
        finally
        {
            ReloadButton.IsEnabled = true;
        }
    }

    private void ShowMessage(string message)
    {
        MessageBar.Text = message;
    }

    private void SourceFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        //TODO fix aot, use index??
        var seletedItem = e.AddedItems.FirstOrDefault().As<ComboBoxItem>();
        if (seletedItem?.Tag is string tag)
        {
            var mode = tag switch
            {
                "FromConfFile" => MpvConfOptionIncludeType.FromConfFile,
                "Enabled" => MpvConfOptionIncludeType.Enabled,
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
