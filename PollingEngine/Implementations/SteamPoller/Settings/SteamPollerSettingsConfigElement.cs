using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace SteamPoller
{
    public class SteamPollerSettingsConfigElement : ConfigurationElement, ISteamPollerSettings
    {
        public static SteamPollerSettingsConfigElement LoadFromConfig()
        {
            var section = SteamPollerConfigSection.LoadFromConfig();
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

        IList<long> ISteamPollerSettings.Identities
        {
            get
            {
                return Identities.Cast<SteamIDConfigElement>().Select(x => x.SteamID).ToList();
            }
        }
    }
}
