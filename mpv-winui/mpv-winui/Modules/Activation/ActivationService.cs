using Microsoft.Windows.AppLifecycle;
using System;
using System.Collections.Generic;
using System.Linq;

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
    }
}
