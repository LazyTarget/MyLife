using MyLife.Models;

namespace MyLife.Channels.SteamPoller
{
    public class SteamReportFilter
    {
        public long ID { get; set; }
        public int GroupID { get; set; }
        public FilterGroupRule GroupRule { get; set; }
        public SteamReportAttribute Attribute { get; set; }
        public FilterOperator Operator { get; set; }
        public object Value { get; set; }
    }
}