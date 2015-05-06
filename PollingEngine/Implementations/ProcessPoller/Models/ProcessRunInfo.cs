﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;

namespace ProcessPoller
{
    public class ProcessRunInfo
    {
        public static ProcessRunInfo FromProcess(Process proc)
        {
            try
            {
                var res = new ProcessRunInfo();
                res.ProcessID = proc.Id;
                res.ProcessName = proc.ProcessName;
                res.MachineName = proc.MachineName;
                res.HasExited = proc.HasExited;
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
        public bool HasExited { get; set; }
        public int? ExitCode { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime ExitTime { get; set; }
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
            var proc = Process.GetProcessById(ProcessID);
            return proc;
        }

        public override string ToString()
        {
            return string.Format("#{0} {1}", ProcessID, ProcessName);
        }
    }
}