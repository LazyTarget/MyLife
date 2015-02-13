using System.Collections.Generic;
using CalendarCore.Http;
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

    }
}
