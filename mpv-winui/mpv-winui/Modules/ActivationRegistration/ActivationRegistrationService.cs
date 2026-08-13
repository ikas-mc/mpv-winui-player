using Microsoft.Win32;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mpv_winui.Modules.ActivationRegistration;

public sealed class ActivationRegistrationService
{
    private static readonly Lazy<ActivationRegistrationService> _lazyValue = new(() => new ActivationRegistrationService(), true);
    public static ActivationRegistrationService Instance => _lazyValue.Value;

    private const string AssociationDisplayName = "mpvw";
    private static string ExePath => Environment.ProcessPath ?? string.Empty;
    private static string LogoPath => $"{ExePath},0";

    //https://github.com/microsoft/WindowsAppSDK/blob/main/dev/AppLifecycle/Association.cpp
    public static string ComputeAppId(string? exePath = null)
    {
        var seed = (exePath ?? Environment.ProcessPath ?? string.Empty).ToLowerInvariant();

        const ulong offsetBasis = 14695981039346656037UL;
        const ulong prime = 1099511628211UL;

        ulong hash = offsetBasis;
        foreach (var ch in seed)
        {
            hash = (hash ^ ((byte)(ch & 0xFF))) * prime;
            hash = (hash ^ ((byte)(ch >> 8))) * prime;
        }

        return "App." + hash.ToString("x");
    }

    public Task RegisterAsync(IReadOnlyList<string> extensions)
    {
        if (extensions is null || extensions.Count == 0)
        {
            return Task.CompletedTask;
        }

        var fileTypes = extensions.ToArray();
        return Task.Run(() =>
        {
            ActivationRegistrationManager.RegisterForFileTypeActivation(
                fileTypes, LogoPath, AssociationDisplayName, ["open"], ExePath);
        });
    }

    public Task UnregisterAsync(IReadOnlyList<string> extensions)
    {
        if (extensions is null || extensions.Count == 0)
        {
            return Task.CompletedTask;
        }

        var fileTypes = extensions.ToArray();
        return Task.Run(() =>
        {
            try
            {
                ActivationRegistrationManager.UnregisterForFileTypeActivation(fileTypes, null);
            }
            catch (Exception)
            {
                // Ignore failures; an extension may already be unregistered.
            }
        });
    }

    public Task<IReadOnlyList<string>> GetRegisteredExtensionsAsync()
    {
        return Task.Run(GetRegisteredExtensions);
    }

    public Task RegisterProtocolAsync(string scheme)
    {
        if (string.IsNullOrWhiteSpace(scheme))
        {
            return Task.CompletedTask;
        }

        var s = NormalizeScheme(scheme);
        return Task.Run(() =>
        {
            ActivationRegistrationManager.RegisterForProtocolActivation(s, LogoPath, AssociationDisplayName, ExePath);
        });
    }

    public Task UnregisterProtocolAsync(string scheme)
    {
        if (string.IsNullOrWhiteSpace(scheme))
        {
            return Task.CompletedTask;
        }

        var s = NormalizeScheme(scheme);
        return Task.Run(() =>
        {
            try
            {
                ActivationRegistrationManager.UnregisterForProtocolActivation(s, null);
            }
            catch (Exception)
            {
                // Ignore failures; the scheme may already be unregistered.
            }
        });
    }

    public Task<IReadOnlyList<string>> GetRegisteredProtocolsAsync()
    {
        return Task.Run(GetRegisteredProtocols);
    }

    private string NormalizeScheme(string scheme)
    {
        var s = scheme.Trim();
        var index = s.IndexOf("://", StringComparison.Ordinal);
        if (index >= 0)
        {
            s = s[..index];
        }

        return s.TrimEnd(':').ToLowerInvariant();
    }

    private IReadOnlyList<string> GetRegisteredExtensions()
    {
        var appId = ComputeAppId();
        var progId = appId + ".File";
        var path = $@"Software\Microsoft\WindowsAppRuntimeApplications\{appId}\Capabilties\FileAssociations";

        using var key = Registry.CurrentUser.OpenSubKey(path);
        if (key is null)
        {
            return [];
        }

        var result = new List<string>();
        foreach (var name in key.GetValueNames())
        {
            if (name.StartsWith('.') && string.Equals(key.GetValue(name) as string, progId, StringComparison.Ordinal))
            {
                result.Add(name);
            }
        }

        return result;
    }

    private IReadOnlyList<string> GetRegisteredProtocols()
    {
        var appId = ComputeAppId();
        var progId = appId + ".Protocol";
        var path = $@"Software\Microsoft\WindowsAppRuntimeApplications\{appId}\Capabilties\UrlAssociations";

        using var key = Registry.CurrentUser.OpenSubKey(path);
        if (key is null)
        {
            return [];
        }

        var result = new List<string>();
        foreach (var name in key.GetValueNames())
        {
            if (string.Equals(key.GetValue(name) as string, progId, StringComparison.Ordinal))
            {
                result.Add(name);
            }
        }

        return result;
    }
}
