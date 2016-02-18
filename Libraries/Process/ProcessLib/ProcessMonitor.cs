using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Management;
using System.Threading.Tasks;
using ProcessLib.Interfaces;
using ProcessLib.Models;
using SharedLib;

namespace ProcessLib
{
    public class ProcessMonitor : IProcessMonitor
    {
        private List<IProcess> _preList = new List<IProcess>();
        private readonly IDictionary<long, bool> _exitedAttached = new Dictionary<long, bool>();
        private readonly IProcessRetriever _processRetriever;
        private readonly IProcessRepository _processRepository;
        private readonly DisposableLock _actionLock = new DisposableLock();
        private bool _initialized;
        private bool _stopped;


        public ProcessMonitor(IProcessRepository processRepository)
            : this(processRepository, Environment.MachineName)
        {

        }

        public ProcessMonitor(IProcessRepository processRepository, string machineName)
            : this(processRepository, new ProcessRetriever(machineName))
        {
            
        }

        public ProcessMonitor(IProcessRepository processRepository, IProcessRetriever processRetriever)
        {
            if (processRepository == null)
                throw new ArgumentNullException(nameof(processRepository));
            if (processRetriever == null)
                throw new ArgumentNullException(nameof(processRetriever));
            _processRepository = processRepository;
            _processRetriever = processRetriever;
        }


        public string MachineName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_processRetriever.MachineName) && _processRetriever.MachineName != ".")
                    return _processRetriever.MachineName;
                return Environment.MachineName;
            }
        }


        protected virtual DateTime GetNow()
        {
            return DateTime.UtcNow;
        }

        protected virtual void Log(string message)
        {
            Console.WriteLine(message);
            System.Diagnostics.Trace.WriteLine(message);
        }


        public async Task Init()
        {
            using (await _actionLock.EnterAsync())
            {
                if (_initialized)
                    return;
                Log("Initializing...");
                await DoInit();
                Log("Initialized...");
                _initialized = true;
                _stopped = false;
            }
        }
        
        protected virtual async Task DoInit()
        {
            _preList = (await LoadProcesses()).ToList();
        }


        public async Task Work()
        {
            using (await _actionLock.EnterAsync())
            {
                if (_stopped)
                {
                    Log("Won't work, has stopped");
                    return;
                }
                if (!_initialized)
                {
                    Log("Won't work, not initialized");
                    return;
                }
                await DoWork();
            }
        }
        
        protected virtual async Task DoWork()
        {
            Log("\t::Polling processes::");
            var now = GetNow();
            var processes = _processRetriever.GetProcesses().ToDictionary(x => x.Id, x => x);

            var preList = _preList;
            var resList = new List<IProcess>();
            
            foreach (var proc in processes.Values)
            {
                var process = _preList.FirstOrDefault(x => x.ProcessID == proc.Id &&
                                                           x.ProcessName == proc.ProcessName);
                var newProcess = process == null || process.ID <= 0;
                if (newProcess)
                {
                    // Started
                    //process = new ProcessLib.Models.Process();
                    process = new ProcessRunInfo(proc);
                    ApplyProcess(process, proc, now);
                    process.TimeAdded = process.TimeUpdated = now;

                    if (process.HasExited)
                    {
                        // the start wasn't tracked, skip process
                        if (!process.StartTime.HasValue)
                        {
                            continue;
                        }
                        else
                        {
                            await OnProcessExited(process);
                        }
                    }
                    else
                    {
                        await OnProcessStarted(process);
                    }
                }
                else
                {
                    ApplyProcess(process, proc, now);
                    process.TimeUpdated = now;

                    if (process.HasExited)
                    {
                        await OnProcessExited(process);
                    }
                    else
                    {
                        //await StoreProcess(process);
                        await OnProcessUpdate(process);
                    }
                }



                try
                {
                    bool attached;
                    attached = _exitedAttached.TryGetValue(process.ID, out attached) && attached;
                    if (!process.HasExited && !proc.EnableRaisingEvents && !attached)
                    {
                        EventHandler onExited = null;
                        onExited = delegate (object sender, EventArgs args)
                        {
                            try
                            {
                                Log($"Process.Exited(..) :: #{process.ProcessID} {process.ProcessName}");
                                // Un-attach event handler
                                proc.EnableRaisingEvents = false;
                                if (onExited != null)
                                    proc.Exited -= onExited;
                                var r = _exitedAttached.Remove(process.ID);
                                attached = false;
                            }
                            catch (Exception ex)
                            {
                                Log($"Issue when detaching listener to Process.Exited, #{process.ProcessID} {process.ProcessName}. Error: {ex.Message}");
                            }

                            var task = OnProcessExited(process);
                            task.Wait(TimeSpan.FromSeconds(30));
                            process.ExitCode = process.ExitCode;
                            // enforces that ExitCode should be written to the local variable and not the 'Process-proxy' (when ProcessRunInfo)
                        };

                        proc.Exited += onExited;
                        proc.EnableRaisingEvents = attached = true;
                        _exitedAttached[process.ID] = attached;
                        Log($"Attached listener to Process.Exited, #{process.ProcessID} {process.ProcessName}");
                    }
                }
                catch (Win32Exception ex)
                {
                    if (ex.Message != "Access is denied")
                    {
                        //Log($"Error attaching Process.Exited event, #{process.ProcessID} {process.ProcessName}. Error: {ex.Message}");
                    }
                    proc.EnableRaisingEvents = false;
                    _exitedAttached[process.ID] = false;
                }
                catch (Exception ex)
                {
                    //Log($"Error attaching Process.Exited event, #{process.ProcessID} {process.ProcessName}. Error: {ex.Message}");
                    proc.EnableRaisingEvents = false;
                    _exitedAttached[process.ID] = false;
                }

                resList.Add(process);
            }


            // prev processes
            foreach (var process in preList)
            {
                if (resList.Any(x => x.ID == process.ID))
                    continue;

                // Continue or Exit
                var hasExited = false;
                System.Diagnostics.Process proc = null;
                try
                {
                    now = GetNow();
                    proc = _processRetriever.GetProcessById(process.ProcessID);
                    if (proc != null)
                    {
                        if (proc.ProcessName == process.ProcessName)
                        {
                            if (process.StartTime.HasValue)
                            {
                                // Round to nearest millisecond
                                var procStartTime = proc.StartTime.ToUniversalTime();
                                procStartTime = procStartTime.AddTicks(-(procStartTime.Ticks % TimeSpan.TicksPerSecond));
                                var processStartTime = process.StartTime.Value;
                                processStartTime = processStartTime.AddTicks(-(processStartTime.Ticks%TimeSpan.TicksPerSecond));

                                if (procStartTime == processStartTime)
                                    ApplyProcess(process, proc, now);
                                else
                                    hasExited = true;
                            }
                            else
                            {
                                
                            }
                        }
                        else
                            hasExited = true;
                    }
                    else
                        hasExited = true;
                }
                catch (Exception ex)
                {
                    hasExited = true;
                }
                finally
                {
                    if (proc == null)
                        proc = (process as ProcessRunInfo)?.GetProcess();
                    if (proc == null || hasExited)
                    {
                        // process is not found (has exited)
                        process.HasExited = true;
                        if (!process.ExitTime.HasValue)
                            process.ExitTime = now;
                    }
                }

                if (proc != null && proc.EnableRaisingEvents)
                {
                    // do nothing as Exited should handle OnProcessExited
                    ApplyProcess(process, proc, now);
                }
                else if (process.HasExited)
                {
                    await OnProcessExited(process);
                }
                else
                {
                    //await StoreProcess(process);
                    await OnProcessUpdate(process);
                }
                resList.Add(process);
            }

            
            //_preList = resList;
            _preList = resList.Where(x => !x.HasExited).ToList();
            Log($"Total of '{_preList.Count}' processes running");

            var exited = resList.Where(x => x.HasExited).ToList();
            Log($"Total of '{exited.Count}' process exited during interval");
            foreach (var process in exited)
            {
                try
                {
                    process.Dispose();
                }
                catch (Exception ex)
                {
                    
                }
            }
        }


        public async Task Stop()
        {
            using (await _actionLock.EnterAsync())
            {
                if (_stopped)
                    return;
                Log("Stopping...");
                await DoStop();
                Log("Stopped...");
                _initialized = false;
                _stopped = true;
            }
        }
        
        protected virtual async Task DoStop()
        {
            // Enforce update
            await DoWork();
            
            // Dispose of processes
            var processes = _preList.ToList();
            _preList.Clear();
            foreach (var process in processes)
            {
                try
                {
                    process.Dispose();
                }
                catch (Exception ex)
                {

                }
            }
        }


        protected virtual async Task OnProcessStarted(IProcess process)
        {
            try
            {
                var msg = string.Format("#{2} {0} has started, duration: {1}", process.ProcessName, TimeSpanToString(CalculateDuration(process)), process.ProcessID);
                Log(msg);

                await StoreProcess(process);
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        protected virtual async Task OnProcessExited(IProcess process)
        {
            try
            {
                var msg = string.Format("#{2} {0} has exited, duration: {1}", process.ProcessName, TimeSpanToString(CalculateDuration(process)), process.ProcessID);
                Log(msg);

                await StoreProcess(process);
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        protected virtual async Task OnProcessUpdate(IProcess process)
        {
            try
            {
                //var msg = string.Format("{0} is still running, duration: {1}", process.ProcessName, TimeSpanToString(CalculateDuration(process)));
                //Log(msg);

                await StoreProcess(process);
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        protected virtual async Task<IProcess> StoreProcess(IProcess process)
        {
            try
            {
                var now = DateTime.UtcNow;
                var machineName = process.MachineName != "."
                    ? process.MachineName
                    : MachineName;

                var proc = new Models.Process();
                CopyFrom(process, proc);
                proc.MachineName = machineName;
                if (proc.TimeAdded == DateTime.MinValue)
                    proc.TimeAdded = now;
                if (proc.TimeUpdated == DateTime.MinValue)
                    proc.TimeUpdated = now;
                proc = await _processRepository.UpdateProcess(proc);
                process.Titles = null;
                CopyFrom(proc, process);
            }
            catch (Exception ex)
            {
                Log("Failed to store process, Entry: " + process + ", Error: " + ex.Message);
                throw;
            }
            return process;
        }


        protected virtual async Task<IEnumerable<IProcess>> LoadProcesses()
        {
            Expression<Func<Process, bool>> filter = (x) => x.MachineName == MachineName && !x.HasExited;
            var result = await _processRepository.GetProcesses(filter);
            return result;
        }





        
        private void ApplyProcess(IProcess process, System.Diagnostics.Process proc, DateTime now)
        {
            if (process == null)
                return;
            if (proc == null)
                return;
            var p = new Process();
            try
            {
                CopyFrom(process, p);

                var machineName = proc.MachineName != "."
                    ? proc.MachineName
                    : MachineName;

                p.ID = process.ID;
                p.ProcessID = proc.Id;
                p.ProcessName = proc.ProcessName;
                p.MachineName = machineName;
                p.TimeAdded = process.TimeAdded;
                p.TimeUpdated = now;
                if (p.HasExited)
                {
                    p.ExitCode = proc.ExitCode;
                    p.ExitTime = proc.ExitTime;
                }
                else
                    p.ExitTime = now;

                //process.MainWindowTitle = proc.MainWindowTitle;
                p.Titles = p.Titles ?? new List<ProcessTitle>();
                var prevTitle = p.Titles.OrderByDescending(x => x.StartTime).FirstOrDefault();
                if (prevTitle == null || prevTitle.ProcessID == p.ID)
                {
                    if (prevTitle == null || prevTitle.Title != proc.MainWindowTitle)
                    {
                        if (prevTitle != null && !prevTitle.EndTime.HasValue)
                            prevTitle.EndTime = now;
                        if (!string.IsNullOrWhiteSpace(proc.MainWindowTitle))
                        {
                            var title = new ProcessTitle
                            {
                                StartTime = now,
                                EndTime = null,
                                Title = proc.MainWindowTitle,
                                ProcessID = p.ID,
                            };
                            p.Titles.Add(title);
                            p.Titles = p.Titles.OrderBy(x => x.StartTime).ThenBy(x => x.ID).ToList();
                        }
                        else
                        {
                            
                        }
                    }
                    else
                    {

                    }
                }

                try
                {
                    p.StartTime = proc.StartTime.ToUniversalTime();
                }
                catch (Win32Exception ex)
                {
                    //throw;
                }
                catch (Exception ex)
                {
                    //throw;
                }

                try
                {
                    p.ModuleName = proc.MainModule.ModuleName;
                    p.FileName = proc.MainModule.FileName;
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
                            var exePath = (mo["ExecutablePath"] ?? "").ToString();
                            if (!string.IsNullOrWhiteSpace(exePath))
                            {
                                p.FileName = exePath;
                                p.ModuleName = Path.GetFileName(p.FileName);
                            }
                            else
                            {
                                
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //throw;
                }
            }
            catch (Win32Exception ex)
            {
                //throw;
            }
            catch (Exception ex)
            {
                //throw;
            }
            finally
            {
                if (process != null && p != null)
                    CopyFrom(p, process);
            }
        }


        private static void CopyFrom(IProcess process, IProcess target)
        {
            if (process == null)
                return;
            target.ID = process.ID;
            target.ProcessID = process.ProcessID;
            target.ProcessName = process.ProcessName;
            target.MachineName = process.MachineName;
            target.ModuleName = process.ModuleName;
            target.FileName = process.FileName;
            target.HasExited = process.HasExited;
            target.ExitCode = process.ExitCode;
            target.StartTime = process.StartTime;
            target.ExitTime = process.ExitTime;
            target.TimeAdded = process.TimeAdded;
            target.TimeUpdated = process.TimeUpdated;

            target.Titles = target.Titles ?? new List<ProcessTitle>();
            if (process.Titles != null)
            {
                foreach (var processTitle in process.Titles)
                {
                    if (!target.Titles.Contains(processTitle))
                    {
                        target.Titles = new List<ProcessTitle>(target.Titles);
                        target.Titles.Add(processTitle);
                    }
                }
            }
            target.Titles = target.Titles.OrderBy(x => x.StartTime).ThenBy(x => x.ID).ToList();
        }


        private static string TimeSpanToString(TimeSpan duration)
        {
            var res = "";
            if (duration.TotalMinutes < 1)
                res += string.Format("{0}s", duration.Seconds);
            else if (duration.TotalHours < 1)
                res += string.Format("{0}m {1}s", duration.Minutes, duration.Seconds);
            else if (duration.TotalDays < 1)
                res += string.Format("{0}h {1}m", duration.Hours, duration.Minutes);
            else
                res += string.Format("{0}d {1}h {2}m", duration.Days, duration.Hours, duration.Minutes);
            return res;
        }

        private static TimeSpan CalculateDuration(IProcess process)
        {
            var diff = TimeSpan.Zero;
            if (process != null)
                diff = CalculateDuration(process.StartTime, process.ExitTime);
            return diff;
        }

        private static TimeSpan CalculateDuration(DateTime? start, DateTime? end)
        {
            TimeSpan diff;
            if (!start.HasValue)
                diff = TimeSpan.Zero;
            else if (end.HasValue)
                diff = end.Value.ToUniversalTime().Subtract(start.Value.ToUniversalTime());
            else
                diff = DateTime.UtcNow.Subtract(start.Value.ToUniversalTime());
            return diff;
        }

    }
}
