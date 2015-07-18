using System;

namespace SteamLib.Models
{
    public interface IReportInfo
    {
        long ID { get; }

        string Name { get; }

        string Description { get; }
        
        DateTime StartTime { get; }

        DateTime EndTime { get; }

        DateTime LastModified { get; }

        DateTime LastGenerated { get; }

    }
}
