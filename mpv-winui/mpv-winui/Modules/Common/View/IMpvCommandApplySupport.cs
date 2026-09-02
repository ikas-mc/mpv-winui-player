using System.Threading.Tasks;

namespace mpv_winui.Modules.Common.View
{
    public interface IMpvCommandApplySupport
    {
        Task ApplyMpvCommandAsync(string command);
    }
}