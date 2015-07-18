using System;

namespace SteamLib.Models
{
    public interface ISteamReportGenerationRequest
    {
        long ID { get; }

        long UserID { get; set; }

        string Name { get; }

        string Description { get; }

        DateTime StartTime { get; }
        
        DateTime EndTime { get; }

        SteamReportFilterSet FilterSet { get; } 

    }
}
