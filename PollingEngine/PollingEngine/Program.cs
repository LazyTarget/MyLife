﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using PollingEngine.Core;

namespace PollingEngine
{
    class Program
    {
        static void Main(string[] args)
        {
            // todo: create as winservice

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_OnUnhandledException;


            if (!Environment.UserInteractive)
            {
                // If run via Service Control Manager
                StartService(args);
            }
            else
            {
                AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_OnFirstChanceException;

                // If run via Explorer, Command prompt or other
                StartServiceWithConsole(args);
            }
        }


        private static void StartService(string[] args)
        {
            List<PollingContext> contexts;
            var manager = InitProgramManager(out contexts);
            var winService = new WinService(manager);
            var services = new System.ServiceProcess.ServiceBase[] { winService };
            System.ServiceProcess.ServiceBase.Run(services);
        }

        private static void StartServiceWithConsole(string[] args)
        {
            List<PollingContext> contexts;
            var manager = InitProgramManager(out contexts);
            manager.Start();
            Console.WriteLine("Programs started");

            string input;
            while (true)
            {
                Console.WriteLine();
                input = Console.ReadLine();
                if (input == "stop all")
                    manager.Exit(false);
                if (input == "exit")
                {
                    manager.Exit(true);
                    break;
                }
                else if (input == "end")
                    break;
                else if (input == "cls")
                    Console.Clear();
                else
                {
                    var parts = (input ?? "").Trim().Split(' ');
                    if (parts.Length >= 3)
                    {
                        var verb = parts[0];
                        var subject = parts[1];
                        var value = parts[2];


                        if (verb == "start")
                        {
                            if (subject == "program")
                            {
                                var ctx = contexts.FirstOrDefault(x => x.Program.GetType().Name == value);
                                if (ctx != null)
                                    ctx.State = State.Starting;
                                else
                                    Console.WriteLine("Context '{0}' not found", value);
                            }
                        }
                        else if (verb == "stop")
                        {
                            if (subject == "program")
                            {
                                var ctx = contexts.FirstOrDefault(x => x.Program.GetType().Name == value);
                                if (ctx != null)
                                    ctx.State = State.Stopping;
                                else
                                    Console.WriteLine("Context '{0}' not found", value);
                            }
                        }
                        else if (verb == "get")
                        {
                            if (subject == "program")
                            {
                                var ctx = contexts.FirstOrDefault(x => x.Program.GetType().Name.Equals(value, StringComparison.InvariantCultureIgnoreCase));
                                if (ctx != null)
                                {
                                    Console.WriteLine();
                                    Console.WriteLine("--Program info-- ");
                                    Console.WriteLine("Program: " + ctx.Program);
                                    Console.WriteLine("State: " + ctx.State);
                                    Console.WriteLine("Interval: " + ctx.Interval);
                                    Console.WriteLine("IntervalSequence: " + ctx.IntervalSequence);
                                    Console.WriteLine("TimeStarted: " + ctx.TimeStarted);
                                    Console.WriteLine("TimeStopped: " + ctx.TimeStopped);
                                    Console.WriteLine("TimeRunning: " + ctx.TimeRunning);
                                    Console.WriteLine();
                                    Console.WriteLine();
                                }
                                else
                                    Console.WriteLine("Context '{0}' not found", value);
                            }
                        }
                        else if (verb == "set")
                        {
                            if (subject == "progconfig")
                            {
                                var ctx = contexts.FirstOrDefault(x => x.Program.GetType().Name.Equals(value, StringComparison.InvariantCultureIgnoreCase));
                                if (ctx != null)
                                {
                                    var configArgs = parts.Skip(3).ToArray();

                                    Console.WriteLine();
                                    Console.WriteLine("--Pushing configs to program-- ");
                                    Console.WriteLine("Args: " + string.Join(" ", configArgs));
                                    Console.WriteLine("Program: " + ctx.Program);
                                    ctx.Program.ApplyArguments(configArgs);

                                    Console.WriteLine();
                                }
                                else
                                    Console.WriteLine("Context '{0}' not found", value);
                            }
                        }

                    }
                }
            }

            if (input != "exit")
            {
                Console.WriteLine("Program will exit after [enter]");
                Console.ReadLine();
            }
        }

        internal static ProgramManager InitProgramManager(out List<PollingContext> contexts)
        {
            var manager = new ProgramManager();
            contexts = new List<PollingContext>();
            //var pollers = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x=>x.ExportedTypes.Where(y=>y.GetInterfaces().Contains(typeof(IPollingProgram))));

            var pollingEngineConfigSection = PollingEngineConfigSection.LoadFromConfig();
            foreach (IPollingProgramGeneralSettings poller in pollingEngineConfigSection.Pollers)
            {
                if (!poller.Enabled)
                {
                    Debug.WriteLine("Ignoring PollingProgram '{0}' as it has Enabled set to false", poller.Type);
                    continue;
                }
                var type = Type.GetType(poller.Type);
                if (type == null)
                {
                    Console.WriteLine("Ignoring PollingProgram '{0}' as it could not find the Type", poller.Type);
                    continue;
                }
                var prog = Activator.CreateInstance(type);
                var pollingProgram = (IPollingProgram) prog;
                var pollingContext = new PollingContext(pollingProgram, poller.Interval);
                contexts.Add(pollingContext);
            }
            
            //contexts.Add(new PollingContext(new SteamPoller.SteamPoller2(),     TimeSpan.FromSeconds(Convert.ToInt32(ConfigurationManager.AppSettings.Get("Program.SteamPoller2")))));
            //contexts.Add(new PollingContext(new XbmcPoller.XbmcPoller(),        TimeSpan.FromSeconds(Convert.ToInt32(ConfigurationManager.AppSettings.Get("Program.XbmcPoller")))));
            contexts.Add(new PollingContext(new ProcessPoller.ProcessPoller(),  TimeSpan.FromSeconds(Convert.ToInt32(ConfigurationManager.AppSettings.Get("Program.ProcessPoller")))));


            manager.Load(contexts);
            return manager;
        }

        private static void CurrentDomain_OnFirstChanceException(object sender, FirstChanceExceptionEventArgs args)
        {
            Trace.WriteLine("OnFirstChanceException:");
            Trace.WriteLine(args.Exception);
        }

        private static void CurrentDomain_OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            Trace.WriteLine("OnUnhandledException:");
            Trace.WriteLine(args.ExceptionObject);
        }
    }
}
