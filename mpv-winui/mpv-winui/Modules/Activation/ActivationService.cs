using Microsoft.Windows.AppLifecycle;
using mpv_winui.Modules.Common.Utils;
using mpv_winui.Modules.Player;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Storage;

namespace mpv_winui.Modules.Activation
{
    public class ActivationService
    {
        private static readonly Lazy<ActivationService> _lazyValue = new(() => new ActivationService(), true);

        public static ActivationService Instance => _lazyValue.Value;

        private ActivationService()
        {
        }

        public async Task<IReadOnlyList<FileItem>?> ParseFileItemsAsync(AppActivationArguments activatedArgs)
        {
            switch (activatedArgs.Kind)
            {
                case ExtendedActivationKind.Launch:
                {
                    if (activatedArgs.Data is ILaunchActivatedEventArgs launchArgs)
                    {
                        string arguments = launchArgs.Arguments;
                        if (AppContext.AppLogger.IsDebugEnabled)
                        {
                            AppContext.AppLogger.Debug("app launch, Arguments={}", arguments);
                        }

                        if (!string.IsNullOrWhiteSpace(arguments))
                        {
                            var args = await Win32CommandLineParser.ParseAsync(arguments, true);
                            if (args.Count > 0 && !string.IsNullOrEmpty(args[0]))
                            {
                                var item = await ParseUriToFileItem(args[0]);
                                return item == null ? [] : (FileItem[])[item];
                            }
                        }
                    }

                    break;
                }

                case ExtendedActivationKind.File:
                {
                    if (activatedArgs.Data is Windows.ApplicationModel.Activation.IFileActivatedEventArgs fileArgs)
                    {
                        if (fileArgs.Files?.Count > 0)
                        {
                            return fileArgs.Files
                                .Where(x => !x.IsOfType(StorageItemTypes.None))
                                .Select(x => new FileItem(x.Path, x.IsOfType(StorageItemTypes.File) ? FileType.File : FileType.Folder))
                                .ToList();
                        }
                    }
                    break;
                }

                case ExtendedActivationKind.Protocol:
                {
                    if (activatedArgs.Data is Windows.ApplicationModel.Activation.IProtocolActivatedEventArgs protocolArgs)
                    {
                        if (protocolArgs.Uri.Scheme == "mpvw")
                        {
                            var query = protocolArgs.Uri.Query;
                            if (query.StartsWith("?file="))
                            {
                                var path = query[6..];
                                if (!string.IsNullOrEmpty(path))
                                {
                                    var item = await ParseUriToFileItem(path);
                                    return item == null ? [] : (FileItem[])[item];
                                }
                            }
                        }
                    }
                    break;
                }

                default:
                {
                    break;
                }
            }

            return [];
        }

        private static async Task<FileItem?> ParseUriToFileItem(string path)
        {
            return await Task.Run(() =>
            {
                if (Uri.TryCreate(path, UriKind.Absolute, out Uri? uri) && !uri.IsFile)
                {
                    return new FileItem(path, FileType.Url);
                }

                if (Directory.Exists(path))
                {
                    return new FileItem(path, FileType.Folder);
                }

                if (File.Exists(path))
                {
                    return new FileItem(path, FileType.File);
                }

                return new FileItem(path, FileType.Other);
            });
        }
    }
}
