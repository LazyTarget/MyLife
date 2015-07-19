using System;

namespace SteamLib.Models
{
    public class SteamReportGenerationRequest : ISteamReportGenerationRequest
    {
        public long ID { get; set; }
        public long UserID { get; set; }
        public long SubscriptionID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool Enabled { get; set; }
        public SteamReportFilterSet FilterSet { get; set; }
    }
}
