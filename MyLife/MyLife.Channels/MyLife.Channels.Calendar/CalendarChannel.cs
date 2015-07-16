using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalendarCore;
using MyLife.Core;
using MyLife.Models;

namespace MyLife.Channels.Calendar
{
    public class CalendarChannel : IEventChannel
    {
        public static readonly Guid ChannelIdentifier = new Guid("447da262-e9a0-4075-879c-490131a7940b");

        private readonly ICalendarServer _calendarServer;
        private string _defaultCalendarID;

        public CalendarChannel(ICalendarServer calendarServer)
        {
            this._calendarServer = calendarServer;
            Settings = new CalendarChannelSettings();
        }

        public Guid Identifier { get { return ChannelIdentifier; } }

        public CalendarChannelSettings Settings { get; set; }


        public async Task<IEnumerable<IEvent>> GetEvents(EventRequest request)
        {
            if (string.IsNullOrEmpty(_defaultCalendarID))
            {
                // todo: remove test code
                _defaultCalendarID = "calendar.3ae2a03e43b87c56.b3eab7507ea5429d9015a169b8dfcd19";
                //return null;
            }

            var events = await _calendarServer.GetEvents(_defaultCalendarID);
            if (events == null)
                return null;
            var result = events.Select(ModelConverter.ToEvent);
            if (!Settings.GetFutureEventsInFeed)
            {
                result = result.Where(x => x.StartTime <= DateTime.UtcNow);
            }
            return result;
        }

    }
}
