namespace SteamLib.Models
{
    public enum FilterGroupRule
    {
        AND,
        OR,
    }

    public enum FilterOperator
    {
        None,
        LessThan,
        LessOrEqualsThan,
        Equals,
        GreaterOrEqualsThan,
        GreaterThan,
    }

    public enum ReportPeriodType
    {
        None,
        Hourly,
        Daily,
        Weekly,
        Monthly,
        Quarterly,
        Yearly,
    }


    public enum SteamReportAttribute
    {
        None,
        UserID,
        GameID,
        StatChangeCount,
        AchievementCount,
    }
}