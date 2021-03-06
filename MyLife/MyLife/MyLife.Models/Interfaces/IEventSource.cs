﻿using System;

namespace MyLife.Models
{
    public interface IEventSource
    {
        string Name { get; set; }

        string LogoUri { get; set; }

        Guid ChannelIdentifier { get; set; }

    }
}
