using System;
using System.Collections.Generic;
using PollingEngine.Core;

namespace PollingEngine
{
    class Program
    {
        static void Main(string[] args)
        {
            // todo: create as winservice

            var manager = new ProgramManager();
            var contexts = new List<PollingContext>();
            //contexts.Add(new PollingContext(new SteamPoller.SteamPoller(), TimeSpan.FromSeconds(15)));
            contexts.Add(new PollingContext(new XbmcPoller.XbmcPoller(), TimeSpan.FromSeconds(5)));

            manager.Load(contexts);
            manager.Start();

            Console.WriteLine("Programs started");

            while (true)
            {
                Console.WriteLine();
                var input = Console.ReadLine();
                if (input == "exit")
                    manager.Exit();
                else if (input == "end")
                    break;
            }

            Console.WriteLine("Program will exit after [enter]");
            Console.ReadLine();
        }
    }
}
