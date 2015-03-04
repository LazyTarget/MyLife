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
        private StravaClient _client;
        private IAuthentication _auth;

        public StravaChannel()
        {
            
        }

        public StravaChannel(string accessToken)
        {
            _auth = new StaticAuthentication(accessToken);
            _client = new StravaClient(_auth);
        }

        internal StravaChannel(IAuthentication authentication)
        {
            _auth = authentication;
            _client = new StravaClient(_auth);
        }
        
        public async Task<IEnumerable<IEvent>> GetEvents()
        {
            if (_auth == null)
            {
                var webAuth = new WebAuthentication();
                webAuth.AccessTokenReceived += delegate(object sender, TokenReceivedEventArgs args)
                {

                };
                webAuth.AuthCodeReceived += delegate(object sender, AuthCodeReceivedEventArgs args)
                {

                };
                //webAuth.AuthCode = "a15a2ff917413947e56ae5bdcff768b009c86ae4";
                webAuth.GetTokenAsync("5001", "4946b6e8deb7201f081793a049ab5bc3023cabac", Scope.Public);

                _auth = webAuth;
                _client = new StravaClient(_auth);
            }

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
