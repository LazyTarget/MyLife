using System;

namespace SharedLib
{
    public static class DateExtensions
    {
        public static DateTime GetFirstDayOfWeek(this DateTime now, DayOfWeek firstDayOfWeek)
        {
            var date = now.Date;
            while (date.DayOfWeek != firstDayOfWeek)
            {
                date = date.AddDays(-1);
            }
            return date;
        }

        public static DateTime GetFirstDayOfMonth(this DateTime now)
        {
            var date = now.Date.AddDays(-(now.Day - 1));
            return date;
        }

        public static DateTime GetFirstDayOfQuarter(this DateTime now)
        {
            var date = now.Date;
            while ((date.Month - 1) % 3 != 0)
            {
                date = date.AddMonths(-1);
            }
            date = date.GetFirstDayOfMonth();
            return date;
        }

        public static DateTime GetFirstDayOfYear(this DateTime now)
        {
            var date = now.Date.AddMonths(-(now.Month - 1));
            date = date.GetFirstDayOfMonth();
            return date;
        }

        public static int GetQuarter(this DateTime now)
        {
            var date = now.Date;
            var quarter = (int)Math.Floor(date.Month / 3d) + 1;
            return quarter;
        }
        


        public static TimeRange GetTimeRange(this DateTime now, TimePeriod timePeriod)
        {
            var timeRange = GetTimeRange(now, timePeriod, DayOfWeek.Monday);
            return timeRange;
        }

        public static TimeRange GetTimeRange(this DateTime now, TimePeriod timePeriod, DayOfWeek firstDayOfWeek)
        {
            DateTime start, end;
            switch (timePeriod)
            {
                case TimePeriod.Hour:
                    start = now.Date.AddHours(now.Hour);
                    end = start.AddHours(1);
                    break;

                case TimePeriod.Day:
                    start = now.Date;
                    end = start.AddDays(1);
                    break;

                case TimePeriod.Week:
                    start = now.GetFirstDayOfWeek(firstDayOfWeek);
                    end = start.AddDays(7);
                    break;

                case TimePeriod.Month:
                    start = now.GetFirstDayOfMonth();
                    end = start.AddMonths(1);
                    break;

                case TimePeriod.Quarter:
                    var quarter = (int) Math.Floor(now.Month / 4d);
                    start = now.GetFirstDayOfYear().AddMonths(quarter * 4);
                    end = start.AddMonths(1);
                    break;

                case TimePeriod.Year:
                    start = now.GetFirstDayOfYear();
                    end = start.AddYears(1);
                    break;

                default:
                    throw new NotImplementedException(String.Format("TimePeriod '{0}' not implemented", timePeriod));
            }
            var timeRange = new TimeRange(start, end);
            return timeRange;
        }
    }
}
