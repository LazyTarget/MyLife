using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Odbc;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Management;
using System.Threading.Tasks;
using MyLife.API.Client;
using PollingEngine.Core;
using Process = System.Diagnostics.Process;

namespace ProcessPoller
{
    public class ProcessPoller : IPollingProgram
    {
        private List<ProcessLib.Interfaces.IProcess> _preList = new List<ProcessLib.Interfaces.IProcess>();
        private readonly Simple.OData.Client.ODataClient _client;
        private readonly ProcessManager _manager;
        private Func<Process, bool> _processFilter; 

        public ProcessPoller()
        {
            var uri = new Uri("http://localhost:5227/api");
            _client = new Simple.OData.Client.ODataClient(uri);
            _manager = new ProcessManager(_client);
            _processFilter = process =>
            {
                var res = true;
                try
                {
                    if (process.Id <= 0)
                        res = false;
#if DEBUG
                    else if (string.IsNullOrWhiteSpace(process.MainWindowTitle))
                        res = false;
#endif

                    if (process.ProcessName.ToLower() == "explorer")
                        res = true;
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    
                }
                return res;
            };
        }


        public async Task OnStarting(PollingContext context)
        {
            ProcessLib.Models.Process proc;
            try
            {


                var init = context != null;
                if (init)
                {
                    var start = DateTime.UtcNow;
                    proc = new ProcessLib.Models.Process
                    {
                        StartTime = start,
                        ProcessID = 22022,
                        MachineName = Environment.MachineName,
                        FileName = "QWERTY.exe",
                    };
                    proc.Titles = new List<ProcessLib.Models.ProcessTitle>(proc.Titles ?? new List<ProcessLib.Models.ProcessTitle>());


                    var title2 = new ProcessLib.Models.ProcessTitle
                    {
                        Title = "Qwerty - Starting up...",
                        StartTime = start,
                    };
                    proc.Titles.Add(title2);


                    proc = await _manager.UpdateProcess(proc);
                }
                else
                {
                    var t = _client.For<ProcessLib.Models.Process>()
                        .Top(5)
                        .Expand(x => x.Titles)
                        //.FindEntryAsync();
                        .FindEntriesAsync();
                    var processes = (await t).ToList();
                    //var processes = new List<ProcessLib.Models.Process> {(await  t)};

                    proc = processes.FirstOrDefault();
                }



                if (proc != null)
                {
                    proc.Titles = new List<ProcessLib.Models.ProcessTitle>(proc.Titles ?? new List<ProcessLib.Models.ProcessTitle>());
                    proc.Titles.Add(new ProcessLib.Models.ProcessTitle
                    {
                        Title = $"Qwerty - Title @{DateTime.UtcNow}",
                        StartTime = DateTime.UtcNow,
                    });

                    proc = await _manager.UpdateProcess(proc);
                }
            }
            catch (Exception ex)
            {

            }


            _preList = (await LoadProcesses()).ToList();
        }


        public async Task OnInterval(PollingContext context)
        {
            Log("Polling processes");
            //var processes = Process.GetProcesses();
            var processes2 = Process.GetProcesses().ToDictionary(x => x.Id, x => x);
            var processes = Process.GetProcesses().Where(_processFilter).ToDictionary(x => x.Id, x => x);

            var now = DateTime.UtcNow;
            var preList = _preList;
            //var curList = new List<ProcessLib.Interfaces.IProcess>();
            
            //curList.AddRange(processes.Select(proc =>
            //{
            //    var process = new ProcessLib.Models.Process();
            //    ApplyProcess(process, proc, now);
            //    process.TimeUpdated = now;
            //    return process;
            //}));

            

            var resList = new List<ProcessLib.Interfaces.IProcess>();
            //foreach (var processRunInfo in curList)
            //{
            //    var proc = _preList.FirstOrDefault(x => x.ProcessID == processRunInfo.ProcessID &&
            //                                            x.ProcessName == processRunInfo.ProcessName);
            //    if (proc == null)
            //    {
            //        // Started
            //        proc = processRunInfo;
            //        await OnProcessStarted(proc);
            //    }
            //    else
            //    {
            //        // Continue or Exit
            //        //proc.ProcessInfo = processRunInfo;      // update data
            //        processRunInfo.ID = proc.ID;
            //        processRunInfo.TimeAdded = proc.TimeAdded;
            //        if (proc.Titles != null && processRunInfo.Titles != null)
            //        {
            //            proc.Titles.Where(x => processRunInfo.Titles.All(y => y.ID <= 0 || y.ID != x.ID)).ToList().ForEach(x =>
            //            {
            //                processRunInfo.Titles.Add(x);
            //            });
            //            processRunInfo.Titles = processRunInfo.Titles.OrderBy(x => x.StartTime).ThenBy(x => x.ID).ToList();
            //        }
            //        proc.CopyFrom(processRunInfo);

            //        if (proc.HasExited)
            //        {
            //            await OnProcessExited(proc);
            //            //continue;
            //        }
            //        else
            //            await StoreProcess(proc);
            //    }
            //    resList.Add(proc);
            //}
            
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
                        try
                        {
                            proc.EnableRaisingEvents = true;
                            proc.Exited += delegate(object sender, EventArgs args)
                            {
                                var task = OnProcessExited(process);
                                task.Wait(TimeSpan.FromSeconds(30));
                                process.ExitCode = process.ExitCode;    // enforces that ExitCode should be written to the local variable and not the 'Process-proxy'
                            };
                        }
                        catch (Exception ex)
                        {
                            proc.EnableRaisingEvents = false;
                        }

                        await OnProcessStarted(process);
                    }
                }
                else
                {
                    ApplyProcess(process, proc, now);
                    process.TimeUpdated = now;
                    
                    if (process.HasExited)
                        await OnProcessExited(process);
                    else
                        await StoreProcess(process);
                }
                resList.Add(process);
            }


            // prev processes
            foreach (var process in preList)
            {
                if (resList.Any(x => x.ID == process.ID))
                    continue;

                // Continue or Exit
                Process proc;
                try
                {
                    now = DateTime.UtcNow;
                    proc = Process.GetProcessById(process.ProcessID);
                    //var proc = processes.ContainsKey(process.ProcessID)
                    //    ? processes[process.ProcessID]
                    //    : null;
                    //var proc = (process as ProcessRunInfo)?.GetProcess();
                    if (proc != null)
                    {
                        //process.ProcessInfo = ProcessRunInfo.FromProcess(proc); // update data
                        ApplyProcess(process, proc, now);
                    }
                    else
                    {
                        // process is not found (has exited)
                        process.HasExited = true;
                        if (!process.ExitTime.HasValue)
                            process.ExitTime = now;
                    }
                }
                catch (Exception ex)
                {
                    // process is not found (has exited)
                    process.HasExited = true;
                    if (!process.ExitTime.HasValue)
                        process.ExitTime = now;

                    proc = (process as ProcessRunInfo)?.GetProcess();
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
                    await StoreProcess(process);
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

        public async Task OnStopping(PollingContext context)
        {
            
        }

        public void ApplyArguments(string[] args)
        {
            
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




        private async Task OnProcessStarted(ProcessLib.Interfaces.IProcess process)
        {
            try
            {
                var msg = string.Format("{0} has started, duration: {1}", process.ProcessName, TimeSpanToString(CalculateDuration(process)));
                Log(msg);

                await StoreProcess(process);
            }
            catch (Exception ex)
            {
                Log("Failed to create odbc entry, Error: " + ex.Message);
                throw;
            }
        }


        private async Task OnProcessExited(ProcessLib.Interfaces.IProcess process)
        {
            try
            {
                var msg = string.Format("{0} has exited, duration: {1}", process.ProcessName, TimeSpanToString(CalculateDuration(process)));
                Log(msg);

                await StoreProcess(process);
            }
            catch (Exception ex)
            {
                //Log("Failed to create odbc entry, Error: " + ex.Message);
                throw;
            }
        }


        private async Task<ProcessLib.Interfaces.IProcess> StoreProcess(ProcessLib.Interfaces.IProcess process)
        {
            try
            {
                //var procID = process.ID;
                //if (procID <= 0 || process.ProcessInfo.HasExited)
                //    Log("Begin creating/updating odbc entry");

                var now = DateTime.UtcNow;
                var machineName = process.MachineName != "."
                    ? process.MachineName
                    : Environment.MachineName;

                var proc = new ProcessLib.Models.Process();
                CopyFrom(process, proc);
                proc.MachineName = machineName;
                if (proc.TimeAdded == DateTime.MinValue)
                    proc.TimeAdded = now;
                if (proc.TimeUpdated == DateTime.MinValue)
                    proc.TimeUpdated = now;
                proc = await _manager.UpdateProcess(proc);
                process.Titles = null;
                CopyFrom(proc, process);

                //var connectionString = ConfigurationManager.ConnectionStrings["MyLifeDatabase"].ConnectionString;
                //var cn = new OdbcConnection(connectionString);
                //if (cn.State != ConnectionState.Open)
                //    cn.Open();

                //int changes;
                //if (process.ID <= 0)
                //{
                //    var sql = "INSERT INTO Process_Events(ProcessID, ProcessName, MachineName, HasExited, StartTime, ExitTime, ExitCode, MainWindowTitle, ModuleName, FileName) " +
                //              "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?); SELECT ? = @@IDENTITY";
                //    var cmd = new OdbcCommand(sql, cn);
                //    cmd.Parameters.AddWithValue("@ProcessID", process.ProcessID);
                //    cmd.Parameters.AddWithValue("@ProcessName", process.ProcessName);
                //    cmd.Parameters.AddWithValue("@MachineName", machineName);
                //    cmd.Parameters.AddWithValue("@HasExited", process.HasExited);
                //    cmd.Parameters.AddWithValue("@StartTime", process.StartTime);
                //    cmd.Parameters.AddWithValue("@ExitTime", process.HasExited ? (object) process.ExitTime : DBNull.Value);
                //    cmd.Parameters.AddWithValue("@ExitCode", process.ExitCode.HasValue ? (object)process.ExitCode.Value : DBNull.Value);
                //    cmd.Parameters.AddWithValue("@MainWindowTitle", !string.IsNullOrEmpty(process.MainWindowTitle) ? (object) process.MainWindowTitle : DBNull.Value);
                //    cmd.Parameters.AddWithValue("@ModuleName", !string.IsNullOrEmpty(process.ModuleName) ? (object) process.ModuleName : DBNull.Value);
                //    cmd.Parameters.AddWithValue("@FileName", !string.IsNullOrEmpty(process.FileName) ? (object) process.FileName : DBNull.Value);
                //    var idParam = new OdbcParameter("@EntityID", OdbcType.BigInt, 4)
                //    {
                //        Direction = ParameterDirection.Output,
                //    };
                //    cmd.Parameters.Add(idParam);
                //    changes = cmd.ExecuteNonQuery();
                //    process.ID = (long)idParam.Value;
                //}
                //else
                //{
                //    var sql = "UPDATE Process_Events " +
                //              "SET  HasExited = ? " +
                //              "     ,ExitTime = ? " +
                //              "     ,ExitCode = ? " +
                //              (!string.IsNullOrEmpty(process.MainWindowTitle)
                //                  ? "     ,MainWindowTitle = ? "
                //                  : "") +
                //              " WHERE ID = ? ";
                //    var cmd = new OdbcCommand(sql, cn);
                //    cmd.Parameters.AddWithValue("@HasExited", process.HasExited);
                //    //cmd.Parameters.AddWithValue("@ExitTime", processRunInfo.HasExited ? (object) processRunInfo.ExitTime : DBNull.Value);
                //    cmd.Parameters.AddWithValue("@ExitTime", process.ExitTime);
                //    cmd.Parameters.AddWithValue("@ExitCode", process.ExitCode.HasValue ? (object)process.ExitCode.Value : DBNull.Value);
                //    if (!string.IsNullOrEmpty(process.MainWindowTitle))
                //        cmd.Parameters.AddWithValue("@MainWindowTitle", !string.IsNullOrEmpty(process.MainWindowTitle) ? (object) process.MainWindowTitle : DBNull.Value);
                //    cmd.Parameters.AddWithValue("@EntityID", process.ID);
                //    changes = cmd.ExecuteNonQuery();
                //}

                //if (changes > 0)
                //{
                //    //if (procID <= 0 || process.ProcessInfo.HasExited)
                //    //    Log(String.Format("Created/updated odbc event: '{0}'", process));
                //}
                //else
                //{
                //    Log("No changes commited when creating odbc entry: " + process);
                //}
            }
            catch (Exception ex)
            {
                Log("Failed to create odbc entry, Entry: " + process + ", Error: " + ex.Message);
                throw;
            }
            return process;
        }


        private async Task<IEnumerable<ProcessLib.Interfaces.IProcess>> LoadProcesses()
        {
            var machineName = Environment.MachineName;

            var result = await _client.For<ProcessLib.Models.Process>()
                //.Top(5)
                .Expand(x => x.Titles)
                .Filter(x => x.MachineName == machineName && !x.HasExited)
                .FindEntriesAsync();
            return result;

            //OdbcDataReader reader;
            //try
            //{
            //    Log("Loading stored processes");

            //    var machineName = Environment.MachineName;

            //    var connectionString = ConfigurationManager.ConnectionStrings["MyLifeDatabase"].ConnectionString;
            //    var cn = new OdbcConnection(connectionString);
            //    if (cn.State != ConnectionState.Open)
            //        cn.Open();

            //    var sql = "SELECT * " +
            //              "FROM Process_Events " +
            //              "WHERE MachineName = ? AND HasExited = 0 " +
            //              "ORDER BY StartTime";
            //    var cmd = new OdbcCommand(sql, cn);
            //    cmd.CommandText = sql;
            //    cmd.Parameters.AddWithValue("@MachineName", machineName);
            //    reader = cmd.ExecuteReader();
            //}
            //catch (Exception ex)
            //{
            //    Log("Failed to create odbc entry, Error: " + ex.Message);
            //    throw;
            //}

            //using (reader)
            //{
            //    var cID = reader.GetOrdinal("ID");
            //    var cProcessID = reader.GetOrdinal("ProcessID");
            //    var cProcessName = reader.GetOrdinal("ProcessName");
            //    var cMachineName = reader.GetOrdinal("MachineName");
            //    var cModuleName = reader.GetOrdinal("ModuleName");
            //    var cFileName = reader.GetOrdinal("FileName");
            //    var cMainWindowTitle = reader.GetOrdinal("MainWindowTitle");
            //    var cHasExited = reader.GetOrdinal("HasExited");
            //    var cExitCode = reader.GetOrdinal("ExitCode");
            //    var cStartTime = reader.GetOrdinal("StartTime");
            //    var cExitTime = reader.GetOrdinal("ExitTime");

            //    foreach (IDataRecord record in reader)
            //    {
            //        var proc = new ProcessLib.Models.Process();
            //        proc.ID = record.GetInt64(cID);

            //        proc.ProcessID = record.GetInt32(cProcessID);
            //        proc.ProcessName = record.GetString(cProcessName);
            //        proc.MachineName = record.GetString(cMachineName);
            //        proc.ModuleName = record.IsDBNull(cModuleName)
            //            ? null
            //            : record.GetString(cModuleName);
            //        proc.FileName = record.IsDBNull(cFileName)
            //            ? null
            //            : record.GetString(cFileName);
            //        proc.MainWindowTitle = record.IsDBNull(cMainWindowTitle)
            //            ? null
            //            : record.GetString(cMainWindowTitle);
            //        proc.HasExited = record.GetBoolean(cHasExited);
            //        proc.ExitCode = record.IsDBNull(cExitCode)
            //            ? (int?) null
            //            : record.GetInt32(cExitCode);
            //        proc.StartTime = record.GetDateTime(cStartTime);
            //        proc.ExitTime = record.IsDBNull(cExitTime)
            //            ? DateTime.MinValue
            //            : record.GetDateTime(cExitTime);
            //        yield return proc;
            //    }
            //}
        }


        private void Log(string message)
        {
            Console.WriteLine(message);
            Trace.WriteLine(message);
        }


        public static void CopyFrom(ProcessLib.Interfaces.IProcess process, ProcessLib.Interfaces.IProcess target)
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

            target.Titles = target.Titles ?? new List<ProcessLib.Models.ProcessTitle>();
            if (process.Titles != null)
            {
                //var list = new List<ProcessTitle>();
                //var typed = process.Titles.Where(x => x is ProcessTitle).Cast<ProcessTitle>();
                //list.AddRange(typed);

                //var nontyped = process.Titles.Where(x => !(x is ProcessTitle)).ToList();
                //nontyped.ForEach(x =>
                //{
                //    var t = new ProcessTitle();
                //    t.CopyFrom(x);
                //    list.Add(t);
                //});
                //Titles = list;

                //foreach (var processTitle in process.Titles)
                //{
                //    if (processTitle.ID == 0 || target.Titles.All(y => y.ID != processTitle.ID))
                //    {
                //        var t = target.Titles.ToList();
                //        target.Titles = new List<ProcessLib.Models.ProcessTitle>(target.Titles.Count + 4);
                //        t.ForEach(target.Titles.Add);
                //        target.Titles.Add(processTitle);
                //    }
                //    else
                //    {
                        
                //    }
                //}

                foreach (var processTitle in process.Titles)
                {
                    if (!target.Titles.Contains(processTitle))
                    {
                        target.Titles = new List<ProcessLib.Models.ProcessTitle>(target.Titles);
                        target.Titles.Add(processTitle);
                    }
                }
            }
            target.Titles = target.Titles.OrderBy(x => x.StartTime).ThenBy(x => x.ID).ToList();
        }


        public static void ApplyProcess(ProcessLib.Interfaces.IProcess process, Process proc, DateTime now)
        {
            if (process == null)
                return;
            if (proc == null)
                return;
            var p = new ProcessLib.Models.Process();
            try
            {
                CopyFrom(process, p);

                var machineName = proc.MachineName != "."
                    ? proc.MachineName
                    : Environment.MachineName;

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
                p.Titles = p.Titles ?? new List<ProcessLib.Models.ProcessTitle>();
                var prevTitle = p.Titles.OrderByDescending(x => x.StartTime).FirstOrDefault();
                if (prevTitle == null || prevTitle.ProcessID == p.ID)
                {
                    if (prevTitle == null || prevTitle.Title != proc.MainWindowTitle)
                    {
                        if (prevTitle != null && !prevTitle.EndTime.HasValue)
                            prevTitle.EndTime = now;
                        if (!string.IsNullOrWhiteSpace(proc.MainWindowTitle))
                        {
                            var title = new ProcessLib.Models.ProcessTitle
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


        public static TimeSpan CalculateDuration(ProcessLib.Interfaces.IProcess process)
        {
            var diff = TimeSpan.Zero;
            if (process != null)
                diff = CalculateDuration(process.StartTime, process.ExitTime);
            return diff;
        }

        public static TimeSpan CalculateDuration(DateTime? start, DateTime? end)
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
