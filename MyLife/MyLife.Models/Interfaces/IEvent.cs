using System;

namespace MyLife.Models
{
    public interface IEvent
    {
        string ID { get; }

        string Text { get; }

        string Description { get; }


        DateTime StartTime { get; }

        DateTime EndTime { get; }


        string ImageUri { get; }

        IEventSource Source { get; set; }

    }
}
