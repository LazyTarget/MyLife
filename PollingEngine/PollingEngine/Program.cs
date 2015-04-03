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
            var steamPoller = new SteamPoller.SteamPoller();
            contexts.Add(new PollingContext(steamPoller, TimeSpan.FromSeconds(15)));

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
