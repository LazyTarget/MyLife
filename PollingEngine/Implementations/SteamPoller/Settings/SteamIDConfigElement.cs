using System.Configuration;

namespace SteamPoller
{
    public class SteamIDConfigElement : ConfigurationElement
    {
        [ConfigurationProperty("SteamID", IsRequired = true)]
        public long SteamID
        {
            get { return (long)this["SteamID"]; }
            set { this["SteamID"] = value; }
        }
    }
}
