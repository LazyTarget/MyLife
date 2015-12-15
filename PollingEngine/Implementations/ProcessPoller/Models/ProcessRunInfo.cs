using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;

namespace ProcessPoller
{
    public class ProcessRunInfo //: ProcessLib.Interfaces.IProcess //IProcessRunInfo
    {
        private Process _process;
        private DateTime? _exitTime;
        private int? _exitCode;


        
        public long ID { get; }
        public int ProcessID { get; private set; }
        public string ProcessName { get; private set; }
        public string MachineName { get; private set; }
        public string ModuleName { get; private set; }
        public string FileName { get; private set; }
        public string MainWindowTitle { get; private set; }
        public bool HasExited
        {
            get { return _process != null && _process.HasExited; }
        }
        public int? ExitCode
        {
            get
            {
                try
                {
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

        public DateTime? ExitTime
        {
            get
            {
                try
                {
                    if (_exitTime.HasValue)
                        return _exitTime.Value;
                    if (HasExited && _process != null)
                        return _process.ExitTime;
                    return null;
                }
                catch (Exception ex)
                {
                    
                }
                return null;
            }
            set { _exitTime = value; }
        }

        public TimeSpan TotalProcessorTime { get; private set; }
        public TimeSpan UserProcessorTime { get; private set; }
        public TimeSpan PrivilegedProcessorTime { get; private set; }
        public IList<ProcessLib.Interfaces.IProcessTitle> Titles { get; set; }


        public TimeSpan Duration
        {
            get
            {
                TimeSpan diff;
                //if (HasExited)
                if (ExitTime.HasValue)
                    diff = ExitTime.Value.Subtract(StartTime);
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



        public void ApplyFromProcess(Process proc)
        {
            try
            {
                _process = proc;
                ProcessID = proc.Id;
                ProcessName = proc.ProcessName;
                MachineName = proc.MachineName;
                if (HasExited)
                {
                    ExitCode = proc.ExitCode;
                    ExitTime = proc.ExitTime;
                }
                else
                    ExitTime = DateTime.Now;
                StartTime = proc.StartTime;
                TotalProcessorTime = proc.TotalProcessorTime;
                UserProcessorTime = proc.UserProcessorTime;
                PrivilegedProcessorTime = proc.PrivilegedProcessorTime;


                try
                {
                    MainWindowTitle = proc.MainWindowTitle;
                    ModuleName = proc.MainModule.ModuleName;
                    FileName = proc.MainModule.FileName;
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
                            FileName = mo["ExecutablePath"].ToString();
                            ModuleName = Path.GetFileName(FileName);
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
            catch (Exception ex)
            {
                //throw;
            }
        }
    }
}