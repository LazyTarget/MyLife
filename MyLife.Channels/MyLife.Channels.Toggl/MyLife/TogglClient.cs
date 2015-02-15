using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyLife.Core;
using MyLife.Models;

namespace MyLife.Channels.Toggl
{
    public class TogglClient : IEventChannel
    {
        private readonly global::Toggl.Toggl toggl;

        
        public TogglClient(string key)
        {
            toggl = new global::Toggl.Toggl(key);
        }


        public async Task<IEnumerable<IEvent>> GetEvents()
        {
            var list = await toggl.TimeEntry.List();
            var events = list.Select(ModelConverter.ToEvent);
            return events;
        }

    }
}
