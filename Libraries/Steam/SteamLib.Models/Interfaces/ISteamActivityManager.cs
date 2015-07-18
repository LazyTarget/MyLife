using System.Collections.Generic;
using System.Threading.Tasks;
using SharedLib;

namespace SteamLib.Models
{
    public interface ISteamActivityManager
    {
        Task<IEnumerable<GamingSession>> GetGamingSessions(TimePeriod request);

        Task<IEnumerable<GamingSession>> GetGamingSessions(TimePeriod request, params long[] steamUserIDs);
    }
}
