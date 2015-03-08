using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyLife.Models;

namespace MyLife.Core
{
    public class MyLifeClient
    {
        private readonly List<IChannel> _channels = new List<IChannel>();
        

        public void AddChannel(IChannel channel)
        {
            if (channel == null)
                throw new ArgumentNullException("channel");
            //if (_channels.Any(x => x.GetType() == channel.GetType()))
            //    throw new InvalidOperationException("Channel of type '" + channel.GetType() + "' already added");
            _channels.Add(channel);
        }


        public async Task<IEnumerable<IEvent>> GetEvents()
        {
            var events = new List<IEvent>();
            foreach (var eventChannel in _channels.OfType<IEventChannel>())
            {
                var e = await eventChannel.GetEvents();
                if (e != null)
                {
                    var list = e.ToList();
                    events.AddRange(list);
                }
            }

            events = events.OrderByDescending(x => x.StartTime)
                           .ThenBy(x => x.EndTime)
                           .ToList();
            return events;
        }

    }
}
