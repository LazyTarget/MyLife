using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ProcessLib.Models;

namespace ProcessLib.Interfaces
{
    public interface IProcessRepository
    {
        Task<IEnumerable<Process>> GetProcesses(Expression<Func<Process, bool>> filter);
        Task<Process> UpdateProcess(Process process);
        Task<ProcessTitle> UpdateProcessTitle(ProcessTitle processTitle);
    }
}
