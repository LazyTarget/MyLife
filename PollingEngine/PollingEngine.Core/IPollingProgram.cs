using System.Threading.Tasks;

namespace PollingEngine.Core
{
    public interface IPollingProgram
    {
        Task OnStarting(PollingContext context);
        Task OnInterval(PollingContext context);
        Task OnStopping(PollingContext context);
        void ApplyArguments(string[] args);
    }
}
