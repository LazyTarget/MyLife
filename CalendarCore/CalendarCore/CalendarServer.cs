using System.Collections.Generic;
using System.Threading.Tasks;

namespace CalendarCore
{
    public interface ICalendarServer
    {
        Task<Calendar> GetCalendar(string id);

        Task<IEnumerable<Calendar>> GetCalendars();
        
        Task<IEnumerable<Event>> GetEvents(string calendarID);

        Task<Event> CreateEvent(Event evt);

    }
}
