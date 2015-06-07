using System;

namespace XbmcPoller
{
    public class CrSessionVideo
    {
        public long SessionID { get; set; }
        public long ViewedVideoID { get; set; }
        public VideoItemInfo Video { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public bool Active { get; set; }
    }
}