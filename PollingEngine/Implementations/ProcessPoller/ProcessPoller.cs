using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MyLife.API.Client;
using PollingEngine.Core;
using ProcessLib;
using ProcessLib.Interfaces;

namespace ProcessPoller
{
    public class ProcessPoller : IPollingProgram
    {
        private string _machineName;
        private IProcessPollerSettings _settings;
        private Simple.OData.Client.ODataClient _client;
        private IProcessMonitor _processMonitor;

        public ProcessPoller()
            //: this(new ProcessPollerSettings())
            : this(ProcessPollerSettingsConfigElement.LoadFromConfig())
        {
            
        }

        public ProcessPoller(IProcessPollerSettings settings)
        {
            _ApplySettings(settings);
        }


        public virtual async Task ApplySettings(IProcessPollerSettings settings)
        {
            if (_processMonitor != null)
            {
                await _processMonitor.Stop();
            }

            _ApplySettings(settings);
        }

        private void _ApplySettings(IProcessPollerSettings settings)
        {
            _settings = settings;
            _machineName = _settings.MachineName;
            var uri = new Uri(_settings.DataApiBaseUrl);
            _client = new Simple.OData.Client.ODataClient(uri);
            IProcessRepository processRepository = new ProcessRepository(_client);
            IProcessRetriever processRetriever = new ProcessRetriever(_machineName);
            if (_settings.ProcessFilter != null)
                processRetriever = new FilteredProcessRetriever(processRetriever, _settings.ProcessFilter);
            _processMonitor = new ProcessMonitor(processRepository, processRetriever);
        }


        public virtual async Task OnStarting(PollingContext context)
        {
            await _processMonitor.Init();
        }

        public virtual async Task OnInterval(PollingContext context)
        {
            await _processMonitor.Work();
        }

        public virtual async Task OnStopping(PollingContext context)
        {
            await _processMonitor.Stop();
        }


        public virtual void ApplyArguments(string[] args)
        {
            
        }
        

        protected virtual void Log(string message)
        {
            Console.WriteLine(message);
            Trace.WriteLine(message);
        }
        

    }
}
