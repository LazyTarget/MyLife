using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;

namespace ProcessPoller
{
    public class ProcessRunInfo : IProcessRunInfo, IDisposable
    {
        private Process _process;
        private DateTime _exitTime;
        private int? _exitCode;

        public static ProcessRunInfo FromProcess(Process proc)
        {
            try
            {
                var res = new ProcessRunInfo();
                res._process = proc;
                res.ProcessID = proc.Id;
                res.ProcessName = proc.ProcessName;
                res.MachineName = proc.MachineName;
                if (res.HasExited)
                {
                    res.ExitCode = proc.ExitCode;
                    res.ExitTime = proc.ExitTime;
                }
                else
                    res.ExitTime = DateTime.Now;
                res.StartTime = proc.StartTime;
                res.TotalProcessorTime = proc.TotalProcessorTime;
                res.UserProcessorTime = proc.UserProcessorTime;
                res.PrivilegedProcessorTime = proc.PrivilegedProcessorTime;

                try
                {
                    res.MainWindowTitle = proc.MainWindowTitle;
                    res.ModuleName = proc.MainModule.ModuleName;
                    res.FileName = proc.MainModule.FileName;
                }
                catch (Win32Exception ex)
                {
                    var wmiQueryString = "SELECT ProcessId, ExecutablePath FROM Win32_Process WHERE ProcessId = " + proc.Id;
                    using (var searcher = new ManagementObjectSearcher(wmiQueryString))
                    using (var results = searcher.Get())
                    {
                        var mo = results.Cast<ManagementObject>().FirstOrDefault();
                        if (mo != null)
                        {
                            res.FileName = mo["ExecutablePath"].ToString();
                            res.ModuleName = Path.GetFileName(res.FileName);
                        }
                    }
                }
                catch (Exception ex)
                {

                }

                return res;
            }
            catch (Exception ex)
            {
                //throw;
                return null;
            }
        }



        public int ProcessID { get; private set; }
        public string ProcessName { get; private set; }
        public string MachineName { get; private set; }
        public string ModuleName { get; private set; }
        public string FileName { get; private set; }
        public string MainWindowTitle { get; private set; }
        public bool HasExited
        {
            get { return _process.HasExited; }
        }
        public int? ExitCode
        {
            get
            {
                try
                {
                    if (_process == null)
                        return null;
                    if (HasExited)
                        return _process.ExitCode;
                }
                catch (Exception ex)
                {
                    
                }
                if (_exitCode.HasValue)
                    return _exitCode.Value;
                return null;
            }
            set { _exitCode = value; }
        }

        public DateTime StartTime { get; private set; }

        public DateTime ExitTime
        {
            get
            {
                try
                {
                    if (_process == null)
                        return DateTime.MinValue;
                    if (HasExited)
                        return _process.ExitTime;
                }
                catch (Exception ex)
                {
                    
                }
                return _exitTime;
            }
            set { _exitTime = value; }
        }

        public TimeSpan TotalProcessorTime { get; private set; }
        public TimeSpan UserProcessorTime { get; private set; }
        public TimeSpan PrivilegedProcessorTime { get; private set; }


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



        public Process GetProcess()
        {
            //var proc = Process.GetProcessById(ProcessID);
            //return proc;
            return _process;
        }

        public override string ToString()
        {
            return string.Format("#{0} {1}", ProcessID, ProcessName);
        }

        public void Dispose()
        {
            try
            {
                if (_process != null)
                {
                    _process.Dispose();
                    _process = null;
                }
            }
            catch (Exception ex)
            {

            }
        }

    }
}