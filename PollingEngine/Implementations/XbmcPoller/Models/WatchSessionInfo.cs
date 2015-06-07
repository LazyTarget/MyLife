using System;
using System.Collections.Generic;

namespace XbmcPoller
{
    public class WatchSessionInfo
    {
        public WatchSessionInfo()
        {
            Videos = new List<CrSessionVideo>();
        }

        public DateTime LastPollTime { get; set; }

        public List<CrSessionVideo> Videos { get; set; } 

        public CrSessionVideo ActiveVideo { get; set; }


        public long SessionID { get; set; }

        public bool Active { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

    }
}