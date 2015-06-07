using System;
using System.Configuration;

namespace XbmcPoller
{
    public class XbmcPollerConfigSection : ConfigurationSection
    {
        public static XbmcPollerConfigSection LoadFromConfig()
        {
            var settings = ConfigurationManager.GetSection("XbmcPoller") as XbmcPollerConfigSection;
            if (settings == null)
                throw new ApplicationException("Could not load settings");
            return settings;
        }
        

        [ConfigurationProperty("Settings")]
        public XbmcPollerSettingsConfigElement Settings
        {
            get { return (XbmcPollerSettingsConfigElement) this["Settings"]; }
        }

    }
}
