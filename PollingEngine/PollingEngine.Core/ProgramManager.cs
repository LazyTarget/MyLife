﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace PollingEngine.Core
{
    public class ProgramManager
    {
        private List<PollingContext> _contexts;
        private bool _shouldExit;


        public void Load(IEnumerable<PollingContext> contexts)
        {
            _contexts = contexts.ToList();
        }



        public void Start()
        {
            _shouldExit = false;
            foreach (var ctx in _contexts)
            {
                ctx.State = State.Starting;
                var thread = new Thread(() => Run(ctx));
                thread.Start();
            }
        }


        private void Run(PollingContext context)
        {
            var progName = context.Program.GetType().Name;
            while (!_shouldExit)
            {
                var prog = context.Program;
                if (context.State == State.Starting)
                {
                    Console.WriteLine("Starting program: {0}. Interval: {1}", progName, context.Interval);
                    try
                    {
                        prog.OnStarting(context).Wait();
                        if (context.Interval.TotalSeconds > 0)
                        {
                            context.State = State.Running;
                            context.TimeStarted = DateTime.Now;
                            Console.WriteLine("Started program: " + progName);
                        }
                        else
                        {
                            context.State = State.Stopped;
                            Debug.WriteLine("Invalid interval for program: '{0}': {1}", progName, context.Interval);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error starting program '{0}': {1}", progName, ex.Message);
                        Debug.WriteLine(ex);
                        context.State = State.Stopping;     // stop program if failure to start
                    }
                }
                else if (context.State == State.Running)
                {
                    Debug.WriteLine("OnInterval for program: " + progName);
                    try
                    {
                        context.IntervalSequence++;
                        prog.OnInterval(context).Wait();
                        context.TimeRunning = context.TimeRunning.Add(context.Interval);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error in program '{0}': {1}", progName, ex.Message);
                        Debug.WriteLine(ex);
                    }
                    Wait(context.Interval);
                }
                else if (context.State == State.Stopping)
                {
                    Console.WriteLine("Stopping program: " + progName);
                    try
                    {
                        prog.OnStopping(context).Wait();
                        context.TimeStopped = DateTime.Now;
                        context.State = State.Stopped;
                        Console.WriteLine("Stopped program: " + progName);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error stopping program '{0}': {1}", progName, ex.Message);
                        Debug.WriteLine(ex);
                        context.State = State.Stopped;     // force stop program if failure to stop
                    }
                }
                else if (context.State == State.Stopped)
                {
                    //context.IntervalSequence = 0;
                    Wait(context.Interval);
                }
            }
            Console.WriteLine("Program exited: " + progName);
        }


        private void Wait(TimeSpan interval)
        {
            if (interval <= TimeSpan.Zero)
                interval = TimeSpan.FromSeconds(5);
            Thread.Sleep(interval);
        }


        public void Exit(bool force)
        {
            if (force)
            {
                Console.WriteLine("Setting should exit");
                _shouldExit = true;
            }
            else
            {
                Console.WriteLine("Requesting stop on all programs");
                foreach (var ctx in _contexts)      // todo: make change aware
                {
                    if (ctx.State == State.Running ||
                        ctx.State == State.Starting)
                    {
                        ctx.State = State.Stopping;
                    }
                    else
                    {
                        
                    }
                }
            }
        }


    }
}
