using System.Collections.Generic;
using System.Threading.Tasks;
using MyLife.Models;

namespace MyLife.Core
{
    public interface IEventChannel : IChannel
    {
        Task<IEnumerable<IEvent>> GetEvents();
    }
}
