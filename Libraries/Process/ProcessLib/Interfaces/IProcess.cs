using System;
using System.Collections.Generic;
using ProcessLib.Models;

namespace ProcessLib.Interfaces
{
    public interface IProcess : IDisposable
    {
        long ID { get; set; }
        int ProcessID { get; set; }
        string ProcessName { get; set; }
        string MachineName { get; set; }
        string ModuleName { get; set; }
        string FileName { get; set; }
        //string MainWindowTitle { get; set; }
        bool HasExited { get; set; }
        int? ExitCode { get; set; }
        DateTime? StartTime { get; set; }
        DateTime? ExitTime { get; set; }
        DateTime TimeAdded { get; set; }
        DateTime TimeUpdated { get; set; }
        //TimeSpan Duration { get; }
        IList<ProcessTitle> Titles { get; set; }
    }
}