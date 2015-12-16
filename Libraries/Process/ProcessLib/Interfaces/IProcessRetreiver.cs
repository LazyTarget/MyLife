using System.Collections.Generic;

namespace ProcessLib.Interfaces
{
    public interface IProcessRetriever
    {
        string MachineName { get; }
        System.Diagnostics.Process GetProcessById(int processId);
        IEnumerable<System.Diagnostics.Process> GetProcesses();
    }
}
