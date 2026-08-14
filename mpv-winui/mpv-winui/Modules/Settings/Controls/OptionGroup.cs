using System.Collections.Generic;
using WinRT;

namespace mpv_winui.Modules.Settings.Controls;

[GeneratedBindableCustomProperty]
public partial record OptionGroup(string Key, string? Label, IReadOnlyList<Option> Options)
{
}
