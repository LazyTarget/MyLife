using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toggl.DataObjects;
using Toggl.QueryObjects;

namespace Toggl.Interfaces
{
    public interface IReportService
    {
        IApiService ToggleSrv { get; set; }

        Task<DetailedReport> Detailed(DetailedReportParams requestParameters);
    }
}
