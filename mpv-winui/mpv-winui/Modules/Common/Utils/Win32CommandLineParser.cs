using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace mpv_winui.Modules.Common.Utils;

public static class Win32CommandLineParser
{
    public static Task<IReadOnlyList<string>> ParseAsync(string commandLine, bool ignoreFirst)
    {
        return Task.Run(() =>
        {
            return Parse(commandLine, ignoreFirst);
        });
    }

    public static unsafe IReadOnlyList<string> Parse(string commandLine, bool ignoreFirst)
    {
        if (string.IsNullOrEmpty(commandLine))
        {
            return [];
        }

        int argc = 0;
        PWSTR* argv = null;
        try
        {
            argv = PInvoke.CommandLineToArgv(commandLine, out argc);
            if (ignoreFirst)
            {
                if (argv == null || argc <= 1)
                {
                    return [];
                }

                var result = new string[argc - 1];
                for (int i = 1; i < argc; i++)
                {
                    result[i - 1] = argv[i].ToString();
                }

                return result;
            }
            else
            {
                if (argv == null || argc <= 0)
                {
                    return [];
                }

                var result = new string[argc];
                for (int i = 0; i < argc; i++)
                {
                    result[i] = argv[i].ToString();
                }
                return result;
            }
        }
        finally
        {
            if (argv != null)
            {
                PInvoke.LocalFree(new HLOCAL(argv));
            }
        }
    }
}
