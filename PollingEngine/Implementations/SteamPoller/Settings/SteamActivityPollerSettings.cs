using System;
using System.Collections.Generic;

namespace SteamPoller
{
    public class SteamActivityPollerSettings : ISteamActivityPollerSettings
    {
        public SteamActivityPollerSettings()
        {
            Identities = new List<long>();
        }

        public string SteamApiKey { get; set; }

        public string ConnString { get; set; }
        
        public IList<long> Identities { get; set; }
        
        public TimeSpan MergeSessionPeriod { get; set; }
    }
}
