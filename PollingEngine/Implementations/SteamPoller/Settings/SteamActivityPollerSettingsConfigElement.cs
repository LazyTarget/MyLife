using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace SteamPoller
{
    public class SteamActivityPollerSettingsConfigElement : ConfigurationElement, ISteamActivityPollerSettings
    {
        public static SteamActivityPollerSettingsConfigElement LoadFromConfig()
        {
            var section = SteamActivityPollerConfigSection.LoadFromConfig();
            if (section == null)
                throw new ApplicationException("Could not load settings");
            var settings = section.Settings;
            return settings;
        }


        [ConfigurationProperty("SteamApiKey", IsRequired = true)]
        public string SteamApiKey
        {
            get { return (string)this["SteamApiKey"]; }
            set { this["SteamApiKey"] = value; }
        }

        [ConfigurationProperty("ConnString")]
        public string ConnString
        {
            get { return (string)this["ConnString"]; }
            set { this["ConnString"] = value; }
        }

        [ConfigurationProperty("Identities", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(SteamIDCollection),
            AddItemName = "add",
            ClearItemsName = "clear",
            RemoveItemName = "remove")]
        public SteamIDCollection Identities
        {
            get { return (SteamIDCollection)this["Identities"]; }
            set { this["Identities"] = value; }
        }

        IList<long> ISteamActivityPollerSettings.Identities
        {
            get
            {
                return Identities.Cast<SteamIDConfigElement>().Select(x => x.SteamID).ToList();
            }
        }

        [ConfigurationProperty("MergeSessionPeriod", DefaultValue = "00:05")]
        public TimeSpan MergeSessionPeriod
        {
            get { return (TimeSpan)this["MergeSessionPeriod"]; }
            set { this["MergeSessionPeriod"] = value; }
        }
    }
}
