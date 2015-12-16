using System;
using System.Collections.Generic;
using System.ComponentModel;
using ProcessLib.Interfaces;

namespace ProcessLib.Models
{
    internal class ProcessRunInfo : IProcess
    {
        private System.Diagnostics.Process _process;
        private DateTime? _startTime;
        private DateTime? _exitTime;
        private int? _exitCode;
        private bool? _hasExited;


        public ProcessRunInfo(System.Diagnostics.Process proc)
        {
            _process = proc;
        }

        
        public long ID { get; set; }
        public int ProcessID { get; set; }
        public string ProcessName { get; set; }
        public string MachineName { get; set; }
        public string ModuleName { get; set; }
        public string FileName { get; set; }
        //public string MainWindowTitle { get; set; }

        public bool HasExited
        {
            get
            {
                bool result = false;
                if (_hasExited.HasValue)
                    result = _hasExited.Value;
                try
                {
                    if (_process != null)
                        result = _process.HasExited;
                }
                catch (Win32Exception ex)
                {
                    
                }
                catch (Exception ex)
                {

                }
                return result;
            }
            set { _hasExited = value; }
        }

        public int? ExitCode
        {
            get
            {
                int? result = null;
                if (_exitCode.HasValue)
                    result = _exitCode.Value;
                try
                {
                    if (_process != null && _process.HasExited)
                        result = _process.ExitCode;
                }
                catch (Exception ex)
                {
                    
                }
                return result;
            }
            set { _exitCode = value; }
        }

        public DateTime? StartTime
        {
            get
            {
                DateTime? result = null;
                if (_startTime.HasValue)
                    result = _startTime.Value;
                try
                {
                    if (_process != null)
                        result = _process.StartTime;
                }
                catch (Exception ex)
                {

                }
                return result;
            }
            set { _startTime = value; }
        }

        public DateTime? ExitTime
        {
            get
            {
                DateTime? result = null;
                if (_exitTime.HasValue)
                    result = _exitTime.Value;
                try
                {
                    if (_process != null && _process.HasExited)
                        result = _process.ExitTime;
                }
                catch (Exception ex)
                {

                }
                return result;
            }
            set { _exitTime = value; }
        }

        public DateTime TimeAdded { get; set; }
        public DateTime TimeUpdated { get; set; }

        public IList<ProcessTitle> Titles { get; set; }
        

        public System.Diagnostics.Process GetProcess()
        {
            //var proc = Process.GetProcessById(ProcessID);
            //return proc;
            return _process;
        }

        //public override string ToString()
        //{
        //    return string.Format("#{0} {1}", ProcessID, ProcessName);
        //}

        public void Dispose()
        {
            if (_process != null)
            {
                _process.Dispose();
                _process = null;
            }
        }
    }
}