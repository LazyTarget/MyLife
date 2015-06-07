using System.Collections.Generic;

namespace SteamPoller
{
    public class SteamPollerSettings : ISteamPollerSettings
    {
        public SteamPollerSettings()
        {
            Identities = new List<long>();
        }

        public string SteamApiKey { get; set; }

        public string ConnString { get; set; }
        
        public IList<long> Identities { get; set; }

    }
}
