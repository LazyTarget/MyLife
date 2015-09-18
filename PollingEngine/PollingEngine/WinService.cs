using PollingEngine.Core;

namespace PollingEngine
{
    /// <summary>
    /// Class exposed for Windows Service
    /// </summary>
    public class WinService : System.ServiceProcess.ServiceBase
    {
        public const string ServiceName = "PollingEngine";
        public const string DisplayName = "PollingEngine";
        public const string Description = "Windows Service which polls various data sources for data and stores them to a seperate database // Peter Åslund";

        internal ProgramManager ProgramManager { get; private set; }


        internal WinService(ProgramManager programManager)
        {
            ProgramManager = programManager;
        }


        protected override void OnStart(string[] args)
        {
            ProgramManager.Start();
            base.OnStart(args);
        }

        protected override void OnPause()
        {
            //programManager.Pause();
            base.OnPause();
        }

        protected override void OnContinue()
        {
            //programManager.Continue();
            base.OnContinue();
        }

        protected override void OnStop()
        {
            ProgramManager.Exit(false);
            base.OnStop();
        }

    }
}
