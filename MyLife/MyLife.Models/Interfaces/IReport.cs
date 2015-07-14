using System;

namespace MyLife.Models
{
    public interface IReport
    {
        string ID { get; }

        string Name { get; }

        string Description { get; }
        
        DateTime StartTime { get; }

        DateTime EndTime { get; }

        DateTime LastModified { get; }

        DateTime LastGenerated { get; }

        IEvent ToEvent();

    }
}
