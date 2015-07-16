using System;

namespace MyLife.Models
{
    public interface IReportGenerationRequest
    {
        /// <summary>
        /// MyLife user id
        /// </summary>
        long UserID { get; set; }

        string PublicID { get; }

        string Name { get; }

        string Description { get; }

        DateTime StartTime { get; }
        
        DateTime EndTime { get; }

    }
}
