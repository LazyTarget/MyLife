using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyLife.Models;

namespace MyLife.Core
{
    public class MyLife
    {
        private readonly List<IEventChannel> _eventChannels = new List<IEventChannel>();
        

        public void AddChannel(IEventChannel channel)
        {
            if (channel == null)
                throw new ArgumentNullException("channel");
            //if (_eventChannels.Any(x => x.GetType() == channel.GetType()))
            //    throw new InvalidOperationException("Channel of type '" + channel.GetType() + "' already added");
            _eventChannels.Add(channel);
        }


        public async Task<IEnumerable<IEvent>> GetEvents()
        {
            var events = new List<IEvent>();
            foreach (var eventChannel in _eventChannels)
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
