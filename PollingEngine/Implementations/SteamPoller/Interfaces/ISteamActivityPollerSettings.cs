using System;
using System.Collections.Generic;

namespace SteamPoller
{
    public interface ISteamActivityPollerSettings
    {
        string SteamApiKey { get; }

        string ConnString { get; }

        IList<long> Identities { get; }

        TimeSpan MergeSessionPeriod { get; }
        
    }
}
