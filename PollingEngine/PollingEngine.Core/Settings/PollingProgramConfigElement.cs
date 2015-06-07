using System;
using System.Configuration;

namespace PollingEngine.Core
{
    public class PollingProgramConfigElement : ConfigurationElement, IPollingProgramGeneralSettings
    {
        [ConfigurationProperty("Type", IsKey = true, IsRequired = true)]
        public string Type
        {
            get { return (string)this["Type"]; }
            set { this["Type"] = value; }
        }

        [ConfigurationProperty("Enabled", DefaultValue = "false", IsRequired = true)]
        public bool Enabled
        {
            get { return (bool)this["Enabled"]; }
            set { this["Enabled"] = value; }
        }

        [ConfigurationProperty("Interval", DefaultValue = "00:00:15", IsRequired = true)]
        public TimeSpan Interval
        {
            get { return (TimeSpan)this["Interval"]; }
            set { this["Interval"] = value; }
        }
    }
}
