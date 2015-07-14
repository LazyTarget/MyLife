using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyLife.Core;
using MyLife.Models;
using OdbcWrapper;
using SteamPoller.Models;

namespace MyLife.Channels.SteamPoller
{
    public class SteamChannel : IEventChannel, IReportChannel
    {
        public static readonly Guid ChannelIdentifier = new Guid("a45faa27-4f86-4b89-85d6-131c8233df15");

        
        public Guid Identifier { get { return ChannelIdentifier; } }

        public SteamChannelSettings Settings { get; set; }


        public SteamChannel(string connectionString)
        {
            _odbc = new OdbcClient(connectionString);
            _reportFilterer = new SteamReportFilterer();
        }

        private readonly OdbcClient _odbc;
        private readonly SteamReportFilterer _reportFilterer;



        public async Task<IEnumerable<IEvent>> GetEvents(EventRequest request)
        {
            var sessions = await GetGamingSessions(request);
            var events = sessions.Select(ModelConverter.ToEvent).ToList();
            
            var reports = await GetReports(request);
            var reportEvents = reports.Select(x => x.ToEvent()).ToList();
            
            var result = new List<IEvent>();
            result.AddRange(events);
            result.AddRange(reportEvents);
            return events;
        }


        private async Task<IEnumerable<GamingSession>> GetGamingSessions(EventRequest request)
        {
            var sql = "SELECT * FROM Steam_GamingSessions " +
                      //"WHERE UserID = " + Settings.UserID;
                      "WHERE StartTime >= ? AND EndTime <= ?";
            var cmd = _odbc.CreateCommand();
            cmd.CommandText = sql;
            cmd.AddParam("@StartTime", request.StartTime);
            cmd.AddParam("@EndTime", request.EndTime);

            var dataTable = await cmd.ExecuteReader();
            var rows = dataTable.Rows.ToList();
            var dynamic = rows.Select(x => x.ToExpando()).ToList();
            var sessions = dynamic.Select(x => x.To<GamingSession>()).ToList();
            return sessions;
        }
        
        public async Task<IEnumerable<SteamReport>> GetReports(EventRequest request)
        {
            var sql = "SELECT * FROM Steam_Reports " +
                      //"WHERE UserID = " + Settings.UserID;
                      "WHERE StartTime >= ? AND EndTime <= ?";
            var cmd = _odbc.CreateCommand();
            cmd.CommandText = sql;
            cmd.AddParam("@StartTime", request.StartTime);
            cmd.AddParam("@EndTime", request.EndTime);

            var dataTable = await cmd.ExecuteReader();
            var rows = dataTable.Rows.ToList();
            var dynamic = rows.Select(x => x.ToExpando()).ToList();
            var reports = dynamic.Select(x =>
            {
                var report = x.To<SteamReport>();
                //if (report != null)
                //    report = await LoadReportExtraData(report);
                return report;
            });
            return reports;
        }


        async Task<IEnumerable<IReport>> IReportChannel.GetReports(EventRequest request)
        {
            var reports = await GetReports(request);
            return reports;
        }
        
        public async Task<SteamReport> GetReport(long id)
        {
            var sql = "SELECT * FROM Steam_Reports " +
                      "WHERE ID = ?";
            var cmd = _odbc.CreateCommand();
            cmd.CommandText = sql;
            cmd.AddParam("@ReportID", id);

            var dataTable = await cmd.ExecuteReader();
            var rows = dataTable.Rows.ToList();
            var dynamic = rows.Select(x => x.ToExpando()).ToList();
            var reports = dynamic.Select(x => x.To<SteamReport>()).ToList();
            var result = reports.FirstOrDefault();
            if (result != null)
                result = await LoadReportExtraData(result);
            return result;
        }

        private async Task<SteamReport> LoadReportExtraData(SteamReport report)
        {
            // Load filters
            var sql = "SELECT * FROM Steam_ReportFilters " +
                      "WHERE ReportID = ?";
            var cmd = _odbc.CreateCommand();
            cmd.CommandText = sql;
            cmd.AddParam("@ReportID", report.ID);

            var dataTable = await cmd.ExecuteReader();
            var rows = dataTable.Rows.ToList();
            var dynamic = rows.Select(x => x.ToExpando()).ToList();
            var filters = dynamic.Select(x => x.To<SteamReportFilter>()).ToList();
            report.Filters = filters;


            // Load sessions
            sql = "SELECT * FROM Steam_ReportSessions " +
                  "WHERE ReportID = ?";
            cmd = _odbc.CreateCommand();
            cmd.CommandText = sql;
            cmd.AddParam("@ReportID", report.ID);

            dataTable = await cmd.ExecuteReader();
            rows = dataTable.Rows.ToList();
            dynamic = rows.Select(x => x.ToExpando()).ToList();
            var sessionIDs = dynamic.Select(x => x.Get<long>("SessionID")).ToList();
            var allSessions = await GetGamingSessions(new EventRequest
            {
                StartTime = report.StartTime,
                EndTime = report.EndTime,
            });
            var sessions = allSessions.Where(x => sessionIDs.Contains(x.ID)).ToList();
            report.Sessions = sessions;

            return report;
        }

        public async Task<IReport> GenerateReport(IReportGenerationRequest request)
        {
            var reportID = request.ID.SafeConvert<long>();
            SteamReport report;
            if (reportID > 0)
            {
                report = await GetReport(reportID);
                if (report == null)
                    throw new Exception("Report could not be found");
            }
            else
                report = new SteamReport();

            report.Name = request.Name;
            report.Description = request.Description;
            report.StartTime = request.StartTime;
            report.EndTime = request.EndTime;
            report.LastModified = DateTime.UtcNow;

            var steamReportRequest = request as ISteamReportGenerationRequest;
            if (steamReportRequest != null)
            {
                report.Filters = steamReportRequest.Filters;
                // todo: report.EventFormatting
            }

            await StoreReport(report);
            await StoreReportFilters(report);


            var sessions = (await GetGamingSessions(new EventRequest
            {
                StartTime = report.StartTime,
                EndTime = report.EndTime,
            })).ToList();
            var result = _reportFilterer.GetFilterSessions(sessions, report.Filters);

            report.Sessions = result;

            await StoreReportSessions(report);
            
            return report;
        }



        private async Task StoreReport(SteamReport report)
        {
            report.LastModified = DateTime.UtcNow;

            string sql;
            var changes = 0;
            var cmd = _odbc.CreateCommand();
            if (report.ID <= 0)
            {
                // Create report
                cmd = _odbc.CreateCommand();
                sql = "INSERT INTO Steam_Reports(Name, Description, StartTime, EndTime, LastModified, LastGenerated) " +
                      "VALUES (?, ?, ?, ?, ?, ?); SELECT ? = @@IDENTITY";
                var reportIDParam = new OdbcParameter2
                {
                    Name = "@ReportID",
                    IsOutput = true,
                };
                cmd.CommandText = sql;
                cmd.AddParam("@Name", report.Name);
                cmd.AddParam("@Description", report.Description);
                cmd.AddParam("@StartTime", report.StartTime);
                cmd.AddParam("@EndTime", report.EndTime);
                cmd.AddParam("@LastModified", report.LastModified);
                cmd.AddParam("@LastGenerated", report.LastGenerated);
                cmd.Parameters.Add(reportIDParam);
                changes += await cmd.ExecuteNonQuery();
                report.ID = reportIDParam.Value.SafeConvert<long>();
            }
            else
            {
                // Update report
                cmd = _odbc.CreateCommand();
                sql = "UPDATE Steam_Reports " +
                      "SET  Name = ?," +
                      "     Description = ?," +
                      "     StartTime = ?," +
                      "     EndTime = ?," +
                      "     LastModified = ?," +
                      "     LastGenerated = ? " +
                      "WHERE ID = ?";
                cmd.CommandText = sql;
                cmd.AddParam("@Name", report.Name);
                cmd.AddParam("@Description", report.Description);
                cmd.AddParam("@StartTime", report.StartTime);
                cmd.AddParam("@EndTime", report.EndTime);
                cmd.AddParam("@LastModified", report.LastModified);
                cmd.AddParam("@LastGenerated", report.LastGenerated);
                cmd.AddParam("@ReportID", report.ID);
                changes += await cmd.ExecuteNonQuery();
            }
        }

        private async Task StoreReportFilters(SteamReport report)
        {
            string sql;
            var changes = 0;
            
            // Remove previous filters
            var cmd = _odbc.CreateCommand();
            sql = "DELETE FROM Steam_ReportFilters " +
                  "WHERE ReportID = ?";
            cmd.CommandText = sql;
            cmd.AddParam("@ReportID", report.ID);
            changes += await cmd.ExecuteNonQuery();


            if (report.Filters != null && report.Filters.Any())
            {
                foreach (var filter in report.Filters)
                {
                    if (filter.ID <= 0)
                    {
                        // Create report
                        cmd = _odbc.CreateCommand();
                        sql = "INSERT INTO Steam_ReportFilters(ReportID, GroupID, GroupRule, Attribute, Operator, Value) " +
                              "VALUES (?, ?, ?, ?, ?, ?); SELECT ? = @@IDENTITY";
                        var filterIDParam = new OdbcParameter2
                        {
                            Name = "@FilterID",
                            IsOutput = true,
                        };
                        cmd.CommandText = sql;
                        cmd.AddParam("@ReportID", report.ID);
                        cmd.AddParam("@GroupID", filter.GroupID);
                        cmd.AddParam("@GroupRule", filter.GroupRule.ToString());
                        cmd.AddParam("@Attribute", filter.Attribute.ToString());
                        cmd.AddParam("@Operator", filter.Operator.ToString());
                        cmd.AddParam("@Value", filter.Value);
                        cmd.Parameters.Add(filterIDParam);
                        changes += await cmd.ExecuteNonQuery();
                        filter.ID = filterIDParam.Value.SafeConvert<long>();
                    }
                    else
                    {
                        // Update report
                        cmd = _odbc.CreateCommand();
                        sql = "UPDATE Steam_ReportFilters " +
                              "SET  GroupID = ?," +
                              "     GroupRule = ?," +
                              "     Attribute = ?," +
                              "     Operator = ?," +
                              "     Value = ? " +
                              "WHERE ID = ?";
                        cmd.CommandText = sql;
                        cmd.AddParam("@GroupID", filter.GroupID);
                        cmd.AddParam("@GroupRule", filter.GroupRule.ToString());
                        cmd.AddParam("@Attribute", filter.Attribute.ToString());
                        cmd.AddParam("@Operator", filter.Operator.ToString());
                        cmd.AddParam("@Value", filter.Value);
                        cmd.AddParam("@FilterID", filter.ID);
                        changes += await cmd.ExecuteNonQuery();
                    }
                }

                report.LastModified = DateTime.UtcNow;
            }
        }

        private async Task StoreReportSessions(SteamReport report)
        {
            string sql;
            var changes = 0;

            // Clear previous filter result
            var cmd = _odbc.CreateCommand();
            sql = "DELETE FROM Steam_ReportSessions " +
                  "WHERE ReportID = ?";
            cmd.CommandText = sql;
            cmd.AddParam("@ReportID", report.ID);
            changes += await cmd.ExecuteNonQuery();

            foreach (var session in report.Sessions)
            {
                // Insert new filter result
                cmd = _odbc.CreateCommand();
                sql = "INSERT INTO Steam_ReportSessions(ReportID, SessionID) " +
                      "VALUES (?, ?);";
                cmd.CommandText = sql;
                cmd.AddParam("@ReportID", report.ID);
                cmd.AddParam("@SessionID", session.ID);
                changes += await cmd.ExecuteNonQuery();
            }
            
            report.LastGenerated = DateTime.UtcNow;
            report.LastModified = DateTime.UtcNow;
            await StoreReport(report);
        }


        

    }
}
