using System;

namespace XbmcPoller
{
    public class XbmcPollerSettings : IXbmcPollerSettings
    {
        public string ApiBaseUrl { get; set; }

        public string ApiUsername { get; set; }

        public string ApiPassword { get; set; }

        public string ConnString { get; set; }

        public TimeSpan MinVideoLength { get; set; }

        public TimeSpan MergeSessionPeriod { get; set; }

        public bool CloseSessionOnStop { get; set; }
    }
}
