﻿namespace XbmcPoller
{
    public interface IXbmcPollerSettings
    {
        string ApiBaseUrl { get; }

        string ApiUsername { get; }

        string ApiPassword { get; }

        string ConnString { get; }
        
    }
}