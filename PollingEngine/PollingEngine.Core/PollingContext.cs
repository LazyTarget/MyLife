using System;

namespace PollingEngine.Core
{
    public class PollingContext
    {
        public PollingContext(IPollingProgram program, TimeSpan interval)
        {
            Program = program;
            Interval = interval;
        }

        public IPollingProgram Program { get; private set; }

        public TimeSpan Interval { get; private set; }

        public State State { get; set; }
        
        public DateTime TimeStarted { get; set; }

        public DateTime TimeStopped { get; set; }

    }
}
