﻿using System;

namespace MyLife.Models
{
    public class EventSource : IEventSource
    {
        public string Name { get; set; }

        public string LogoUri { get; set; }

        public Guid ChannelIdentifier { get; set; }
    }
}
