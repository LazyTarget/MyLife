using System;
using Newtonsoft.Json;

namespace CalendarCore
{
    public class Event
    {
        [JsonProperty(PropertyName = "id")]
        public string ID { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "location")]
        public string Location { get; set; }


        [JsonProperty(PropertyName = "is_all_day_event")]
        public bool IsAllDayEvent { get; set; }

        [JsonProperty(PropertyName = "is_recurrent")]
        public bool IsRecurrent { get; set; }

        [JsonProperty(PropertyName = "start_time")]
        public DateTime Start { get; set; }

        [JsonProperty(PropertyName = "end_time")]
        public DateTime End { get; set; }


    }
}
