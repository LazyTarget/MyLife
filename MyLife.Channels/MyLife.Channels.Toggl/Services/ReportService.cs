using System.Threading.Tasks;
using Toggl.DataObjects;
using Toggl.Interfaces;
using Toggl.QueryObjects;

namespace Toggl.Services
{
    public class ReportService : IReportService
    {
        public IApiService ToggleSrv { get; set; }
        
        public async Task<DetailedReport> Detailed(DetailedReportParams requestParameters)
        {
            var report = await ToggleSrv.Get<DetailedReport>(ApiRoutes.Reports.Detailed, requestParameters.ToKeyValuePair());
            return report;
        }

	    public async Task<DetailedReport> FullDetailedReport(DetailedReportParams requestParameters)
	    {
		    var report = await this.Detailed(requestParameters);

		    if (report.TotalCount < report.PerPage)
			    return report;

			var pageCount = (report.TotalCount + report.PerPage - 1) / report.PerPage;

		    DetailedReport resultReport = null;
			for (var page = 1; page <= pageCount; page++)
			{
				requestParameters.Page = page;
				var pagedReport = await Detailed(requestParameters);

				if (resultReport == null)
				{
					resultReport = pagedReport;
				}
				else
				{
					resultReport.Data.AddRange(pagedReport.Data);
				}
		    }

		    return resultReport;

	    }

	    public ReportService(string apiKey)
            : this(new ApiService(apiKey))
        {

        }
        
        public ReportService(IApiService srv)
        {
            ToggleSrv = srv;
        }        
    }
}
