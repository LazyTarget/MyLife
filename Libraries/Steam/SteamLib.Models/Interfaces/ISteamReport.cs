using System.Collections.Generic;

namespace SteamLib.Models
{
    public interface ISteamReport : IReportInfo
    {
        ISteamReportFilterSet FilterSet { get; }

        IList<GamingSession> Sessions { get; } 

    }
}
