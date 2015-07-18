using System.Collections.Generic;
using System.Threading.Tasks;
using SharedLib;

namespace SteamLib.Models
{
    public interface ISteamReportManager
    {
        /// <summary>
        /// Gets a report
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<ISteamReport> GetReport(long id);


        /// <summary>
        /// Creates or updates a report
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<ISteamReport> GenerateReport(ISteamReportGenerationRequest request);


        /// <summary>
        /// Gets existing reports
        /// </summary>
        /// <param name="timePeriod"></param>
        /// <returns></returns>
        Task<IEnumerable<ISteamReport>> GetReports(TimePeriod timePeriod);


        /// <summary>
        /// Gets existing reports
        /// </summary>
        /// <param name="timePeriod"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        Task<IEnumerable<ISteamReport>> GetReports(TimePeriod timePeriod, long userID);

    }
}
