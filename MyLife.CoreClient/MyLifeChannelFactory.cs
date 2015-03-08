using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using CalendarCore.Outlook;
using MyLife.Channels.Calendar;
using MyLife.Channels.Odbc;
using MyLife.Channels.Strava;
using MyLife.Channels.Toggl;
using MyLife.Core;
using MyLife.Models;
using OdbcWrapper;

namespace MyLife.CoreClient
{
    public class MyLifeChannelFactory
    {
        public static readonly MyLifeChannelFactory Instance = new MyLifeChannelFactory();


        private readonly OdbcClient _odbc;
        private readonly Dictionary<Guid, Type> _channels; 

        public MyLifeChannelFactory()
            : this(Config.OdbcConnectionString)
        {
            _channels = GetChannelTypes();
        }

        public MyLifeChannelFactory(string connectionString)
        {
            _odbc = new OdbcClient(connectionString);
        }


        private static Dictionary<Guid, Type> GetChannelTypes()
        {
            var channels = new Dictionary<Guid, Type>();
            channels.Add(TogglChannel.ChannelIdentifier, typeof (TogglChannel));
            channels.Add(CalendarChannel.ChannelIdentifier, typeof (CalendarChannel));
            channels.Add(StravaChannel.ChannelIdentifier, typeof (StravaChannel));
            channels.Add(OdbcChannel.ChannelIdentifier, typeof (OdbcChannel));
            return channels;
        }


        public async Task<IEnumerable<IChannel>> GetUserChannels(User user)
        {
            var result = new List<IChannel>();
            var getChannelsSql = string.Format("SELECT C.Identifier, Cr.* " +
                                    "FROM CrUserChannels Cr " +
                                    "INNER JOIN Channels C ON C.ID = Cr.ChannelID " +
                                    "WHERE UserID = {0}",
                                    user.ID);
            
            var channelDataTable = await _odbc.ExecuteReader(getChannelsSql);
            foreach (var row in channelDataTable.Rows)
            {
                var channelData = row.ToExpando();

                ExpandoObject settingsData = null;
                var channelSettingsID = channelData.Get<long>("ChannelSettingsID");
                if (channelSettingsID > 0)
                {
                    var getSettingsDataSql = string.Format("SELECT * " +
                                                           "FROM ChannelSettings " +
                                                           "WHERE ID = {0}",
                        channelSettingsID);
                    var channelSettingsTable = await _odbc.ExecuteReader(getSettingsDataSql);
                    if (channelSettingsTable != null && channelSettingsTable.RowCount > 0)
                    {
                        var r = channelSettingsTable.Rows.First();
                        settingsData = r.ToExpando();
                    }
                }

                var channel = await Create(channelData, settingsData);
                result.Add(channel);
            }
            return result;
        }


        private async Task<IChannel> Create(ExpandoObject channelData, ExpandoObject settingsData)
        {
            var identifier = channelData.Get<Guid>("Identifier");
            if (identifier == Guid.Empty)
                throw new ArgumentException("Invalid channel identifier");

            if (identifier == TogglChannel.ChannelIdentifier)
            {
                var apiToken = channelData.Get<string>("AccessToken");
                var togglChannel = new TogglChannel(apiToken);
                return togglChannel;
            }
            if (identifier == CalendarChannel.ChannelIdentifier)
            {
                var refreshToken = channelData.Get<string>("RefreshToken");
                var accessToken = await OutlookClient.GetAccessTokenFromRefreshToken(refreshToken);
                var outlookClient = new OutlookClient(accessToken);
                var calendarChannel = new CalendarChannel(outlookClient);
                return calendarChannel;
            }
            if (identifier == StravaChannel.ChannelIdentifier)
            {
                var accessToken = channelData.Get<string>("AccessToken");
                var stravaChannel = new StravaChannel(accessToken);
                return stravaChannel;
            }
            if (identifier == OdbcChannel.ChannelIdentifier)
            {
                var connectionString = channelData.Get<string>("ConnectionString");
                var odbcChannel = new OdbcChannel(connectionString);
                odbcChannel.Settings = settingsData.To<OdbcChannelSettings>();
                return odbcChannel;
            }
            throw new NotImplementedException("Channel not yet implemented");
        }
        

    }
}
