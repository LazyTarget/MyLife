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
        /// <param name="timeRange"></param>
        /// <returns></returns>
        Task<IEnumerable<ISteamReport>> GetReports(TimeRange timeRange);


        /// <summary>
        /// Gets existing reports
        /// </summary>
        /// <param name="timeRange"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        Task<IEnumerable<ISteamReport>> GetReports(TimeRange timeRange, long userID);


        /// <summary>
        /// Re-generates the report
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task RefreshReport(long id);
        

        /// <summary>
        /// Re-generates the report
        /// </summary>
        /// <param name="report"></param>
        /// <returns></returns>
        Task RefreshReport(ISteamReport report);


        /// <summary>
        /// Gets report tempalate
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<ISteamReportTemplate> GetReportTemplate(long id);


        /// <summary>
        /// Gets report subscriptions
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ISteamReportSubscription>> GetReportSubscriptions();

        /// <summary>
        /// Gets report subscriptions for user
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        Task<IEnumerable<ISteamReportSubscription>> GetReportSubscriptions(long userID);

    }
}
