﻿using System;
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
        private readonly Dictionary<DateTime, List<ProcessRunInfo>> _rawData = new Dictionary<DateTime, List<ProcessRunInfo>>();


        public async Task OnStarting(PollingContext context)
        {
            
        }

        public async Task OnInterval(PollingContext context)
        {
            Log("Polling processes");
            var time = DateTime.Now;
            var processes = Process.GetProcesses();

            var preList = _rawData.Select(x => x.Value).LastOrDefault();
            var curList = new List<ProcessRunInfo>();
            curList.AddRange(processes.Select(ProcessRunInfo.FromProcess).Where(x => x != null));

            try
            {
                while (_rawData.Count > 0)
                {
                    var k = _rawData.OrderBy(x => x.Key).Select(x => x.Key).First();
                    var l = _rawData[k];
                    while (l.Count > 0)
                    {
                        var index = 0;
                        var p = l[index];
                        l.RemoveAt(index);
                        p.Dispose();
                    }
                    _rawData.Remove(k);
                }
            }
            catch (Exception ex)
            {
                Log("Error disposing old PorcessRunInfo. Error: " + ex);
            }
            

            // "merge" + "diff" logic...

            var exited = new List<ProcessRunInfo>();
            var started = new List<ProcessRunInfo>();

            if (preList != null)
            {
                exited.AddRange(
                    preList.Where(
                        p =>
                            p.HasExited ||
                            curList.All(x => x.ProcessID != p.ProcessID) ||
                            (curList.Any(x => x.ProcessID == p.ProcessID) &&
                             curList.First(x => x.ProcessID == p.ProcessID).HasExited)));

                started.AddRange(
                    curList.Where(
                        p => preList.All(x => x.ProcessID != p.ProcessID)
                        ));
            }
            exited.AddRange(curList.Where(x => x.HasExited));
            



            foreach (var processRunInfo in exited)
            {
                OnProcessExited(processRunInfo);
            }

            foreach (var processRunInfo in started)
            {
                OnProcessStarted(processRunInfo);
            }


            _rawData.Add(time, curList);
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




        private void OnProcessStarted(ProcessRunInfo processRunInfo)
        {
            try
            {
                var msg = string.Format("{0} has started, duration: {1}", processRunInfo.ProcessName, TimeSpanToString(processRunInfo.Duration));
                Log(msg);


                //var machineName = processRunInfo.MachineName != "."
                //    ? processRunInfo.MachineName
                //    : Environment.MachineName;

                //Debug.WriteLine("Begin creating odbc entry");

                //var connectionString = ConfigurationManager.ConnectionStrings["PollingDatabase"].ConnectionString;
                //var cn = new OdbcConnection(connectionString);
                //if (cn.State != ConnectionState.Open)
                //    cn.Open();

                //var sql = "INSERT INTO ProcessEvents(ProcessID, ProcessName, MachineName, HasExited, StartTime, EndTime, ExitCode) " +
                //          "VALUES (?, ?, ?, ?, ?, ?, ?)";
                //var cmd = new OdbcCommand(sql, cn);
                //cmd.Parameters.AddWithValue("@ProcessID", processRunInfo.ProcessID);
                //cmd.Parameters.AddWithValue("@ProcessName", processRunInfo.ProcessName);
                //cmd.Parameters.AddWithValue("@MachineName", machineName);
                //cmd.Parameters.AddWithValue("@HasExited", processRunInfo.HasExited);
                //cmd.Parameters.AddWithValue("@StartTime", processRunInfo.StartTime);
                //cmd.Parameters.AddWithValue("@ExitTime", processRunInfo.HasExited ? (object)processRunInfo.ExitTime : DBNull.Value);
                //cmd.Parameters.AddWithValue("@ExitCode", processRunInfo.HasExited ? (object)processRunInfo.ExitCode : DBNull.Value);

                //var changes = cmd.ExecuteNonQuery();
                //if (changes > 0)
                //    Console.WriteLine("Created odbc event: '{0}'", processRunInfo);
                //else
                //    Console.WriteLine("No changes commited when creating odbc entry: " + processRunInfo);
            }
            catch (Exception ex)
            {
                Log("Failed to create odbc entry, Error: " + ex.Message);
                throw;
            }
        }


        private void OnProcessExited(ProcessRunInfo processRunInfo)
        {
            try
            {
                var msg = string.Format("{0} has exited, duration: {1}", processRunInfo.ProcessName, TimeSpanToString(processRunInfo.Duration));
                Log(msg);


                var machineName = processRunInfo.MachineName != "."
                    ? processRunInfo.MachineName
                    : Environment.MachineName;
                
                Log("Begin creating odbc entry");

                var connectionString = ConfigurationManager.ConnectionStrings["MyLifeDatabase"].ConnectionString;
                var cn = new OdbcConnection(connectionString);
                if (cn.State != ConnectionState.Open)
                    cn.Open();

                var sql = "INSERT INTO Process_Events(ProcessID, ProcessName, MachineName, HasExited, StartTime, ExitTime, ExitCode, MainWindowTitle, ModuleName, FileName) " +
                          "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
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

                var changes = cmd.ExecuteNonQuery();
                if (changes > 0)
                {
                    Log(String.Format("Created odbc event: '{0}'", processRunInfo));
                }
                else
                    Log("No changes commited when creating odbc entry: " + processRunInfo);
            }
            catch (Exception ex)
            {
                Log("Failed to create odbc entry, Error: " + ex.Message);
                throw;
            }
        }

        private void Log(string message)
        {
            Console.WriteLine(message);
            Trace.WriteLine(message);
        }


    }
}
