using System;
using System.Collections.Generic;
using MyLife.Models;
using SteamPoller.Models;

namespace MyLife.Channels.SteamPoller
{
    public class SteamReport : IReport, ISteamReportGenerationRequest
    {
        private const string EventIDPrefix = "SteamReport_";
        private const string EventIDSuffix = "";

        public SteamReport()
        {
            Sessions = new List<GamingSession>();
            Filters = new List<SteamReportFilter>();
        }


        public long UserID { get; set; }
        public long ID { get; set; }
        public string PublicID { get { return GetPublicID(ID); } }
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
            var text = string.Format("Steam Report: '{0}' #{1}", Name, ID);
            var description = string.Format("Sessions: {0}", Sessions.Count);

            var evt = new Event
            {
                ID = PublicID,
                Text = text,
                Description = description,
                StartTime = StartTime,
                EndTime = EndTime,
                TimeCreated = LastGenerated,
                Source = ModelConverter.GetEventSource(),
            };
            return evt;
        }


        public static string GetPublicID(long id)
        {
            var publicID = string.Format("{0}{1}{2}", EventIDPrefix, id, EventIDSuffix);
            return publicID;
        }

        public static long ParsePublicID(string publicID)
        {
            long id;
            if (!long.TryParse(publicID, out id))
            {
                publicID = (publicID ?? "");
                if (publicID.StartsWith(EventIDPrefix, StringComparison.OrdinalIgnoreCase))
                    publicID = publicID.Remove(0, EventIDPrefix.Length);
                if (publicID.EndsWith(EventIDSuffix, StringComparison.OrdinalIgnoreCase))
                    publicID = publicID.Remove(publicID.Length - EventIDSuffix.Length, EventIDSuffix.Length);
                if (!long.TryParse(publicID, out id))
                    throw new FormatException(string.Format("Invalid '{0}' id format, Input: '{1}'", typeof(SteamReport).Name, publicID));
            }
            return id;
        }

    }
}
