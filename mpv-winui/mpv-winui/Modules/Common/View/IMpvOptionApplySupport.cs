using System.Threading.Tasks;

namespace mpv_winui.Modules.Common.View
{
    public interface IMpvOptionApplySupport
    {
        Task ApplyMpvOptionAsync(string key, string value);
    }
}
