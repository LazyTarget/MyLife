using System;

namespace ProcessPoller
{
    public interface IProcessPollerSettings
    {
        string DataApiBaseUrl { get; }

        string MachineName { get; }

        // todo: add process filters

        Func<System.Diagnostics.Process, bool> ProcessFilter { get; }

    }
}
