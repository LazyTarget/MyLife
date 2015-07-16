using System;
using System.Dynamic;
using MyLife.Models;
using OdbcWrapper;

namespace MyLife.Channels.Odbc
{
    public static class ModelConverter
    {
        public static IEventSource GetEventSource()
        {
            var source = new EventSource
            {
                Name = "Odbc",
            };
            return source;
        }
        
        public static IEvent ToEvent(ExpandoObject obj)
        {
            var res = new Event
            {
                ID = obj.Get<string>("ID"),
                Text = obj.Get<string>("Text"),
                Description = obj.Get<string>("Description"),
                StartTime = obj.Get<DateTime>("StartTime"),
                EndTime = obj.Get<DateTime>("EndTime"),
                
                Source = GetEventSource(),
            };

            var s = obj.Get<string>("Source");
            if (!string.IsNullOrWhiteSpace(s))
                res.Source.Name = s;
            return res;
        }

    }
}
