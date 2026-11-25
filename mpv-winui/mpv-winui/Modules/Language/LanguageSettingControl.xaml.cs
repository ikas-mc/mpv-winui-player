using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppLifecycle;
using mpv_winui.Modules.Common.Utils;
using mpv_winui.Modules.FileSystem;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;

namespace mpv_winui.Modules.Language
{
    public sealed partial class LanguageSettingControl : UserControl
    {
        private bool _isLoadingLanguages;

        public LanguageSettingControl()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            LoadLanguagesAsync().FireAndForget(OnException);
        }

        private async Task LoadLanguagesAsync()
        {
            var languages = await AppLanguageService.Instance.QueryLanguagesAsync();

            _isLoadingLanguages = true;

            try
            {
                LanguageComboBox.ItemsSource = languages;
                LanguageComboBox.SelectedIndex = AppLanguageService.Instance.FindCurrentLanguage(languages);
            }
            finally
            {
                _isLoadingLanguages = false;
            }
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoadingLanguages)
            {
                return;
            }

            if (LanguageComboBox.SelectedItem is LangInfo langInfo)
            {
                try
                {
                    AppLanguageService.Instance.SetCurrentLanguage(langInfo);
                }
                catch (Exception ex)
                {
                    OnException(ex);
                }
            }
        }

        private async void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = string.Empty;

            StorageFile? file = await FilePickerHelper.PickSingleFileAsync(picker =>
            {
                picker.FileTypeFilter.Add(AppLanguageService.LanguageFileExtension);
            });

            if (file is null)
            {
                return;
            }

            try
            {
                await AppLanguageService.Instance.ImportAsync(file);
                await LoadLanguagesAsync();
            }
            catch (Exception ex)
            {
                OnException(ex);
            }
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = string.Empty;

            try
            {
                await AppLanguageService.Instance.ExportAsync(PickExportFileAsync);
            }
            catch (Exception ex)
            {
                OnException(ex);
            }
        }

        private Task<StorageFile?> PickExportFileAsync(string fileName)
        {
            return FilePickerHelper.PickSaveFileAsync(picker =>
            {
                picker.SuggestedFileName = fileName;
                picker.FileTypeChoices.Add(AppContext.AppLang.LanguageFileType, new List<string> { AppLanguageService.LanguageFileExtension });
            });
        }

        private void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = string.Empty;

            LoadLanguagesAsync().FireAndForget(OnException);
        }

        private async void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = string.Empty;

            try
            {
                StorageFolder folder = await AppLanguageService.Instance.EnsureLanguagesFolderAsync();
                await Launcher.LaunchFolderPathAsync(folder.Path);
            }
            catch (Exception ex)
            {
                OnException(ex);
            }
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.Window is MainWindow mainWindow)
            {
                mainWindow.SaveWindowPositionAndSize();
            }

            AppInstance.Restart(string.Empty);
        }

        private void OnException(Exception e)
        {
            DispatcherQueue.RunAsync(() =>
            {
                StatusText.Text = e.Message;
            });
        }
    }
}
