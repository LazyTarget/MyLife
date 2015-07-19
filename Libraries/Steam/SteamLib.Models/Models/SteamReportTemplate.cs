using System;

namespace SteamLib.Models
{
    public class SteamReportTemplate : ISteamReportTemplate
    {
        public long UserID { get; set; }
        public long ID { get; set; }
        public long? FilterSetID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime LastModified { get; set; }
        public bool Deleted { get; set; }
        public ISteamReportFilterSet FilterSet { get; set; }
        
    }
}
