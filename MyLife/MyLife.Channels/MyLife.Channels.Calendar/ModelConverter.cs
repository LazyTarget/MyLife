using MyLife.Models;

namespace MyLife.Channels.Calendar
{
    public static class ModelConverter
    {
        private static IEventSource GetEventSource()
        {
            var source = new EventSource
            {
                Name = "Outlook.com",
            };
            return source;
        }


        public static IEvent ToEvent(CalendarCore.Event obj)
        {
            var res = new Event
            {
                ID = obj.ID,
                Text = obj.Name,
                Description = obj.Description,
                StartTime = obj.Start,
                EndTime = obj.End,
                
                Source = GetEventSource(),
            };
            return res;
        }

    }
}
