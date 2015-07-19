using System.Collections.Generic;

namespace SteamLib.Models
{
    public interface ISteamReportFilterSet
    {
        long ID { get; }

        IList<SteamReportFilter> Filters { get; }
    }
}