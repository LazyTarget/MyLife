using System;
using System.Globalization;

namespace SteamPoller
{
    public interface ISteamReportPollerSettings
    {
        string ConnString { get; }

        CultureInfo CultureInfo { get; }

        DayOfWeek FirstDayOfWeek { get; }
        
    }
}
