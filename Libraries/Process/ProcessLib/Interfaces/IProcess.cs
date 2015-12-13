using System;

namespace ProcessLib.Interfaces
{
    public interface IProcess
    {
        int ProcessID { get; }
        string ProcessName { get; }
        string MachineName { get; }
        string ModuleName { get; }
        string FileName { get; }
        //string MainWindowTitle { get; }
        bool HasExited { get; }
        int? ExitCode { get; set; }
        DateTime StartTime { get; }
        DateTime? ExitTime { get; set; }
        TimeSpan TotalProcessorTime { get; }
        TimeSpan UserProcessorTime { get; }
        TimeSpan PrivilegedProcessorTime { get; }
        //TimeSpan Duration { get; }
    }
}