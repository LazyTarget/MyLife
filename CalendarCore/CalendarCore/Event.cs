using System;

namespace CalendarCore
{
    public class Event
    {
        public string ID { get; set; }
        
        public string Name { get; set; }

        public string Description { get; set; }


        public bool IsAllDayEvent { get; set; }

        public bool IsRecurrent { get; set; }

        public DateTime Start { get; set; }
        
        public DateTime End { get; set; }


    }
}
