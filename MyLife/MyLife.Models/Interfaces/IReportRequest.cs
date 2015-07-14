using System;

namespace MyLife.Models
{
    public interface IReportGenerationRequest
    {
        string ID { get; }

        string Name { get; }

        string Description { get; }

        DateTime StartTime { get; }
        
        DateTime EndTime { get; }

    }
}
