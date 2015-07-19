using System.Collections.Generic;

namespace SteamLib.Models
{
    public interface ISteamReport : IReportInfo
    {
        long SubscriptionID { get; }

        bool Enabled { get; }

        ISteamReportFilterSet FilterSet { get; }

        IList<GamingSession> Sessions { get; }

    }
}
