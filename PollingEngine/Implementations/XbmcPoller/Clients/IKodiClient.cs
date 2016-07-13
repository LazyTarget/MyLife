using System.Threading.Tasks;

namespace XbmcPoller
{
    public interface IKodiClient
    {
        Task<bool> PingXbmc();
        Task<string> GetVersion();
        Task<VideoItemInfo> GetItemInfo();
        Task GetPlaybackInfo();
    }
}