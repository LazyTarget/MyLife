using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyLife.Core;
using MyLife.Models;
using OdbcWrapper;
using SteamPoller.Models;

namespace MyLife.Channels.SteamPoller
{
    public class SteamChannel : IEventChannel
    {
        public static readonly Guid ChannelIdentifier = new Guid("a45faa27-4f86-4b89-85d6-131c8233df15");

        
        public Guid Identifier { get { return ChannelIdentifier; } }

        public SteamChannelSettings Settings { get; set; }


        public SteamChannel(string connectionString)
        {
            _odbc = new OdbcClient(connectionString);
        }

        private readonly OdbcClient _odbc;



        public async Task<IEnumerable<IEvent>> GetEvents(EventRequest request)
        {
            var events = await GetGamingSessions();
            return events;
        }



        private async Task<IEnumerable<IEvent>> GetGamingSessions()
        {
            var sql = "SELECT * FROM Steam_GamingSessions " +
                      "WHERE UserID = " + Settings.UserID;
            var dataTable = await _odbc.ExecuteReader(sql);
            var rows = dataTable.Rows.ToList();
            var dynamic = rows.Select(x => x.ToExpando()).ToList();
            var sessions = dynamic.Select(x => x.To<GamingSession>()).ToList();
            var result = sessions.Select(ModelConverter.ToEvent).ToList();
            return result;
        }


    }
}
