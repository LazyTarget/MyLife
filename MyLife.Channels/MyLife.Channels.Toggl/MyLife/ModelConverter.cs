using System;
using MyLife.Models;
using Toggl;

namespace MyLife.Channels.Toggl
{
    public static class ModelConverter
    {
        public static IEvent ToEvent(TimeEntry obj)
        {
            var res = new Event
            {
                ID = obj.Id.ToString(),
                Text = obj.Description,
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
