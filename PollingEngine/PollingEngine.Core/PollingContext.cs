using System;

namespace PollingEngine.Core
{
    public class PollingContext
    {
        private State _state;

        public PollingContext(IPollingProgram program, TimeSpan interval)
        {
            Program = program;
            Interval = interval;
        }

        public IPollingProgram Program { get; private set; }

        public TimeSpan Interval { get; private set; }

        public int IntervalSequence { get; internal set; }

        public State State
        {
            get { return _state; }
            set
            {
                if (_state == value)
                    return;
                _state = value;
                OnStateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public DateTime TimeStarted { get; set; }

        public DateTime TimeStopped { get; set; }
        
        public TimeSpan TimeRunning { get; set; }


        public event EventHandler OnStateChanged;

    }
}
