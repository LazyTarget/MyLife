using System.Collections.Generic;
using MyLife.Models;

namespace MyLife.Channels.SteamPoller
{
    public interface ISteamReportGenerationRequest : IReportGenerationRequest
    {
        IList<SteamReportFilter> Filters { get; } 

    }
}
