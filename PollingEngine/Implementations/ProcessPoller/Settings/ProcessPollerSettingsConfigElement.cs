using System;
using System.Configuration;

namespace ProcessPoller
{
    public class ProcessPollerSettingsConfigElement : ConfigurationElement, IProcessPollerSettings
    {
        public static ProcessPollerSettingsConfigElement LoadFromConfig()
        {
            var section = ProcessPollerConfigSection.LoadFromConfig();
            if (section == null)
                throw new ApplicationException("Could not load settings");
            var settings = section.Settings;
            return settings;
        }

        public ProcessPollerSettingsConfigElement()
        {
            //var def = new ProcessPollerSettings();
            //ProcessFilter = def.ProcessFilter;
        }


        [ConfigurationProperty("DataApiBaseUrl", IsRequired = true)]
        public string DataApiBaseUrl
        {
            get { return (string)this["DataApiBaseUrl"]; }
            set { this["DataApiBaseUrl"] = value; }
        }

        [ConfigurationProperty("MachineName")]
        public string MachineName
        {
            get { return (string)this["MachineName"]; }
            set { this["MachineName"] = value; }
        }

        [ConfigurationProperty("ProcessFilter")]
        public Func<System.Diagnostics.Process, bool> ProcessFilter
        {
            get { return (Func<System.Diagnostics.Process, bool>)this["ProcessFilter"]; }
            set { this["ProcessFilter"] = value; }
        }
    }
}
