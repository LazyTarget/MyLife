using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MyLife.Models;

namespace MyLife.App.ViewModels
{
    public class FeedViewModel
    {
        public FeedViewModel()
            : this(new Event[0])
        {
            
        }

        public FeedViewModel(IEnumerable<IEvent> events)
        {
            var items = events.Select(x => new FeedItem(x));
            Items = new ObservableCollection<FeedItem>(items);

            Title = "My feed";
        }
        
        public string Title { get; set; }

        public ObservableCollection<FeedItem> Items { get; private set; }
    }


    public class FeedItem
    {
        private readonly IEvent _event;

        public FeedItem(IEvent evt)
        {
            _event = evt;
        }

        public string ID { get { return _event.ID; } }
        public string Title { get { return _event.Text; } }
        public string Description { get { return _event.Description; } }
        public DateTime StartTime { get { return _event.StartTime; } }
        public DateTime EndTime { get { return _event.EndTime; } }
        public IEventSource Source { get { return _event.Source; } }

        public override string ToString()
        {
            return this.Title;
        }
    }
}