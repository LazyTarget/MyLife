using System.Collections.Generic;

namespace SteamLib.Models
{
    public class SteamReportFilterSet : ISteamReportFilterSet
    {
        public SteamReportFilterSet()
        {
            Filters = new List<SteamReportFilter>();
        }

        public long ID { get; set; }

        public IList<SteamReportFilter> Filters { get; set; } 
    }
}