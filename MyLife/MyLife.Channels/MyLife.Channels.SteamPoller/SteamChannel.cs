using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyLife.Core;
using MyLife.Models;
using SteamLib;
using SteamLib.Models;

namespace MyLife.Channels.SteamPoller
{
    public class SteamChannel : IEventChannel
    {
        public static readonly Guid ChannelIdentifier = new Guid("a45faa27-4f86-4b89-85d6-131c8233df15");

        
        private readonly ISteamManager _steamManager;
        private SteamChannelSettings _settings = new SteamChannelSettings();
        

        public SteamChannel(string connectionString)
        {
            _steamManager = new SteamManager(connectionString);
        }


        public Guid Identifier { get { return ChannelIdentifier; } }

        public SteamChannelSettings Settings
        {
            get { return _settings; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _settings = value;
            }
        }



        public async Task<IEnumerable<IEvent>> GetEvents(EventRequest request)
        {
            var result = new List<IEvent>();

            if (_steamManager.ActivityManager != null)
            {
                var sessions = await _steamManager.ActivityManager.GetGamingSessions(request.TimePeriod, Settings.SteamUserIDs);
                var events = sessions.Select(ModelConverter.ToEvent).ToList();
                result.AddRange(events);
            }

            if (_steamManager.ReportManager != null)
            {
                var reports = await _steamManager.ReportManager.GetReports(request.TimePeriod, Settings.MyLifeUserID);
                var reportEvents = reports.Select(ModelConverter.ToEvent).ToList();
                result.AddRange(reportEvents);
            }

            result = result.OrderBy(x => x.StartTime).ToList();
            return result;
        }
        

    }
}
