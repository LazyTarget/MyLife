using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PollingEngine.Core;
using SharedLib;
using SteamLib;
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
            _manager = new SteamManager(_settings.ConnString);
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
            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = Settings.CultureInfo;

            await UpdateReports();
            
            await UpdateSubscriptions();
        }



        private async Task UpdateReports()
        {
            var reports = await _manager.ReportManager.GetReports(TimeRange.All);
            foreach (var report in reports)
            {
                if (report == null || report.ID <= 0)
                    continue;
                if (!report.Enabled)
                    continue;
                await _manager.ReportManager.RefreshReport(report);
                Console.WriteLine("Updated report '{0}' for user #{1}", report.Name, report.UserID);
            }
        }
        

        private async Task UpdateSubscriptions()
        {
            var subscriptions = await _manager.ReportManager.GetReportSubscriptions();
            foreach (var subscription in subscriptions)
            {
                if (subscription.Deleted)
                    continue;
                if (!subscription.Enabled)
                    continue;
                
                var timeRange = GetTimeRange(DateTime.UtcNow, subscription.PeriodType);
                var reports = await _manager.ReportManager.GetReports(timeRange, subscription.UserID);

                var shouldCreate = reports != null && reports.All(x => x.SubscriptionID != subscription.ID);
                if (shouldCreate)
                {
                    var template = await _manager.ReportManager.GetReportTemplate(subscription.TemplateID);
                    if (template != null)
                    {
                        var filterSet = new SteamReportFilterSet();
                        filterSet.Filters = template.FilterSet.Filters;
                        filterSet.ID = template.FilterSet.ID;       // reuse filterSet?

                        var templateName = FormatTemplateNameForSubscription(template.Name, timeRange, subscription.PeriodType);

                        var generateRequest = new SteamReportGenerationRequest
                        {
                            Name = templateName,
                            Description = template.Description,
                            StartTime = timeRange.StartTime,
                            EndTime = timeRange.EndTime,
                            UserID = subscription.UserID,
                            FilterSet = filterSet,
                            SubscriptionID = subscription.ID,
                            Enabled = true,
                        };
                        var report = await _manager.ReportManager.GenerateReport(generateRequest);
                        Console.WriteLine("Created report '{0}' from subscription", report.Name);
                    }
                    else
                        Console.WriteLine("Could not create report, template not found. TemplateID: {0}, SubscriptionID: {1}", subscription.TemplateID, subscription.ID);
                }

            }
        }


        private TimeRange GetTimeRange(DateTime now, ReportPeriodType type)
        {
            TimeRange timeRange;
            switch (type)
            {
                case ReportPeriodType.Hourly:
                    timeRange = now.GetTimeRange(TimePeriod.Hour, Settings.FirstDayOfWeek);
                    break;
                case ReportPeriodType.Daily:
                    timeRange = now.GetTimeRange(TimePeriod.Day, Settings.FirstDayOfWeek);
                    break;
                case ReportPeriodType.Weekly:
                    timeRange = now.GetTimeRange(TimePeriod.Week, Settings.FirstDayOfWeek);
                    break;
                case ReportPeriodType.Monthly:
                    timeRange = now.GetTimeRange(TimePeriod.Month, Settings.FirstDayOfWeek);
                    break;
                case ReportPeriodType.Quarterly:
                    timeRange = now.GetTimeRange(TimePeriod.Quarter, Settings.FirstDayOfWeek);
                    break;
                case ReportPeriodType.Yearly:
                    timeRange = now.GetTimeRange(TimePeriod.Year, Settings.FirstDayOfWeek);
                    break;
                default:
                    throw new NotImplementedException(String.Format("ReportPeriodType '{0}' not implemented", type));
            }
            return timeRange;
        }


        private string FormatTemplateNameForSubscription(string name, TimeRange timeRange, ReportPeriodType periodType)
        {
            string res;
            switch (periodType)
            {
                case ReportPeriodType.Hourly:
                    res = String.Format("{0} [{1}, {2}-{3}]",
                                        name, timeRange.StartTime.ToShortDateString(),
                                        timeRange.StartTime.ToString("HH:mm"),
                                        timeRange.EndTime.ToString("HH:mm"));
                    break;

                case ReportPeriodType.Daily:
                    res = String.Format("{0} [{1}]", name, timeRange.StartTime.ToShortDateString());
                    break;

                case ReportPeriodType.Weekly:
                    var calendar = Settings.CultureInfo.Calendar;
                    var week = calendar.GetWeekOfYear(timeRange.StartTime, CalendarWeekRule.FirstFourDayWeek, Settings.FirstDayOfWeek);
                    res = String.Format("{0} [{1} w.{2}]", name, timeRange.StartTime.ToString("yyyy"), week);
                    break;

                case ReportPeriodType.Monthly:
                    res = String.Format("{0} [{1} {2}]", name, timeRange.StartTime.ToString("yyyy"), timeRange.StartTime.ToString("MM"));
                    break;

                case ReportPeriodType.Quarterly:
                    var quarter = timeRange.StartTime.GetQuarter();
                    res = String.Format("{0} [{1} Q{2}]", name, timeRange.StartTime.ToString("yyyy"), quarter);
                    break;

                case ReportPeriodType.Yearly:
                    res = String.Format("{0} [{1}]", name, timeRange.StartTime.ToString("yyyy"));
                    break;

                default:
                    return name;
            }
            return res;
        }


    }

}
