using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CalendarCore.Outlook
{
    public static class JsonDtoConverter
    {
        public static Calendar ToCalendar(string json)
        {
            var obj = JsonConvert.DeserializeObject<JObject>(json);

            var result = new Calendar();
            return result;
        }


        public static IEnumerable<Calendar> ToCalendars(string json)
        {
            var arr = JsonConvert.DeserializeObject<JArray>(json);
            foreach (var token in arr)
            {
                var j = token.Value<string>();
                var cal = ToCalendar(j);
                yield return cal;
            }
        }

    }
}
