using System;
using System.Configuration;

namespace SteamPoller
{
    public class SteamReportPollerSettingsConfigElement : ConfigurationElement, ISteamReportPollerSettings
    {
        public static SteamReportPollerSettingsConfigElement LoadFromConfig()
        {
            var section = SteamReportPollerConfigSection.LoadFromConfig();
            if (section == null)
                throw new ApplicationException("Could not load settings");
            var settings = section.Settings;
            return settings;
        }


        [ConfigurationProperty("ConnString")]
        public string ConnString
        {
            get { return (string)this["ConnString"]; }
            set { this["ConnString"] = value; }
        }
    }
}
