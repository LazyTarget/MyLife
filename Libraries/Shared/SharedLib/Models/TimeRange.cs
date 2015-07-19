using System;

namespace SharedLib
{
    public class TimeRange : IEquatable<TimeRange>
    {
        public TimeRange()
        {
            StartTime = DateTime.MinValue;
            EndTime = DateTime.MaxValue;
        }

        public TimeRange(DateTime start, DateTime end)
        {
            StartTime = start;
            EndTime = end;
        }

        public DateTime StartTime { get; set; }
        
        public DateTime EndTime { get; set; }




        public bool Equals(TimeRange other)
        {
            if (other == null)
                return false;
            var res = StartTime.Equals(other.StartTime) &&
                      EndTime.Equals(other.EndTime);
            return res;
        }


        public static TimeRange All
        {
            get
            {
                return new TimeRange
                {
                    StartTime = DateTime.MinValue,
                    EndTime = DateTime.MaxValue,
                };
            }
        }

    }
}
