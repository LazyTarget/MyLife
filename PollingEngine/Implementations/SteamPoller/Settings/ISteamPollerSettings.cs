using System.Collections.Generic;

namespace SteamPoller
{
    public interface ISteamPollerSettings
    {
        string SteamApiKey { get; }

        string ConnString { get; }

        IList<long> Identities { get; }
        
    }
}
