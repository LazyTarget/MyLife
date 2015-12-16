using System;
using System.Collections.Generic;
using ProcessLib.Interfaces;

namespace ProcessLib
{
    public class ProcessRetriever : IProcessRetriever
    {
        public ProcessRetriever()
        {
            MachineName = Environment.MachineName;
        }

        public ProcessRetriever(string machineName)
            : this()
        {
            if (!string.IsNullOrWhiteSpace(machineName))
                MachineName = machineName;
        }


        public string MachineName { get; set; }


        public virtual System.Diagnostics.Process GetProcessById(int processId)
        {
            System.Diagnostics.Process res;
            if (!string.IsNullOrWhiteSpace(MachineName) && MachineName != Environment.MachineName)
                res = System.Diagnostics.Process.GetProcessById(processId, MachineName);
            else
                res = System.Diagnostics.Process.GetProcessById(processId);
            return res;
        }

        public virtual IEnumerable<System.Diagnostics.Process> GetProcesses()
        {
            System.Diagnostics.Process[] res;
            if (!string.IsNullOrWhiteSpace(MachineName) && MachineName != Environment.MachineName)
                res = System.Diagnostics.Process.GetProcesses(MachineName);
            else
                res = System.Diagnostics.Process.GetProcesses();
            return res;
        }
    }
}