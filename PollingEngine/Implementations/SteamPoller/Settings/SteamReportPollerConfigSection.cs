using System;
using System.Configuration;

namespace SteamPoller
{
    public class SteamReportPollerConfigSection : ConfigurationSection
    {
        public static SteamReportPollerConfigSection LoadFromConfig()
        {
            var settings = ConfigurationManager.GetSection("SteamReportPoller") as SteamReportPollerConfigSection;
            if (settings == null)
                throw new ApplicationException("Could not load settings");
            return settings;
        }
        

        [ConfigurationProperty("Settings")]
        public SteamReportPollerSettingsConfigElement Settings
        {
            get { return (SteamReportPollerSettingsConfigElement) this["Settings"]; }
        }

    }
}
