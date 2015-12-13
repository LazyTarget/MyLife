using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Odbc;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using PollingEngine.Core;

namespace ProcessPoller
{
    public class ProcessPoller : IPollingProgram
    {
        private List<ProcessEntity> _preList = new List<ProcessEntity>();


        public async Task OnStarting(PollingContext context)
        {
            _preList = LoadProcesses().ToList();
        }


        public async Task OnInterval(PollingContext context)
        {
            Log("Polling processes");
            var processes = Process.GetProcesses();

            var preList = _preList;
            var curList = new List<ProcessRunInfo>();
            curList.AddRange(processes.Select(ProcessRunInfo.FromProcess).Where(x => x != null));

            

            // "merge" + "diff" logic...

            //var exited = new List<ProcessRunInfo>();
            //var started = new List<ProcessRunInfo>();

            //if (preList != null)
            //{
            //    exited.AddRange(
            //        preList.Where(
            //            p =>
            //                p.HasExited ||
            //                curList.All(x => x.ProcessID != p.ProcessID) ||
            //                (curList.Any(x => x.ProcessID == p.ProcessID) &&
            //                 curList.First(x => x.ProcessID == p.ProcessID).HasExited)));

            //    started.AddRange(
            //        curList.Where(
            //            p => preList.All(x => x.ProcessID != p.ProcessID)
            //            ));
            //}
            //exited.AddRange(curList.Where(x => x.HasExited));
            

            //foreach (var processRunInfo in exited)
            //{
            //    OnProcessExited(processRunInfo);
            //}

            //foreach (var processRunInfo in started)
            //{
            //    OnProcessStarted(processRunInfo);
            //}


            var resList = new List<ProcessEntity>();
            foreach (var processRunInfo in curList)
            {
                var proc = _preList.FirstOrDefault(x => x.ProcessInfo.ProcessID == processRunInfo.ProcessID &&
                                                        x.ProcessInfo.ProcessName == processRunInfo.ProcessName);
                if (proc == null)
                {
                    // Started
                    proc = new ProcessEntity
                    {
                        ProcessInfo = processRunInfo,
                    };
                    OnProcessStarted(proc);
                }
                else
                {
                    // Continue or Exit
                    proc.ProcessInfo = processRunInfo;      // update data

                    if (proc.ProcessInfo.HasExited)
                        OnProcessExited(proc);
                    else
                        StoreProcess(proc);
                }
                resList.Add(proc);
            }


            // prev processes
            foreach (var process in preList)
            {
                if (resList.Any(x => x.ID == process.ID))
                    continue;

                // Continue or Exit
                try
                {
                    var proc = Process.GetProcessById(process.ProcessInfo.ProcessID);
                    process.ProcessInfo = ProcessRunInfo.FromProcess(proc); // update data
                }
                catch (Exception ex)
                {
                    // process is not found (has exited)
                    //process.ProcessInfo.HasExited = true;
                }


                if (process.ProcessInfo.HasExited)
                {
                    OnProcessExited(process);
                    process.ProcessInfo.Dispose();
                    process.ProcessInfo = null;
                }
                else
                {
                    StoreProcess(process);
                    resList.Add(process);
                }
            }


            _preList = resList;
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
            else
                res += string.Format("{0}h {1}m", duration.Hours, duration.Minutes);
            return res;
        }




        private void OnProcessStarted(ProcessEntity process)
        {
            try
            {
                var processRunInfo = process.ProcessInfo;
                var msg = string.Format("{0} has started, duration: {1}", processRunInfo.ProcessName, TimeSpanToString(processRunInfo.Duration));
                Log(msg);

                StoreProcess(process);
            }
            catch (Exception ex)
            {
                Log("Failed to create odbc entry, Error: " + ex.Message);
                throw;
            }
        }


        private void OnProcessExited(ProcessEntity process)
        {
            try
            {
                var processRunInfo = process.ProcessInfo;
                var msg = string.Format("{0} has exited, duration: {1}", processRunInfo.ProcessName, TimeSpanToString(processRunInfo.Duration));
                Log(msg);

                StoreProcess(process);
            }
            catch (Exception ex)
            {
                //Log("Failed to create odbc entry, Error: " + ex.Message);
                throw;
            }
        }


        private void StoreProcess(ProcessEntity process)
        {
            try
            {
                var processRunInfo = process.ProcessInfo;
                var machineName = processRunInfo.MachineName != "."
                    ? processRunInfo.MachineName
                    : Environment.MachineName;

                var procID = process.ID;
                //if (procID <= 0 || process.ProcessInfo.HasExited)
                //    Log("Begin creating/updating odbc entry");

                var connectionString = ConfigurationManager.ConnectionStrings["MyLifeDatabase"].ConnectionString;
                var cn = new OdbcConnection(connectionString);
                if (cn.State != ConnectionState.Open)
                    cn.Open();

                int changes;
                if (process.ID <= 0)
                {
                    var sql = "INSERT INTO Process_Events(ProcessID, ProcessName, MachineName, HasExited, StartTime, ExitTime, ExitCode, MainWindowTitle, ModuleName, FileName) " +
                              "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?); SELECT ? = @@IDENTITY";
                    var cmd = new OdbcCommand(sql, cn);
                    cmd.Parameters.AddWithValue("@ProcessID", processRunInfo.ProcessID);
                    cmd.Parameters.AddWithValue("@ProcessName", processRunInfo.ProcessName);
                    cmd.Parameters.AddWithValue("@MachineName", machineName);
                    cmd.Parameters.AddWithValue("@HasExited", processRunInfo.HasExited);
                    cmd.Parameters.AddWithValue("@StartTime", processRunInfo.StartTime);
                    cmd.Parameters.AddWithValue("@ExitTime", processRunInfo.HasExited ? (object) processRunInfo.ExitTime : DBNull.Value);
                    cmd.Parameters.AddWithValue("@ExitCode", processRunInfo.ExitCode.HasValue ? (object)processRunInfo.ExitCode.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@MainWindowTitle", !string.IsNullOrEmpty(processRunInfo.MainWindowTitle) ? (object) processRunInfo.MainWindowTitle : DBNull.Value);
                    cmd.Parameters.AddWithValue("@ModuleName", !string.IsNullOrEmpty(processRunInfo.ModuleName) ? (object) processRunInfo.ModuleName : DBNull.Value);
                    cmd.Parameters.AddWithValue("@FileName", !string.IsNullOrEmpty(processRunInfo.FileName) ? (object) processRunInfo.FileName : DBNull.Value);
                    var idParam = new OdbcParameter("@EntityID", OdbcType.BigInt, 4)
                    {
                        Direction = ParameterDirection.Output,
                    };
                    cmd.Parameters.Add(idParam);
                    changes = cmd.ExecuteNonQuery();
                    process.ID = (long)idParam.Value;
                }
                else
                {
                    var sql = "UPDATE Process_Events " +
                              "SET  HasExited = ? " +
                              "     ,ExitTime = ? " +
                              "     ,ExitCode = ? " +
                              (!string.IsNullOrEmpty(processRunInfo.MainWindowTitle)
                                  ? "     ,MainWindowTitle = ? "
                                  : "") +
                              " WHERE ID = ? ";
                    var cmd = new OdbcCommand(sql, cn);
                    cmd.Parameters.AddWithValue("@HasExited", processRunInfo.HasExited);
                    //cmd.Parameters.AddWithValue("@ExitTime", processRunInfo.HasExited ? (object) processRunInfo.ExitTime : DBNull.Value);
                    cmd.Parameters.AddWithValue("@ExitTime", processRunInfo.ExitTime);
                    cmd.Parameters.AddWithValue("@ExitCode", processRunInfo.ExitCode.HasValue ? (object)processRunInfo.ExitCode.Value : DBNull.Value);
                    if (!string.IsNullOrEmpty(processRunInfo.MainWindowTitle))
                        cmd.Parameters.AddWithValue("@MainWindowTitle", !string.IsNullOrEmpty(processRunInfo.MainWindowTitle) ? (object) processRunInfo.MainWindowTitle : DBNull.Value);
                    cmd.Parameters.AddWithValue("@EntityID", process.ID);
                    changes = cmd.ExecuteNonQuery();
                }

                if (changes > 0)
                {
                    //if (procID <= 0 || process.ProcessInfo.HasExited)
                    //    Log(String.Format("Created/updated odbc event: '{0}'", process));
                }
                else
                {
                    Log("No changes commited when creating odbc entry: " + process);
                }
            }
            catch (Exception ex)
            {
                Log("Failed to create odbc entry, Entry: " + process + ", Error: " + ex.Message);
                throw;
            }
        }


        private IEnumerable<ProcessEntity> LoadProcesses()
        {
            OdbcDataReader reader;
            try
            {
                Log("Loading stored processes");

                var machineName = Environment.MachineName;

                var connectionString = ConfigurationManager.ConnectionStrings["MyLifeDatabase"].ConnectionString;
                var cn = new OdbcConnection(connectionString);
                if (cn.State != ConnectionState.Open)
                    cn.Open();

                var sql = "SELECT * " +
                          "FROM Process_Events " +
                          "WHERE MachineName = ? AND HasExited = 0 " +
                          "ORDER BY StartTime";
                var cmd = new OdbcCommand(sql, cn);
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@MachineName", machineName);
                reader = cmd.ExecuteReader();
            }
            catch (Exception ex)
            {
                Log("Failed to create odbc entry, Error: " + ex.Message);
                throw;
            }

            using (reader)
            {
                var cID = reader.GetOrdinal("ID");
                var cProcessID = reader.GetOrdinal("ProcessID");
                var cProcessName = reader.GetOrdinal("ProcessName");
                var cMachineName = reader.GetOrdinal("MachineName");
                var cModuleName = reader.GetOrdinal("ModuleName");
                var cFileName = reader.GetOrdinal("FileName");
                var cMainWindowTitle = reader.GetOrdinal("MainWindowTitle");
                var cHasExited = reader.GetOrdinal("HasExited");
                var cExitCode = reader.GetOrdinal("ExitCode");
                var cStartTime = reader.GetOrdinal("StartTime");
                var cExitTime = reader.GetOrdinal("ExitTime");

                foreach (IDataRecord record in reader)
                {
                    var proc = new ProcessEntity();
                    proc.ID = record.GetInt64(cID);

                    var info = new CustomProcessRunInfo();
                    proc.ProcessInfo = info;

                    info.ProcessID = record.GetInt32(cProcessID);
                    info.ProcessName = record.GetString(cProcessName);
                    info.MachineName = record.GetString(cMachineName);
                    info.ModuleName = record.IsDBNull(cModuleName)
                        ? null
                        : record.GetString(cModuleName);
                    info.FileName = record.IsDBNull(cFileName)
                        ? null
                        : record.GetString(cFileName);
                    info.MainWindowTitle = record.IsDBNull(cMainWindowTitle)
                        ? null
                        : record.GetString(cMainWindowTitle);
                    info.HasExited = record.GetBoolean(cHasExited);
                    info.ExitCode = record.IsDBNull(cExitCode)
                        ? (int?) null
                        : record.GetInt32(cExitCode);
                    info.StartTime = record.GetDateTime(cStartTime);
                    info.ExitTime = record.IsDBNull(cExitTime)
                        ? DateTime.MinValue
                        : record.GetDateTime(cExitTime);
                    yield return proc;
                }
            }
        }


        private void Log(string message)
        {
            Console.WriteLine(message);
            Trace.WriteLine(message);
        }


    }
}
