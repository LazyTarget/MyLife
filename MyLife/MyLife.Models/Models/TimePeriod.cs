using System;

namespace MyLife.Models
{
    public class TimePeriod
    {
        public TimePeriod()
        {
            StartTime = DateTime.MinValue;
            EndTime = DateTime.MaxValue;
        }

        public DateTime StartTime { get; set; }
        
        public DateTime EndTime { get; set; }

    }
}
