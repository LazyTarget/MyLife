using System.Collections.Generic;
using System.Threading.Tasks;
using MyLife.Models;

namespace MyLife.Core
{
    public interface IReportChannel : IChannel
    {
        /// <summary>
        /// Gets a report
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<IReport> GetReport(string id);


        /// <summary>
        /// Creates or updates a report
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<IReport> GenerateReport(IReportGenerationRequest request);

        /// <summary>
        /// Gets existing reports
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<IEnumerable<IReport>> GetReports(EventRequest request);
    }
}
