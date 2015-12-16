using System;
using System.Configuration;

namespace ProcessPoller
{
    public class ProcessPollerConfigSection : ConfigurationSection
    {
        public static ProcessPollerConfigSection LoadFromConfig()
        {
            var settings = ConfigurationManager.GetSection("ProcessPoller") as ProcessPollerConfigSection;
            if (settings == null)
                throw new ApplicationException("Could not load settings");
            return settings;
        }
        

        [ConfigurationProperty("Settings")]
        public ProcessPollerSettingsConfigElement Settings
        {
            get { return (ProcessPollerSettingsConfigElement) this["Settings"]; }
        }

    }
}
