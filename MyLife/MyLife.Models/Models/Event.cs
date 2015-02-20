using System;

namespace MyLife.Models
{
    public class Event : IEvent
    {
        public Event()
        {
            StartTime = DateTime.MinValue;
            EndTime = DateTime.MinValue;
        }

        public string ID { get; set; }

        public string Text { get; set; }
        
        public string Description { get; set; }


        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public string ImageUri { get; set; }

        public IEventSource Source { get; set; }
    }
}
