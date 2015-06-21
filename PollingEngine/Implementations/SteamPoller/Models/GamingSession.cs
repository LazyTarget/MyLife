using System;

namespace SteamPoller.Models
{
    public class GamingSession
    {
        private DateTime _startTime;
        private DateTime _endTime;


        public long ID { get; set; }
        public long UserID { get; set; }
        public long GameID { get; set; }
        public string GameName { get; set; }
        public bool Active { get; set; }

        public DateTime StartTime
        {
            get { return _startTime; }
            set
            {
                if (_startTime.Kind == DateTimeKind.Unspecified)
                    _startTime = DateTime.SpecifyKind(value, DateTimeKind.Utc);
                else if (_startTime.Kind == DateTimeKind.Local)
                    _startTime = value.ToUniversalTime();
                else
                    _startTime = value;
            }
        }

        public DateTime EndTime
        {
            get { return _endTime; }
            set
            {
                if (_endTime.Kind == DateTimeKind.Unspecified)
                    _endTime = DateTime.SpecifyKind(value, DateTimeKind.Utc);
                else if (_endTime.Kind == DateTimeKind.Local)
                    _endTime = value.ToUniversalTime();
                else
                    _endTime = value;
            }
        }

        public TimeSpan Duration
        {
            get { return _endTime.Subtract(_startTime); }
        }
    }
}
