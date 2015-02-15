using System.Collections.Generic;
using System.Threading.Tasks;
using MyLife.Models;

namespace MyLife.Core
{
    public interface IEventChannel
    {
        Task<IEnumerable<IEvent>> GetEvents();
    }
}
