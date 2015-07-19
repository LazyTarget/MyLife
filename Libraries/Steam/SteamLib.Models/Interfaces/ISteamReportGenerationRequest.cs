using System;

namespace SteamLib.Models
{
    public interface ISteamReportGenerationRequest
    {
        long ID { get; }

        long UserID { get; }

        long SubscriptionID { get; }

        string Name { get; }

        string Description { get; }

        DateTime StartTime { get; }
        
        DateTime EndTime { get; }

        bool Enabled { get; }

        SteamReportFilterSet FilterSet { get; }

    }
}
