using System;
using System.Globalization;

namespace SteamPoller
{
    public class SteamReportPollerSettings : ISteamReportPollerSettings
    {
        public SteamReportPollerSettings()
        {
            FirstDayOfWeek = DayOfWeek.Monday;
            CultureInfo = new CultureInfo("sv-SE");
        }

        public string ConnString { get; set; }

        public CultureInfo CultureInfo { get; set; }

        public DayOfWeek FirstDayOfWeek { get; set; }
    }
}
