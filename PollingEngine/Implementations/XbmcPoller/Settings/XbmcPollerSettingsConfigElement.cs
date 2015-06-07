using System;
using System.Configuration;

namespace XbmcPoller
{
    public class XbmcPollerSettingsConfigElement : ConfigurationElement, IXbmcPollerSettings
    {
        public static XbmcPollerSettingsConfigElement LoadFromConfig()
        {
            var section = XbmcPollerConfigSection.LoadFromConfig();
            if (section == null)
                throw new ApplicationException("Could not load settings");
            var settings = section.Settings;
            return settings;
        }


        [ConfigurationProperty("ApiBaseUrl", DefaultValue = "http://localhost:8082/jsonrpc")]
        public string ApiBaseUrl
        {
            get { return (string)this["ApiBaseUrl"]; }
            set { this["ApiBaseUrl"] = value; }
        }

        [ConfigurationProperty("ApiUsername", DefaultValue = "xbmc")]
        public string ApiUsername
        {
            get { return (string)this["ApiUsername"]; }
            set { this["ApiUsername"] = value; }
        }

        [ConfigurationProperty("ApiPassword")]
        public string ApiPassword
        {
            get { return (string) this["ApiPassword"]; }
            set { this["ApiPassword"] = value; }
        }

        [ConfigurationProperty("ConnString")]
        public string ConnString
        {
            get { return (string)this["ConnString"]; }
            set { this["ConnString"] = value; }
        }
    }
}
