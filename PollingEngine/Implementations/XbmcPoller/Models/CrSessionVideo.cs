using System;

namespace XbmcPoller
{
    public class CrSessionVideo
    {
        public long ID { get; set; }
        public long SessionID { get; set; }
        public long ViewedVideoID { get; set; }
        public VideoItemInfo Video { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public TimeSpan TimePaused { get; set; }

        public bool Paused { get; set; }

        public bool Active { get; set; }
    }
}