using System;

namespace ProcessPoller
{
    public class CustomProcessRunInfo : IProcessRunInfo
    {
        public int ProcessID { get; set; }
        public string ProcessName { get; set; }
        public string MachineName { get; set; }
        public string ModuleName { get; set; }
        public string FileName { get; set; }
        public string MainWindowTitle { get; set; }
        public bool HasExited { get; set; }
        public int? ExitCode { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime ExitTime { get; set; }
        public TimeSpan TotalProcessorTime { get; set; }
        public TimeSpan UserProcessorTime { get; set; }
        public TimeSpan PrivilegedProcessorTime { get; set; }

        public TimeSpan Duration
        {
            get
            {
                TimeSpan diff;
                //if (HasExited)
                if (ExitTime != DateTime.MinValue)
                    diff = ExitTime.Subtract(StartTime);
                else
                    diff = DateTime.Now.Subtract(StartTime);
                return diff;
            }
        }

        public override string ToString()
        {
            return string.Format("#{0} {1}", ProcessID, ProcessName);
        }
    }
}