using System;

namespace MyLife.Core
{
    public interface IChannel
    {
        Guid Identifier { get; }

        //IChannelSettings Settings { get; }
    }
}
