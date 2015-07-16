using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using CalendarCore.Outlook;
using MyLife.Channels.Calendar;
using MyLife.Channels.Odbc;
using MyLife.Channels.SteamPoller;
using MyLife.Channels.Strava;
using MyLife.Channels.Toggl;
using MyLife.Core;
using MyLife.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            channels.Add(SteamChannel.ChannelIdentifier, typeof (SteamChannel));
            return channels;
        }


        public async Task<IEnumerable<ChannelInfo>> GetUserChannels(User user)
        {
            var result = new List<ChannelInfo>();
            var getChannelsSql = string.Format("SELECT C.Identifier, Cr.* " +
                                    "FROM UserChannels Cr " +
                                    "INNER JOIN Channels C ON C.ID = Cr.ChannelID " +
                                    "WHERE Cr.UserID = {0} AND Cr.Enabled = 1 AND C.Enabled = 1 ",
                                    user.ID);
            
            var channelDataTable = await _odbc.ExecuteReader(getChannelsSql);
            foreach (var row in channelDataTable.Rows)
            {
                var channelData = row.ToExpando();

                JObject customSettingsJson = null;
                var channelSettingsID = channelData.Get<long>("ChannelSettingsID");
                if (channelSettingsID > 0)
                {
                    var settingsData = await GetChannelSettings(channelSettingsID);
                    settingsData.Remove("ID");
                    channelData.Extend(settingsData);
                }

                var channel = await Create(channelData);
                var channelInfo = new ChannelInfo
                {
                    Channel = channel,
                    UserChannelID = channelData.Get<long>("ID"),
                };
                result.Add(channelInfo);
            }
            return result;
        }
        
        private async Task<ExpandoObject> GetChannelSettings(long channelSettingsID)
        {
            ExpandoObject res = null;
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
                    res = r.ToExpando();
                }
            }
            return res;
        }


        private async Task<IChannel> Create(ExpandoObject channelData)
        {
            var customSettingsJson = channelData.Get<string>("Settings");
            if (string.IsNullOrWhiteSpace(customSettingsJson))
                customSettingsJson = "{}";
            var customSettings = JObject.Parse(customSettingsJson);


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
                odbcChannel.Settings = customSettings.ToObjectOrDefault<OdbcChannelSettings>() ?? new OdbcChannelSettings();
                return odbcChannel;
            }
            if (identifier == SteamChannel.ChannelIdentifier)
            {
                var settings = customSettings.ToObjectOrDefault<SteamChannelSettings>() ?? new SteamChannelSettings();
                var json = JsonConvert.SerializeObject(settings, Formatting.Indented);

                var connectionString = channelData.Get<string>("ConnectionString");
                var steamChannel = new SteamChannel(connectionString);
                steamChannel.Settings = settings;
                return steamChannel;
            }
            throw new NotImplementedException("Channel not yet implemented");
        }
        

    }
}
