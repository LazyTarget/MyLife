using System.Collections.Generic;

namespace SteamPoller
{
    public interface ISteamPollerSettings
    {
        string SteamApiKey { get; set; }

        string PollingDataConnString { get; }

        IList<long> Identities { get; }
        
    }
}
