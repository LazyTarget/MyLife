using System;
using System.Collections.Generic;
using ProcessLib.Interfaces;

namespace ProcessLib.Models
{
    public class Process : IProcess
    {
        public long ID { get; set; }
        
        public int ProcessID { get; set;  }
        public string ProcessName { get; set; }
        public string MachineName { get; set; }
        public string ModuleName { get; set; }
        public string FileName { get; set; }

        //public string MainWindowTitle { get; set; }
        public virtual IList<ProcessTitle> Titles { get; set; }

        public bool HasExited { get; set; }
        public int? ExitCode { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime ExitTime { get; set; }
        public TimeSpan TotalProcessorTime { get; set; }
        public TimeSpan UserProcessorTime { get; set; }
        public TimeSpan PrivilegedProcessorTime { get; set; }

        //public TimeSpan Duration
        //{
        //    get
        //    {
        //        TimeSpan diff;
        //        //if (HasExited)
        //        if (ExitTime != DateTime.MinValue)
        //            diff = ExitTime.Subtract(StartTime);
        //        else
        //            diff = DateTime.Now.Subtract(StartTime);
        //        return diff;
        //    }
        //}

        public override string ToString()
        {
            return string.Format("#{0} {1}", ProcessID, ProcessName);
        }
    }
}