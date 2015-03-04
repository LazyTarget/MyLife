using com.strava.api.Activities;
using MyLife.Models;

namespace MyLife.Channels.Strava
{
    public static class ModelConverter
    {
        public static IEventSource GetEventSource()
        {
            var source = new EventSource
            {
                Name = "Strava",
            };
            return source;
        }
        
        public static IEvent ToEvent(ActivitySummary obj)
        {
            var res = new Event
            {
                ID = obj.Id.ToString(),
                Text = obj.Name,
                StartTime = obj.DateTimeStart,
                EndTime = obj.DateTimeStart.Add(obj.ElapsedTimeSpan),

                Source = GetEventSource(),
            };
            return res;
        }


        public static IEvent ToEvent(Activity obj)
        {
            var res = ToEvent((ActivitySummary) obj);
            res.Description = obj.Description;
            return res;
        }

    }
}
