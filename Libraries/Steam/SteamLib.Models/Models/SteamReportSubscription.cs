namespace SteamLib.Models
{
    public class SteamReportSubscription : ISteamReportSubscription
    {
        public long ID { get; set; }
        public long UserID { get; set; }
        public long TemplateID { get; set; }
        public bool Enabled { get; set; }
        public bool Deleted { get; set; }
        public ReportPeriodType PeriodType { get; set; }
    }
}
