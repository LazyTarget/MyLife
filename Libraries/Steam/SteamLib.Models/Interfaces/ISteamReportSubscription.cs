using System;

namespace SteamLib.Models
{
    public interface ISteamReportSubscription
    {
        long ID { get; }

        long UserID { get; }

        long TemplateID { get; }

        bool Enabled { get; }

        bool Deleted { get; }

        ReportPeriodType PeriodType { get; }

    }
}
