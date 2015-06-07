using System;

namespace PollingEngine.Core
{
    public interface IPollingProgramGeneralSettings
    {
        string Type { get; }

        bool Enabled { get; }

        TimeSpan Interval { get; }
        
    }
}
