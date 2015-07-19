using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyLife.Models;
using OdbcWrapper;
using SharedLib;

namespace MyLife.Core
{
    public class MyLifeClient
    {
        private readonly OdbcClient _odbc;
        private readonly List<ChannelInfo> _channels = new List<ChannelInfo>();


        public MyLifeClient()
        {
            
        }

        public MyLifeClient(OdbcClient odbc)
        {
            _odbc = odbc;
        }


        public void AddChannel(ChannelInfo channel)
        {
            if (channel == null)
                throw new ArgumentNullException("channel");
            //if (_channels.Any(x => x.GetType() == channel.GetType()))
            //    throw new InvalidOperationException("Channel of type '" + channel.GetType() + "' already added");
            _channels.Add(channel);
        }


        public async Task<IEnumerable<IEvent>> GetEvents(EventRequest request)
        {
            if (request == null)
            {
                var days = 7;
                request = new EventRequest
                {
                    TimeRange = new TimeRange
                    {
                        StartTime = DateTime.Now.AddDays(-days),
                        EndTime = DateTime.Now.AddDays(days),
                    },
                };
            }


            var events = new List<IEvent>();
            foreach (var channelInfo in _channels)
            {
                var eventChannel = channelInfo.Channel as IEventChannel;
                if (eventChannel == null)
                    continue;
                var e = await eventChannel.GetEvents(request);
                if (e != null)
                {
                    e = e.Select(x =>
                    {
                        x.Source = new EventSource();
                        x.Source.ChannelIdentifier = eventChannel.Identifier;
                        return x;
                    });
                    var list = await Task.WhenAll(GetModifiedEvents(e, channelInfo));
                    list = list.Where(x => x != null).ToArray();
                    //list = list.Where(x => x.StartTime >= request.StartTime && x.EndTime <= request.EndTime).ToArray();
                    events.AddRange(list);
                }
            }

            events = events.OrderByDescending(x => x.StartTime)
                           .ThenBy(x => x.EndTime)
                           .ToList();
            return events;
        }


        private IEnumerable<Task<IEvent>> GetModifiedEvents(IEnumerable<IEvent> events, ChannelInfo channelInfo)
        {
            foreach (var evt in events)
            {
                var task = GetModifiedEvent(evt, channelInfo);
                yield return task;
            }
        }


        private async Task<IEvent> GetModifiedEvent(IEvent evt, ChannelInfo channelInfo)
        {
            if (_odbc == null)
                return evt;
            if (evt is ModifiedEvent)
                return evt;

            IEvent result = null;

            // todo: make as a MyLife user setting
            //var sql = string.Format("SELECT * FROM ModifiedEvents WHERE ID = '{0}' AND UserChannelID = '{1}'", evt.ID, channelInfo.UserChannelID);
            //var dt = await _odbc.ExecuteReader(sql);
            //if (dt != null && dt.RowCount > 0)
            //{
            //    foreach (var dataRow in dt.Rows)
            //    {
            //        var res = dataRow.To<ModifiedEvent>();
            //        if (res != null)
            //        {
            //            res.OriginalEvent = evt;
            //            result = res;
            //            break;
            //        }
            //    }
            //}
            if (result == null)
                result = evt;
            return result;
        }

    }
}
