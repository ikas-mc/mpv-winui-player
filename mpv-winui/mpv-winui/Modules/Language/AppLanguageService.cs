using mpv_winui.Modules.FileSystem;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Windows.Storage;

namespace mpv_winui.Modules.Language
{
    [JsonSourceGenerationOptions(
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true)
    ]
    [JsonSerializable(typeof(AppLang))]
    internal partial class AppLangJsonContext : JsonSerializerContext
    {
    }

    public class AppLanguageService
    {
        private static readonly Logger _logger = LogManager.GetLogger("AppLanguage");

        private static readonly Lazy<AppLanguageService> _lazy = new(() => new AppLanguageService(), true);
        public static AppLanguageService Instance => _lazy.Value;

        public const string LanguagesFolderName = "languages";
        public const string LanguageFileExtension = ".json";
        public const string DefaultLanguageName = "en-US";

        private static readonly AppLangJsonContext _readContext = AppLangJsonContext.Default;
        private static readonly AppLangJsonContext _writeContext = new(new JsonSerializerOptions(AppLangJsonContext.Default.Options)
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        });

        private AppLanguageService()
        {
        }

        public async Task<AppLang> LoadAsync()
        {
            var fileName = AppContext.AppSetting.CurrentLanguage;
            if (string.IsNullOrEmpty(fileName))
            {
                return new AppLang();
            }

            if (!fileName.EndsWith(LanguageFileExtension, StringComparison.OrdinalIgnoreCase))
            {
                _logger.Warn("Unsupported language file, using source text. file={}", fileName);
                return new AppLang();
            }

            try
            {
                var appLang = await Task.Run(() =>
                 {
                     var filePath = AppData.Current.ResolveLocalData(LanguagesFolderName, fileName);
                     if (File.Exists(filePath))
                     {
                         using var stream = File.OpenRead(filePath);
                         return JsonSerializer.Deserialize(stream, _readContext.AppLang);
                     }
                     return null;
                 });

                if (appLang != null)
                {
                    return appLang;
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Load language failed, using source text. file={}", fileName);
            }

            return new AppLang();
        }

        public async Task<List<LangInfo>> QueryLanguagesAsync()
        {
            var languages = new List<LangInfo>();

            {
                var language = new Windows.Globalization.Language(DefaultLanguageName);
                languages.Add(LangInfo.Create(DefaultLanguageName, string.Empty, language.DisplayName, true));
            }

            await Task.Run(() =>
            {
                var folderPath = AppData.Current.ResolveLocalData(LanguagesFolderName);
                if (!Directory.Exists(folderPath))
                {
                    return;
                }

                foreach (var filePath in Directory.GetFiles(folderPath))
                {
                    if (!filePath.EndsWith(LanguageFileExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    try
                    {
                        var name = Path.GetFileNameWithoutExtension(filePath);
                        if (!string.IsNullOrEmpty(name))
                        {
                            var language = new Windows.Globalization.Language(name);
                            languages.Add(LangInfo.Create(name, name + LanguageFileExtension, language.DisplayName, false));
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Create language info failed, path={}", filePath);
                    }
                }
            });

            return languages;
        }

        public async Task ImportAsync(StorageFile file)
        {
            var fileName = file.Name;
            if (!fileName.EndsWith(LanguageFileExtension, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException("Language file must be a json file.");
            }

            var name = Path.GetFileNameWithoutExtension(fileName);
            try
            {
                _ = new Windows.Globalization.Language(name);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Invalid language file name, path={}", file.Path);
                throw new InvalidDataException("Language file name could not be parsed.", ex);
            }

            AppLang? parsedAppLang;
            try
            {
                using var stream = await file.OpenStreamForReadAsync();
                parsedAppLang = await JsonSerializer.DeserializeAsync(stream, _readContext.AppLang);
            }
            catch (Exception e)
            {
                throw new InvalidDataException("Language file could not be parsed.", e);
            }
            if (parsedAppLang is null)
            {
                throw new InvalidDataException("Language file could not be parsed.");
            }

            await EnsureLanguagesFolderAsync();
            await Task.Run(() =>
            {
                var filePath = AppData.Current.ResolveLocalData(LanguagesFolderName, fileName);
                if (File.Exists(filePath))
                {
                    File.Move(filePath, filePath + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + ".bak", true);
                }

                using Stream writeStream = File.OpenWrite(filePath);
                JsonSerializer.Serialize(writeStream, parsedAppLang, _writeContext.AppLang);
            });
        }

        public async Task ExportAsync(Func<string, Task<StorageFile?>> pickFileAsync)
        {
            var fileName = AppContext.AppSetting.CurrentLanguage;

            AppLang appLang;
            if (string.IsNullOrEmpty(fileName))
            {
                appLang = new AppLang();
                fileName = DefaultLanguageName + LanguageFileExtension;
            }
            else
            {
                appLang = await Task.Run(() =>
                {
                    var filePath = AppData.Current.ResolveLocalData(LanguagesFolderName, fileName);
                    AppLang? parsedAppLang;
                    try
                    {
                        using var stream = File.OpenRead(filePath);
                        parsedAppLang = JsonSerializer.Deserialize(stream, _readContext.AppLang);
                    }
                    catch (Exception e)
                    {
                        throw new InvalidDataException("Language file could not be parsed.", e);
                    }
                    if (parsedAppLang is null)
                    {
                        throw new InvalidDataException("Language file could not be parsed.");
                    }

                    return parsedAppLang;
                });
            }

            var file = await pickFileAsync(fileName) ?? throw new InvalidDataException("Target file is null");

            using var writeStream = await file.OpenStreamForWriteAsync();
            writeStream.SetLength(0);
            await JsonSerializer.SerializeAsync(writeStream, appLang, _writeContext.AppLang);
        }

        public void SetCurrentLanguage(LangInfo langInfo)
        {
            if (langInfo.IsDefault)
            {
                AppContext.AppSetting.CurrentLanguage = string.Empty;
            }
            else
            {
                AppContext.AppSetting.CurrentLanguage = langInfo.FileName;
            }

            UpdatePrimaryLanguage();
        }

        public Task<StorageFolder> EnsureLanguagesFolderAsync()
        {
            return AppData.Current.OpenOrCreateLocalDataFolderAsync(LanguagesFolderName);
        }

        public int FindCurrentLanguage(List<LangInfo> languages)
        {
            var current = AppContext.AppSetting.CurrentLanguage;
            bool isDefault = string.IsNullOrEmpty(current);

            for (int i = 0; i < languages.Count; i++)
            {
                var langInfo = languages[i];
                if (isDefault && langInfo.IsDefault)
                {
                    return i;
                }

                if (string.Equals(langInfo.FileName, current, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }

        public static void UpdatePrimaryLanguage()
        {
            var fileName = AppContext.AppSetting.CurrentLanguage;
            if (string.IsNullOrEmpty(fileName))
            {
                Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = DefaultLanguageName;
            }
            else
            {
                var name = Path.GetFileNameWithoutExtension(fileName);
                Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = name;
            }
        }
    }
}
