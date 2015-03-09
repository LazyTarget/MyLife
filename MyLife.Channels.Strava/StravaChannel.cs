using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using com.strava.api.Authentication;
using com.strava.api.Clients;
using MyLife.Core;
using MyLife.Models;

namespace MyLife.Channels.Strava
{
    public class StravaChannel : IEventChannel
    {
        public static readonly Guid ChannelIdentifier = new Guid("1cb8ef41-030b-48b4-9ec4-ab131e08dc43");


        private StravaClient _client;
        private IAuthentication _auth;

        private StravaChannel()
        {

        }

        public StravaChannel(string accessToken)
            : this()
        {
            _auth = new StaticAuthentication(accessToken);
            _client = new StravaClient(_auth);
        }

        internal StravaChannel(IAuthentication authentication)
            : this()
        {
            _auth = authentication;
            _client = new StravaClient(_auth);
        }

        public Guid Identifier { get { return ChannelIdentifier; } }


        
        public async Task<IEnumerable<IEvent>> GetEvents(FeedArgs args)
        {
            var page = 1;
            var pageSize = 20;
            var events = await _client.Activities.GetActivitiesAsync(page, pageSize);
            if (events == null)
                return null;
            var result = events.Select(ModelConverter.ToEvent);
            return result;
        }

    }
}
