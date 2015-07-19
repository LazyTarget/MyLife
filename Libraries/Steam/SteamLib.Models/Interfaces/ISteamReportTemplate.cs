using System;

namespace SteamLib.Models
{
    public interface ISteamReportTemplate
    {
        long ID { get; }

        long UserID { get; }

        string Name { get; }

        string Description { get; }
        
        DateTime LastModified { get; }

        bool Deleted { get; }

        ISteamReportFilterSet FilterSet { get; }

    }
}
