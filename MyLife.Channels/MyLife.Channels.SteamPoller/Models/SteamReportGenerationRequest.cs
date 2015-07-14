using System;
using System.Collections.Generic;

namespace MyLife.Channels.SteamPoller
{
    public class SteamReportGenerationRequest : ISteamReportGenerationRequest
    {
        public SteamReportGenerationRequest()
        {
            Filters = new List<SteamReportFilter>();
        }

        public string ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public IList<SteamReportFilter> Filters { get; set; } 

    }
}
