using System;
using System.Configuration;

namespace SteamPoller
{
    public class SteamPollerConfigSection : ConfigurationSection
    {
        public static SteamPollerConfigSection LoadFromConfig()
        {
            var settings = ConfigurationManager.GetSection("SteamPoller") as SteamPollerConfigSection;
            if (settings == null)
                throw new ApplicationException("Could not load settings");
            return settings;
        }
        

        [ConfigurationProperty("Settings")]
        public SteamPollerSettingsConfigElement Settings
        {
            get { return (SteamPollerSettingsConfigElement) this["Settings"]; }
        }

    }
}
