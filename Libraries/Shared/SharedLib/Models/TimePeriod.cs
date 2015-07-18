using System;

namespace SharedLib
{
    public class TimePeriod : IEquatable<TimePeriod>
    {
        public TimePeriod()
        {
            StartTime = DateTime.MinValue;
            EndTime = DateTime.MaxValue;
        }

        public DateTime StartTime { get; set; }
        
        public DateTime EndTime { get; set; }




        public bool Equals(TimePeriod other)
        {
            if (other == null)
                return false;
            var res = StartTime.Equals(other.StartTime) &&
                      EndTime.Equals(other.EndTime);
            return res;
        }


        public static TimePeriod All
        {
            get
            {
                return new TimePeriod
                {
                    StartTime = DateTime.MinValue,
                    EndTime = DateTime.MaxValue,
                };
            }
        }

    }
}
