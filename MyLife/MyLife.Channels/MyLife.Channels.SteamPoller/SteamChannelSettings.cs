using System.Collections.Generic;
using MyLife.Core;

namespace MyLife.Channels.SteamPoller
{
    public class SteamChannelSettings : IChannelSettings
    {
        public SteamChannelSettings()
        {
            SteamUserIDs = new List<long>().ToArray();
        }

        public long MyLifeUserID { get; set; }

        public long[] SteamUserIDs { get; set; }

    }
}
