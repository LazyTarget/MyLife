using System;
using System.Collections.Generic;

namespace SteamLib.Models
{
    public class SteamReport : ISteamReport
    {
        private const string EventIDPrefix = "SteamReport_";
        private const string EventIDSuffix = "";

        public SteamReport()
        {
            Sessions = new List<GamingSession>();
        }


        public long UserID { get; set; }
        public long ID { get; set; }
        public long? FilterSetID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime LastModified { get; set; }
        public DateTime LastGenerated { get; set; }

        public ISteamReportFilterSet FilterSet { get; set; }

        public IList<GamingSession> Sessions { get; set; }


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
