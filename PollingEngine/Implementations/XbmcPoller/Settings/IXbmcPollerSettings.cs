using System;

namespace XbmcPoller
{
    public interface IXbmcPollerSettings
    {
        string ApiBaseUrl { get; }

        string ApiUsername { get; }

        string ApiPassword { get; }

        string ConnString { get; }

        TimeSpan MinVideoLength { get; }

        TimeSpan MergeSessionPeriod { get; }
        
    }
}
