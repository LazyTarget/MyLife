using System;
using System.Collections.Generic;
using System.Linq;
using MyLife.Models;
using SteamPoller.Models;

namespace MyLife.Channels.SteamPoller
{
    public class SteamReport : IReport, ISteamReportGenerationRequest
    {
        public SteamReport()
        {
            Sessions = new List<GamingSession>();
            Filters = new List<SteamReportFilter>();
        }


        public long ID { get; set; }
        string IReport.ID { get { return ID.ToString(); } }
        string IReportGenerationRequest.ID { get { return ID.ToString(); } }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime LastModified { get; set; }
        public DateTime LastGenerated { get; set; }

        public IList<SteamReportFilter> Filters { get; set; }

        public IList<GamingSession> Sessions { get; set; }


        public IEvent ToEvent()
        {
            var text = "Mapped sessions: " + Sessions.Count();
            var description = "";

            var evt = new Event
            {
                ID = "steamreport_" + ID,
                Text = text,
                Description = description,
                StartTime = StartTime,
                EndTime = EndTime,
                Source = ModelConverter.GetEventSource(),
            };
            return evt;
        }
    }
}
