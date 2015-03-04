using System.Threading.Tasks;
using MyLife.App.ViewModels;
using MyLife.Channels.Strava;
using MyLife.Channels.Toggl;
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
        private static readonly MyLifeDataSource _instance = new MyLifeDataSource();


        private Core.MyLife _myLife;


        public MyLifeDataSource()
        {
            _myLife = new Core.MyLife();

            
        }

        public static bool IsAuthenticated { get { return _instance.User != null; } }

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


            _instance._myLife = new Core.MyLife();

            var togglChannel = new TogglChannel("55e7ecf6095ea0d3d9d6af8aadc0fe00");
            _instance._myLife.AddChannel(togglChannel);

            //var outlookClient = new OutlookClient();
            //var accessToken = "EwCAAq1DBAAUGCCXc8wU/zFu9QnLdZXy+YnElFkAARaf0nPYTZgxz8qoYngiCALujrYCaZMjrLJif/HaalWM+OX+8bn10CvxjfTdTdl2kvj/9wo3nlADN4n0pqktHLo4/YzXIAy7pLQj4twdL+SBSc6PgohHkPNQRR9QdNxhLswsQMwBc1sa91DWW177IoiS7vjly5sMcT8YhDucUUHc7zwspV30fmqgzewTDCr4z7KwkwclTpf+ZGIhpqmQ18aiQGXBqy3w4Ohx/PstenGWfcvbAtjW8Rxob6NJDWe3zXcsM90bDQqd6WCjTLlYCjL0q2oMsj+KEE03RFl6d08bZF5eOrdD3/I+zRckLNe8V0n8oA4BQqAPbtQi6n1Q2YUDZgAACNyvuYVoqP3qUAEfct16JdAhGLPyEkJ2QPXjzDLi3Kig33w1xuPnF+wETblY91XygJu7SckKrRx/eRzRwVVAitxRiYnBLqMC8t9LGk+/zwz+pFNHPiNDWMGykbNKZNZYAjylelKYdJjxL9Gckmc3y7Z57Bzg9G9obV405O9nzzDQRsdh/qSWWjZn34hUlump+/JDnS21jMv9MX4S316cYr/UAL9nNR9Co/LKBdSK0yJJkNDe+BUiJDslc3+Hh3nmWp5+5VtqOIdfCEHEC+1yMjkcPgG2H4QPoNlaz/2lsrQ61AXvHKAcgmwzy48YMmcGIlHVxGUQnr4igJx1tPaH0TMO0GkFislG99SGUFP+PqX2LBo8HouIpIPleetOxEsZNhvJSU+i4K/UTllTDu9gsRh3XIaTCwYCdTkQ2ymVNQTRoX+isxVli4HYF8aO7H+8xjAhLxtnQQeieYxtAQ==";
            //outlookClient.SetAccessToken(accessToken);
            //var calendarChannel = new CalendarChannel(outlookClient);
            //_myLife.AddChannel(calendarChannel);


            var authCode = "64afcf881d24e7b13aae2c1cd5e2df9f9616090a";
            var accessToken = "a15a2ff917413947e56ae5bdcff768b009c86ae4";
            var stravaChannel = new StravaChannel(accessToken);
            _instance._myLife.AddChannel(stravaChannel);

            return _instance.User;
        }


        public static async Task<FeedViewModel> GetFeedAsync()
        {
            var feedEvents = await _instance._myLife.GetEvents();

            var feed = new FeedViewModel(feedEvents);
            _instance.FeedViewModel = feed;
            return feed;
        }

    }
}