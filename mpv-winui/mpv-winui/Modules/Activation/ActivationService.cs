using Microsoft.Windows.AppLifecycle;
using mpv_winui.Modules.Player;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

        public IReadOnlyList<string>? Parse(AppActivationArguments activatedArgs)
        {
            switch (activatedArgs.Kind)
            {
                case ExtendedActivationKind.File:
                {
                    if (activatedArgs.Data is Windows.ApplicationModel.Activation.IFileActivatedEventArgs fileArgs)
                    {
                        if (fileArgs.Files?.Count > 0)
                        {
                            return fileArgs.Files.Select(x => x.Path).ToList();
                        }
                    }
                    break;
                }

                case ExtendedActivationKind.Protocol:
                {
                    if (activatedArgs.Data is Windows.ApplicationModel.Activation.IProtocolActivatedEventArgs protocolArgs)
                    {
                        if (protocolArgs.Uri.Scheme == "mpv-winui")
                        {
                            var query = protocolArgs.Uri.Query;
                            if (query.StartsWith("?file="))
                            {
                                var file = query.Substring(6);
                                if (!string.IsNullOrEmpty(file))
                                {
                                    return [Uri.UnescapeDataString(file)];
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

        public async Task<IReadOnlyList<FileItem>?> ParseFileItemsAsync(AppActivationArguments activatedArgs)
        {
            switch (activatedArgs.Kind)
            {
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
                        if (protocolArgs.Uri.Scheme == "mpv-winui")
                        {
                            var query = protocolArgs.Uri.Query;
                            if (query.StartsWith("?file="))
                            {
                                var path = query.Substring(6);
                                if (!string.IsNullOrEmpty(path))
                                {
                                    FileItem? item = await Task.Run(() =>
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

                                        return null;
                                    });

                                    return item == null ? [] : [item];
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
    }
}
