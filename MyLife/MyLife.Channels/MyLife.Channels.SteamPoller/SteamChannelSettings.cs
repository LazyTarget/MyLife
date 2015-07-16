using System.Collections.Generic;
using MyLife.Core;

namespace MyLife.Channels.SteamPoller
{
    public class SteamChannelSettings : IChannelSettings
    {
        public SteamChannelSettings()
        {
            SteamUserIDs = new List<long>();
        }

        public long MyLifeUserID { get; set; }

        public List<long> SteamUserIDs { get; set; }

    }
}
