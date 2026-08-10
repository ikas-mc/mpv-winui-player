using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace mpv_winui.Modules.Player.Menu
{
    [JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    [JsonSerializable(typeof(List<CustomMenuItem>))]
    internal partial class MenuJsonContext : JsonSerializerContext
    {
    }
}
