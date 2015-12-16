using System.Threading.Tasks;

namespace ProcessLib.Interfaces
{
    public interface IProcessMonitor
    {
        string MachineName { get; }
        Task Init();
        Task Work();
        Task Stop();
    }
}
