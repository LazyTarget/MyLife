using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyLife.Core;
using MyLife.Models;

namespace MyLife.Channels.Toggl
{
    public class TogglChannel : IEventChannel
    {
        private readonly global::Toggl.Toggl _toggl;

        
        public TogglChannel(string key)
        {
            _toggl = new global::Toggl.Toggl(key);
        }


        public async Task<IEnumerable<IEvent>> GetEvents()
        {
            var list = await _toggl.TimeEntry.List();
            var events = list.Select(ModelConverter.ToEvent);
            return events;
        }

    }
}
