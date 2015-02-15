using System;
using MyLife.Models;
using Toggl;

namespace MyLife.Channels.Toggl
{
    public static class ModelConverter
    {
        private static IEventSource GetEventSource()
        {
            var source = new EventSource
            {
                Name = "Toggl",
            };
            return source;
        }


        public static IEvent ToEvent(TimeEntry obj)
        {
            var res = new Event
            {
                ID = obj.Id.ToString(),
                Text = obj.Description,

                Source = GetEventSource(),
            };

            DateTime tmp;
            if (DateTime.TryParse(obj.Start, out tmp))
                res.StartTime = tmp;
            if (DateTime.TryParse(obj.Stop, out tmp))
                res.EndTime = tmp;
            return res;
        }

    }
}
