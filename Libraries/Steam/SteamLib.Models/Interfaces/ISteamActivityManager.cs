using System.Collections.Generic;
using System.Threading.Tasks;
using SharedLib;

namespace SteamLib.Models
{
    public interface ISteamActivityManager
    {
        Task<IEnumerable<GamingSession>> GetGamingSessions(TimeRange request);

        Task<IEnumerable<GamingSession>> GetGamingSessions(TimeRange request, params long[] steamUserIDs);
    }
}
