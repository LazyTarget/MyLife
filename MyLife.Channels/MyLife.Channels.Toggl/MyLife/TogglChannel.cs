using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyLife.Core;
using MyLife.Models;

namespace MyLife.Channels.Toggl
{
    public class TogglChannel : IEventChannel
    {
        public static readonly Guid ChannelIdentifier = new Guid("d9a8c13d-a3f6-4e58-92f0-e7687d21752f");


        private readonly global::Toggl.Toggl _toggl;

        
        public TogglChannel(string key)
        {
            _toggl = new global::Toggl.Toggl(key);
        }

        public Guid Identifier { get { return ChannelIdentifier; } }



        public async Task<IEnumerable<IEvent>> GetEvents(FeedArgs args)
        {
            var list = await _toggl.TimeEntry.List();
            var events = list.Select(ModelConverter.ToEvent);
            return events;
        }

    }
}
