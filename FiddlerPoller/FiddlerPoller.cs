using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fiddler;
using PollingEngine.Core;
using SharedLib;

namespace FiddlerPoller
{
    public class FiddlerPoller : IPollingProgram
    {
        private readonly DisposableLock _actionLock = new DisposableLock();
        private readonly List<string> _filters = new List<string>();
        private bool _initialized;
        private bool _stopped;


        public FiddlerPoller()
        {
            _filters.Add("vshub");
        }


        public async Task OnStarting(PollingContext context)
        {
            using (await _actionLock.EnterAsync())
            {
                if (_initialized)
                    return;
                Log("Initializing...");
                await DoInit();
                Log("Initialized...");
                _initialized = true;
                _stopped = false;
            }
        }

        private async Task DoInit()
        {
            var port = 8877;

            var flags = FiddlerCoreStartupFlags.Default;

            flags = FiddlerCoreStartupFlags.RegisterAsSystemProxy | FiddlerCoreStartupFlags.DecryptSSL |
                    FiddlerCoreStartupFlags.ChainToUpstreamGateway | FiddlerCoreStartupFlags.MonitorAllConnections |
                    FiddlerCoreStartupFlags.CaptureLocalhostTraffic | FiddlerCoreStartupFlags.OptimizeThreadPool;

            flags = FiddlerCoreStartupFlags.RegisterAsSystemProxy | //FiddlerCoreStartupFlags.DecryptSSL |
                    FiddlerCoreStartupFlags.ChainToUpstreamGateway | FiddlerCoreStartupFlags.MonitorAllConnections |
                    FiddlerCoreStartupFlags.CaptureLocalhostTraffic | FiddlerCoreStartupFlags.OptimizeThreadPool;

            FiddlerApplication.Startup(port, flags);

            FiddlerApplication.OnWebSocketMessage += Fiddler_OnWebSocketMessage;
            FiddlerApplication.OnNotification += Fiddler_OnNotification;
            FiddlerApplication.ResponseHeadersAvailable += Fiddler_OnResponseHeadersAvailable;
            FiddlerApplication.RequestHeadersAvailable += Fiddler_OnRequestHeadersAvailable;
        }


        public async Task OnInterval(PollingContext context)
        {
            using (await _actionLock.EnterAsync())
            {
                if (_stopped)
                {
                    Log("Won't work, has stopped");
                    return;
                }
                if (!_initialized)
                {
                    Log("Won't work, not initialized");
                    return;
                }
                await DoWork();
            }
        }


        private async Task DoWork()
        {
            
        }


        public async Task OnStopping(PollingContext context)
        {
            using (await _actionLock.EnterAsync())
            {
                if (_stopped)
                    return;
                Log("Stopping...");
                await DoStop();
                Log("Stopped...");
                _initialized = false;
                _stopped = true;
            }
        }
        
        private async Task DoStop()
        {
            FiddlerApplication.OnWebSocketMessage -= Fiddler_OnWebSocketMessage;
            FiddlerApplication.OnNotification -= Fiddler_OnNotification;
            FiddlerApplication.ResponseHeadersAvailable -= Fiddler_OnResponseHeadersAvailable;
            FiddlerApplication.RequestHeadersAvailable -= Fiddler_OnRequestHeadersAvailable;

            FiddlerApplication.Shutdown();
        }



        protected virtual void Log(string message)
        {
            Console.WriteLine(message);
            System.Diagnostics.Trace.WriteLine(message);
        }

        public void ApplyArguments(string[] args)
        {
            
        }


        private void Fiddler_OnNotification(object sender, NotificationEventArgs e)
        {
            Log("OnNotification: " + e.NotifyString);
        }

        private void Fiddler_OnWebSocketMessage(object sender, WebSocketMessageEventArgs e)
        {
            Log("OnWebSocketMessage: " + e.oWSM);
        }

        private void Fiddler_OnRequestHeadersAvailable(Session session)
        {
            if (_filters.Any(x => session.fullUrl.Contains(x)))
                return;
            Log("OnRequestHeadersAvailable: " + session);
        }

        private void Fiddler_OnResponseHeadersAvailable(Session session)
        {
            if (_filters.Any(x => session.fullUrl.Contains(x)))
                return;
            Log("OnResponseHeadersAvailable: " + session);
        }
    }
}
