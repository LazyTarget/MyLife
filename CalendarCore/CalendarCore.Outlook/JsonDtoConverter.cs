using System;
using System.Collections.Generic;
using MyLife.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CalendarCore.Outlook
{
    public static class JsonDtoConverter
    {
        public static Calendar ToCalendar(JObject data)
        {
            var result = new Calendar();
            result.ID = data.GetPropertyValue<string>("id");
            result.Name = data.GetPropertyValue<string>("name");

            return result;
        }


        public static IEnumerable<Calendar> ToCalendars(JArray array)
        {
            foreach (var token in array)
            {
                var o = token.ToObjectOrDefault<JObject>();
                var cal = ToCalendar(o);
                yield return cal;
            }
        }


        public static Event ToEvent(JObject data)
        {
            var result = new Event();
            result.ID = data.GetPropertyValue<string>("id");
            result.Name = data.GetPropertyValue<string>("name");
            result.Description = data.GetPropertyValue<string>("description");
            result.Start = data.GetPropertyValue<DateTime>("start_time");
            result.End = data.GetPropertyValue<DateTime>("end_time");
            result.IsAllDayEvent = data.GetPropertyValue<bool>("is_all_day_event");
            result.IsRecurrent = data.GetPropertyValue<bool>("is_recurrent");
            return result;
        }


        public static IEnumerable<Event> ToEvents(JArray array)
        {
            foreach (var token in array)
            {
                var o = token.ToObjectOrDefault<JObject>();
                var evt = ToEvent(o);
                yield return evt;
            }
        }

    }
}
