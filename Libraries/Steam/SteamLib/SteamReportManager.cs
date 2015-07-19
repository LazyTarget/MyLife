using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OdbcWrapper;
using SharedLib;
using SteamLib.Models;

namespace SteamLib
{
    public class SteamReportManager : ISteamReportManager
    {
        private readonly OdbcClient _odbc;
        private readonly ISteamActivityManager _activityManager;
        private readonly SteamReportFilterer _reportFilterer;
        

        public SteamReportManager(string connectionString, ISteamActivityManager activityManager)
        {
            _activityManager = activityManager;
            _odbc = new OdbcClient(connectionString);
            _reportFilterer = new SteamReportFilterer();
        }
        

        private async Task<SteamReport> _GetReport(long id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid id", "id");

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

        public async Task<ISteamReport> GetReport(long id)
        {
            var report = await _GetReport(id);
            return report;
        }


        private async Task<IEnumerable<Task<SteamReport>>> _GetReports(TimeRange request, long? userID)
        {
            var sql = "SELECT * FROM Steam_Reports " +
                      "WHERE ((StartTime BETWEEN ? AND ?) OR " +
                      "       (EndTime BETWEEN ? AND ?) OR " +
                      "       (? BETWEEN StartTime AND ?) OR " +
                      "       (? BETWEEN ? AND EndTime))" +
                      //(userID.HasValue ? " AND (UserID = ? OR UserID IS NULL)" : "");
                      (userID.HasValue ? " AND (UserID = ?)" : "");
            var cmd = _odbc.CreateCommand();
            cmd.CommandText = sql;
            cmd.AddParam("@StartTime", request.StartTime);
            cmd.AddParam("@EndTime", request.EndTime);

            cmd.AddParam("@StartTime", request.StartTime);
            cmd.AddParam("@EndTime", request.EndTime);

            cmd.AddParam("@StartTime", request.StartTime);
            cmd.AddParam("@EndTime", request.EndTime);

            cmd.AddParam("@EndTime", request.EndTime);
            cmd.AddParam("@StartTime", request.StartTime);

            if (userID.HasValue)
                cmd.AddParam("@UserID", userID.Value);

            var dataTable = await cmd.ExecuteReader();
            var rows = dataTable.Rows.ToList();
            var dynamic = rows.Select(x => x.ToExpando()).ToList();
            var reports = dynamic.Select(async x =>
            {
                var report = x.To<SteamReport>();
                if (report != null)
                    report = await LoadReportExtraData(report);
                return report;
            });
            return reports;
        }
        
        public async Task<IEnumerable<ISteamReport>> GetReports(TimeRange timeRange)
        {
            var reportTasks = await _GetReports(timeRange, null);
            var reports = await Task.WhenAll(reportTasks);
            return reports;
        }

        public async Task<IEnumerable<ISteamReport>> GetReports(TimeRange timeRange, long userID)
        {
            var reportTasks = await _GetReports(timeRange, userID);
            var reports = await Task.WhenAll(reportTasks);
            return reports;
        }


        private async Task<SteamReportFilterSet> LoadFilterSet(long filterSetID)
        {
            var sql = "SELECT * FROM Steam_ReportFilters " +
                      "WHERE SetID = ?";
            var cmd = _odbc.CreateCommand();
            cmd.CommandText = sql;
            cmd.AddParam("@FilterSetID", filterSetID);

            var dataTable = await cmd.ExecuteReader();
            var rows = dataTable.Rows.ToList();
            var dynamic = rows.Select(x => x.ToExpando()).ToList();
            var filters = dynamic.Select(x => x.To<SteamReportFilter>()).ToList();
            var filterSet = new SteamReportFilterSet
            {
                ID = filterSetID,
                Filters = filters,
            };
            return filterSet;
        }

        private async Task<IEnumerable<GamingSession>> LoadReportSessions(SteamReport report)
        {
            var sql = "SELECT * FROM Steam_ReportSessions " +
                      "WHERE ReportID = ?";
            var cmd = _odbc.CreateCommand();
            cmd.CommandText = sql;
            cmd.AddParam("@ReportID", report.ID);

            var dataTable = await cmd.ExecuteReader();
            var rows = dataTable.Rows.ToList();
            var dynamic = rows.Select(x => x.ToExpando()).ToList();

            var sessions = new List<GamingSession>();
            var sessionIDs = dynamic.Select(x => x.Get<long>("SessionID")).ToList();
            if (sessionIDs.Any())
            {
                var allSessions = await _activityManager.GetGamingSessions(new TimeRange
                {
                    StartTime = report.StartTime,
                    EndTime = report.EndTime,
                });
                sessions = allSessions.Where(x => sessionIDs.Contains(x.ID)).ToList();
            }
            return sessions;
        }
        
        private async Task<SteamReport> LoadReportExtraData(SteamReport report)
        {
            if (report.FilterSetID.HasValue)
                report.FilterSet = await LoadFilterSet(report.FilterSetID.Value);

            var sessions = await LoadReportSessions(report);
            report.Sessions = sessions.ToList();
            return report;
        }


        private async Task<SteamReport> _GenerateReport(ISteamReportGenerationRequest request)
        {
            if (request.UserID <= 0)
            {
                throw new ArgumentException("UserID is required", "request");
            }

            SteamReport report;
            if (request.ID > 0)
            {
                report = await _GetReport(request.ID);
                if (report == null)
                    throw new Exception("Report could not be found");
            }
            else
            {
                report = new SteamReport();
                report.UserID = request.UserID;
            }

            report.Name = request.Name;
            report.Description = request.Description;
            report.StartTime = request.StartTime;
            report.EndTime = request.EndTime;
            report.LastModified = DateTime.UtcNow;
            report.SubscriptionID = request.SubscriptionID;
            report.Enabled = request.Enabled;

            // todo: report.EventFormatting
            
            if (request.FilterSet != null)
            {
                await StoreFilterSet(request.FilterSet);
                report.FilterSet = request.FilterSet;
                report.FilterSetID = report.FilterSet.ID;
            }

            await StoreReport(report);


            await _RefreshReport(report);
            return report;
        }

        public async Task<ISteamReport> GenerateReport(ISteamReportGenerationRequest request)
        {
            var report = await _GenerateReport(request);
            return report;
        }


        private async Task _RefreshReport(SteamReport report)
        {
            var sessions = (await _activityManager.GetGamingSessions(new TimeRange
            {
                StartTime = report.StartTime,
                EndTime = report.EndTime,
            })).ToList();
            var result = _reportFilterer.GetFilterSessions(sessions, report.FilterSet);

            report.Sessions = result;

            await StoreReportSessions(report);
        }

        public async Task RefreshReport(long id)
        {
            var report = await _GetReport(id);
            if (report == null)
                throw new Exception("Report could not be found");
            await _RefreshReport(report);
        }

        public async Task RefreshReport(ISteamReport report)
        {
            if (report == null)
                throw new ArgumentNullException("report");

            if (report is SteamReport)
            {
                var steamReport = (SteamReport) report;
                await _RefreshReport(steamReport);
                return;
            }
            await RefreshReport(report.ID);
        }

        public async Task<SteamReportTemplate> _GetReportTemplate(long id)
        {
            var sql = "SELECT * FROM Steam_ReportTemplates " +
                      "WHERE ID = ?";
            var cmd = _odbc.CreateCommand();
            cmd.CommandText = sql;
            cmd.AddParam("@TemplateID", id);

            var dataTable = await cmd.ExecuteReader();
            var rows = dataTable.Rows.ToList();
            var dynamic = rows.Select(x => x.ToExpando()).ToList();
            var templates = dynamic.Select(x => x.To<SteamReportTemplate>());
            var template = templates.FirstOrDefault();
            if (template != null && template.FilterSetID.HasValue)
                template.FilterSet = await LoadFilterSet(template.FilterSetID.Value);
            return template;
        }

        public async Task<ISteamReportTemplate> GetReportTemplate(long id)
        {
            var template = await _GetReportTemplate(id);
            return template;
        }


        private async Task<IEnumerable<SteamReportSubscription>> _GetReportSubscriptions(long? userID)
        {
            var sql = "SELECT * FROM Steam_ReportSubscriptions " +
                      (userID.HasValue ? " WHERE (UserID = ?)" : "");
            var cmd = _odbc.CreateCommand();
            cmd.CommandText = sql;
            if (userID.HasValue)
                cmd.AddParam("@UserID", userID.Value);

            var dataTable = await cmd.ExecuteReader();
            var rows = dataTable.Rows.ToList();
            var dynamic = rows.Select(x => x.ToExpando()).ToList();
            var subscriptions = dynamic.Select(x => x.To<SteamReportSubscription>());
            return subscriptions;
        }

        public async Task<IEnumerable<ISteamReportSubscription>> GetReportSubscriptions()
        {
            var reports = await _GetReportSubscriptions(null);
            return reports;
        }

        public async Task<IEnumerable<ISteamReportSubscription>> GetReportSubscriptions(long userID)
        {
            var reports = await _GetReportSubscriptions(userID);
            return reports;
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
                sql = "INSERT INTO Steam_Reports(Name, Description, FilterSetID, UserID, SubscriptionID, StartTime, EndTime, LastModified, LastGenerated, Enabled) " +
                      "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?); SELECT ? = @@IDENTITY";
                var reportIDParam = new OdbcParameter2
                {
                    Name = "@ReportID",
                    IsOutput = true,
                };
                cmd.CommandText = sql;
                cmd.AddParam("@Name", report.Name);
                cmd.AddParam("@Description", report.Description);
                cmd.AddParam("@FilterSetID", report.FilterSetID);
                cmd.AddParam("@UserID", report.UserID);
                cmd.AddParam("@SubscriptionID", report.SubscriptionID > 0 ? report.SubscriptionID : (object)DBNull.Value);
                cmd.AddParam("@StartTime", report.StartTime);
                cmd.AddParam("@EndTime", report.EndTime);
                cmd.AddParam("@LastModified", report.LastModified);
                cmd.AddParam("@LastGenerated", report.LastGenerated);
                cmd.AddParam("@Enabled", report.Enabled);
                cmd.Parameters.Add(reportIDParam);
                changes += await cmd.ExecuteNonQuery();
                if (changes <= 0)
                    throw new InvalidOperationException("Error creating report");
                report.ID = reportIDParam.Value.SafeConvert<long>();
            }
            else
            {
                // Update report
                cmd = _odbc.CreateCommand();
                sql = "UPDATE Steam_Reports " +
                      "SET  Name = ?," +
                      "     Description = ?," +
                      (report.FilterSetID.HasValue ? "     FilterSetID = ?," : "") +
                      "     UserID = ?," +
                      "     SubscriptionID = ?," +
                      "     StartTime = ?," +
                      "     EndTime = ?," +
                      "     LastModified = ?," +
                      "     LastGenerated = ?," +
                      "     Enabled = ? " +
                      "WHERE ID = ?";
                cmd.CommandText = sql;
                cmd.AddParam("@Name", report.Name);
                cmd.AddParam("@Description", report.Description);
                if (report.FilterSetID.HasValue)
                    cmd.AddParam("@FilterSetID", report.FilterSetID);
                cmd.AddParam("@UserID", report.UserID);
                cmd.AddParam("@SubscriptionID", report.SubscriptionID > 0 ? report.SubscriptionID : (object)DBNull.Value);
                cmd.AddParam("@StartTime", report.StartTime);
                cmd.AddParam("@EndTime", report.EndTime);
                cmd.AddParam("@LastModified", report.LastModified);
                cmd.AddParam("@LastGenerated", report.LastGenerated);
                cmd.AddParam("@Enabled", report.Enabled);
                cmd.AddParam("@ReportID", report.ID);
                changes += await cmd.ExecuteNonQuery();
                if (changes <= 0)
                    throw new InvalidOperationException("Error updating report");
            }
        }

        private async Task StoreFilterSet(SteamReportFilterSet filterSet)
        {
            string sql;
            var changes = 0;

            if (filterSet != null)
            {
                OdbcCommand2 cmd;
                if (filterSet.ID > 0)
                {
                    // Remove previous filters
                    cmd = _odbc.CreateCommand();
                    sql = "DELETE FROM Steam_ReportFilters " +
                          "WHERE SetID = ?";
                    cmd.CommandText = sql;
                    cmd.AddParam("@SetID", filterSet.ID);
                    changes += await cmd.ExecuteNonQuery();
                    filterSet.Filters.ToList().ForEach(x => x.ID = 0);
                }
                else
                {
                    object tag = DBNull.Value;
                    cmd = _odbc.CreateCommand();
                    sql = "INSERT INTO Steam_ReportFilterSets(Tag) " +
                          "VALUES (?); SELECT ? = @@IDENTITY";
                    cmd.CommandText = sql;
                    var filterSetIDParam = new OdbcParameter2
                    {
                        Name = "@FilterSetID",
                        IsOutput = true,
                    };
                    cmd.AddParam("@Tag", tag);
                    cmd.Parameters.Add(filterSetIDParam);
                    changes += await cmd.ExecuteNonQuery();
                    filterSet.ID = filterSetIDParam.Value.SafeConvert<long>();
                    filterSet.Filters.ToList().ForEach(x => x.ID = 0);
                }


                if (filterSet.Filters != null && filterSet.Filters.Any())
                {
                    foreach (var filter in filterSet.Filters)
                    {
                        if (filter.ID <= 0)
                        {
                            // Create report
                            cmd = _odbc.CreateCommand();
                            sql = "INSERT INTO Steam_ReportFilters(SetID, GroupID, GroupRule, Attribute, Operator, Value) " +
                                  "VALUES (?, ?, ?, ?, ?, ?); SELECT ? = @@IDENTITY";
                            var filterIDParam = new OdbcParameter2
                            {
                                Name = "@FilterID",
                                IsOutput = true,
                            };
                            cmd.CommandText = sql;
                            cmd.AddParam("@SetID", filterSet.ID);
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
                                  "SET  SetID = ?," +
                                  "     GroupID = ?," +
                                  "     GroupRule = ?," +
                                  "     Attribute = ?," +
                                  "     Operator = ?," +
                                  "     Value = ? " +
                                  "WHERE ID = ?";
                            cmd.CommandText = sql;
                            cmd.AddParam("@SetID", filterSet.ID);
                            cmd.AddParam("@GroupID", filter.GroupID);
                            cmd.AddParam("@GroupRule", filter.GroupRule.ToString());
                            cmd.AddParam("@Attribute", filter.Attribute.ToString());
                            cmd.AddParam("@Operator", filter.Operator.ToString());
                            cmd.AddParam("@Value", filter.Value);
                            cmd.AddParam("@FilterID", filter.ID);
                            changes += await cmd.ExecuteNonQuery();
                        }
                    }

                }
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
