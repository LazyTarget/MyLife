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
        Weekly,
        Monthly,
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