using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PollingEngine.Core;
using PortableSteam;
using PortableSteam.Interfaces.General.ISteamUser;
using PortableSteam.Interfaces.General.ISteamUserStats;
using SharedLib;
using SteamLib.Models;

namespace SteamPoller
{
    public class SteamReportPoller : IPollingProgram
    {
        private PollingContext _context;
        private ISteamReportPollerSettings _settings;
        private ISteamManager _manager;


        public SteamReportPoller()
        {
            _settings = SteamReportPollerSettingsConfigElement.LoadFromConfig();
        }

        public ISteamReportPollerSettings Settings
        {
            get { return _settings; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _settings = value;
            }
        }


        public async Task OnStarting(PollingContext context)
        {
            _context = context;
            //_odbc = new OdbcConnection(_settings.ConnString);
            //if (_odbc.State != ConnectionState.Open)
            //    _odbc.Open();
        }

        public async Task OnInterval(PollingContext context)
        {
            _context = context;
            await Poll();
        }

        public async Task OnStopping(PollingContext context)
        {
            _context = context;
        }

        public void ApplyArguments(string[] args)
        {
            if (args.Length >= 2)
            {
                var verb = args[0].ToLower();
                var subject = args[1].ToLower();
                var value = args.Length >= 3 ? args[2] : null;

                if (verb == "poll")
                {
                    var thread = new Thread(() => Poll());
                    thread.Start();
                }
            }
        }


        private async Task Poll()
        {
            await UpdateReports();
            
            await UpdateSubscriptions();
        }



        private async Task UpdateReports()
        {
            var reports = await _manager.ReportManager.GetReports(TimePeriod.All);
            throw new NotImplementedException();
        }

        private Task UpdateSubscriptions()
        {
            throw new NotImplementedException();
        }
        

    }

}
