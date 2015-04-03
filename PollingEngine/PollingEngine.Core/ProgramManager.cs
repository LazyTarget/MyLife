using System;
using System.Collections.Generic;
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
                    Console.WriteLine("Starting program: " + progName);
                    try
                    {
                        prog.OnStarting(context);
                        context.State = State.Running;
                        context.TimeStarted = DateTime.Now;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error starting program '{0}': {1}", progName, ex.Message);
                    }
                }
                else if (context.State == State.Running)
                {
                    Console.WriteLine("OnInterval for program: " + progName);
                    try
                    {
                        prog.OnInterval(context);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error in program '{0}': {1}", progName, ex.Message);
                    }
                    Thread.Sleep(context.Interval);
                }
                else if (context.State == State.Stopping)
                {
                    Console.WriteLine("Stopping program: " + progName);
                    try
                    {
                        prog.OnStopping(context);
                        context.TimeStopped = DateTime.Now;
                        context.State = State.Stopped;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error stopping program '{0}': {1}", progName, ex.Message);
                    }
                }
                else if (context.State == State.Stopped)
                {
                    
                }
            }
            Console.WriteLine("Program exited: " + progName);
        }


        public void Exit()
        {
            Console.WriteLine("Setting should exit");
            _shouldExit = true;
        }


    }
}
