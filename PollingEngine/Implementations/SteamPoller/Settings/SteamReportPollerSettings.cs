using System;

namespace SteamPoller
{
    public class SteamReportPollerSettings : ISteamReportPollerSettings
    {
        public SteamReportPollerSettings()
        {

        }

        public string ConnString { get; set; }
    }
}
