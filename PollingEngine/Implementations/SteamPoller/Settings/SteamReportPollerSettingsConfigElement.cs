using System;
using System.Configuration;
using System.Globalization;

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

        [ConfigurationProperty("CultureInfo", DefaultValue = "sv-SE")]
        public CultureInfo CultureInfo
        {
            get { return (CultureInfo)this["CultureInfo"]; }
            set { this["CultureInfo"] = value.Name; }
        }

        [ConfigurationProperty("FirstDayOfWeek", DefaultValue = "Monday")]
        public DayOfWeek FirstDayOfWeek
        {
            get { return (DayOfWeek)this["FirstDayOfWeek"]; }
            set { this["FirstDayOfWeek"] = value; }
        }
    }
}
