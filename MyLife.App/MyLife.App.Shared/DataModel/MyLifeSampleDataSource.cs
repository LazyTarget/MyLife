using System;
using MyLife.App.ViewModels;
using MyLife.Models;

namespace MyLife.App.Data
{
    public class MyLifeSampleDataSource
    {
        public MyLifeSampleDataSource()
        {
            FeedViewModel = new FeedViewModel();
            FeedViewModel.Title = "My feed";

            IEventSource myLifeEventSource = new EventSource
            {
                Name = "MyLife",
            };
            IEventSource outlookEventSource = new EventSource
            {
                Name = "outlook.com",
            };
            IEventSource togglEventSource = new EventSource
            {
                Name = "Toggl",
            };


            FeedViewModel.Items.Add(new FeedItem(new Event
            {
                ID = "1",
                Text = "Became a MyLife member",
                StartTime = new DateTime(2015, 2, 19, 8, 23, 0),
                EndTime   = new DateTime(2015, 2, 19, 8, 23, 0),
                Source = myLifeEventSource,
            }));

            FeedViewModel.Items.Add(new FeedItem(new Event
            {
                ID = "2",
                Text = "Connected Toggl channel",
                StartTime = new DateTime(2015, 2, 19, 8, 33, 0),
                EndTime = new DateTime(2015, 2, 19, 8, 33, 0),
                Source = myLifeEventSource,
            }));

            FeedViewModel.Items.Add(new FeedItem(new Event
            {
                ID = "toggl.1",
                Text = "Programming",
                Description = "Working with MyLife",
                StartTime = new DateTime(2015, 2, 19, 8, 0, 0),
                EndTime = new DateTime(2015, 2, 19, 9, 50, 0),
                Source = togglEventSource,
            }));

            FeedViewModel.Items.Add(new FeedItem(new Event
            {
                ID = "3",
                Text = "Connected Outlook.com channel",
                StartTime = new DateTime(2015, 2, 19, 8, 35, 0),
                EndTime = new DateTime(2015, 2, 19, 8, 35, 0),
                Source = myLifeEventSource,
            }));

            FeedViewModel.Items.Add(new FeedItem(new Event
            {
                ID = "outlook.1",
                Text = "Lunch",
                Description = "Trankil",
                StartTime = new DateTime(2015, 2, 19, 12, 0, 0),
                EndTime = new DateTime(2015, 2, 19, 13, 0, 0),
                Source = outlookEventSource,
            }));
        }

        public FeedViewModel FeedViewModel { get; private set; }

    }
}