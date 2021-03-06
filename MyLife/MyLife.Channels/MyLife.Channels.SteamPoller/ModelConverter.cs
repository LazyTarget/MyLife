﻿using System;
using MyLife.Models;
using SteamLib.Models;

namespace MyLife.Channels.SteamPoller
{
    public static class ModelConverter
    {
        public static IEventSource GetEventSource()
        {
            var source = new EventSource
            {
                Name = "Steam",
                ChannelIdentifier = SteamChannel.ChannelIdentifier,
            };
            return source;
        }
        
        public static IEvent ToEvent(GamingSession obj)
        {
            var durationStr = "";
            if (obj.Duration > TimeSpan.Zero)
            {
                if (obj.Duration.TotalMinutes < 1)
                    durationStr += string.Format(" for {0} seconds", obj.Duration.Seconds);
                else if (obj.Duration.TotalHours < 1)
                    durationStr += string.Format(" for {0}m {1}s", obj.Duration.Minutes, obj.Duration.Seconds);
                else
                    durationStr += string.Format(" for {0}h {1}m", obj.Duration.Hours, obj.Duration.Minutes);
            }

            var t = obj.Active ? "are playing" : "played";
            var text = string.Format("You {0} {1}{2}", t, obj.GameName, durationStr);        // todo: translation support
            var desc = "You unlocked following achievements... (todo)";

            var res = new Event
            {
                ID = string.Format("gamingsession_{0}", obj.ID),
                Text = text,
                Description = desc,
                StartTime = obj.StartTime,
                EndTime = obj.EndTime,
                Source = GetEventSource(),
            };
            return res;
        }

        public static IEvent ToEvent(ISteamReport obj)
        {
            var text = string.Format("Steam Report: '{0}' #{1}", obj.Name, obj.ID);
            var desc = string.Format("Sessions: {0}", obj.Sessions.Count);

            var res = new Event
            {
                ID = SteamReport.GetPublicID(obj.ID),
                Text = text,
                Description = desc,
                StartTime = obj.StartTime,
                EndTime = obj.EndTime,
                Source = GetEventSource(),
            };
            return res;
        }

    }
}
