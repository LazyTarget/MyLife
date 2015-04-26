using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyLife.Core;
using MyLife.Models;

namespace MyLife.CoreClient
{
    public class TestEventChannel : IEventChannel
    {
        private bool _hasPopulated;

        public TestEventChannel()
        {
            Events = new List<IEvent>();
        }

        public List<IEvent> Events { get; set; }

        public virtual string ChannelName { get { return "TestChannel"; } }

        public Guid Identifier { get { return Guid.NewGuid(); } }


        public async Task<IEnumerable<IEvent>> GetEvents(EventRequest request)
        {
            if (!_hasPopulated)
            {
                PopulateEvents();
                _hasPopulated = true;
            }

            var res = Events.Select(x =>
            {
                if (x != null && x.Source == null)
                {
                    x.Source = new EventSource
                    {
                        ChannelIdentifier = Identifier,
                        Name = ChannelName,
                    };
                }
                return x;
            });
            return res;
        }


        protected virtual void PopulateEvents()
        {
            
        }

    }
}
