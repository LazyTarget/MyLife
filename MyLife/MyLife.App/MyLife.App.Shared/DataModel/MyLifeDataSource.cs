using System;
using System.Threading.Tasks;
using MyLife.App.ViewModels;
using MyLife.Core;
using MyLife.CoreClient;
using MyLife.Models;

namespace MyLife.App.Data
{
    /// <summary>
    /// Creates a collection of groups and items with content read from a static json file.
    /// 
    /// SampleDataSource initializes with data read from a static json file included in the 
    /// project.  This provides sample data at both design-time and run-time.
    /// </summary>
    public sealed class MyLifeDataSource
    {
        private static readonly MyLifeDataSource _instance;
        public static TestEventChannel _testChannel;

        static MyLifeDataSource()
        {
            _instance = new MyLifeDataSource();
        }


        public MyLifeDataSource()
        {
#if DEBUG
            //if (_testChannel == null)
            //    _testChannel = new RandomEventChannel();
#endif
        }

        private MyLifeClient _myLifeClient;

        public static bool IsAuthenticated { get { return _instance != null && _instance.User != null; } }

        public User User { get; private set; }
        
        public FeedViewModel FeedViewModel { get; private set; }


        public static async Task<User> AuthenticateUserAsync(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return null;

            _instance.User = new User
            {
                Username = username,
                Password = password,
            };


            try
            {
                //_instance._myLifeClient = new MyLifeClient();
                _instance._myLifeClient = await MyLifeClientFactory.Instance.AuthenticateUser(username, password);


                if (_testChannel != null)
                {
                    _instance._myLifeClient.AddChannel(new ChannelInfo
                    {
                        Channel = _testChannel,
                    });
                }
            }
            catch (Exception ex)
            {
                
            }

            return _instance.User;
        }


        public static async Task<FeedViewModel> GetFeedAsync(EventRequest request = null)
        {
            var feedEvents = await _instance._myLifeClient.GetEvents(request);

            var feed = new FeedViewModel(feedEvents);
            _instance.FeedViewModel = feed;
            return feed;
        }

    }
}