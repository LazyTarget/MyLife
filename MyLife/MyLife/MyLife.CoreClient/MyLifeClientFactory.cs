using System;
using System.Linq;
using System.Threading.Tasks;
using MyLife.Core;
using MyLife.Models;
using OdbcWrapper;

namespace MyLife.CoreClient
{
    public class MyLifeClientFactory
    {
        public static readonly MyLifeClientFactory Instance = new MyLifeClientFactory();
        

        private readonly OdbcClient _odbc;
        private readonly MyLifeChannelFactory _channelFactory;

        public MyLifeClientFactory()
            : this(Config.OdbcConnectionString)
        {

        }

        private MyLifeClientFactory(string connectionString)
        {
            _odbc = new OdbcClient(connectionString);
            //_channelFactory = MyLifeChannelFactory.Instance;
            _channelFactory = new MyLifeChannelFactory(connectionString);
        }


        public async Task<MyLifeClient> AuthenticateUser(string username, string password)
        {
            User user = null;
            var sql = string.Format("SELECT * FROM Users WHERE Username = '{0}' AND Password = '{1}'", username, password);
            var dataTable = await _odbc.ExecuteReader(sql);
            if (dataTable != null && dataTable.RowCount > 0)
            {
                var row = dataTable.Rows.First();
                user = row.To<User>();
            }

            if (user != null)
            {
                var client = new MyLifeClient(_odbc);

                var channels = await _channelFactory.GetUserChannels(user);
                foreach (var channel in channels)
                {
                    client.AddChannel(channel);
                }
                return client;
            }
            else
                throw new Exception("Could not authenticate user");
        }
        

    }
}
