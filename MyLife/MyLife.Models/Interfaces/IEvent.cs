using System;

namespace MyLife.Models
{
    public interface IEvent
    {
        string ID { get; set; }

        string Text { get; set; }

        string Description { get; set; }


        DateTime StartTime { get; set; }

        DateTime EndTime { get; set; }

    }
}
