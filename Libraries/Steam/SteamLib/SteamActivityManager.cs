using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OdbcWrapper;
using SharedLib;
using SteamLib.Models;

namespace SteamLib
{
    public class SteamActivityManager : ISteamActivityManager
    {
        private readonly OdbcClient _odbc;

        public SteamActivityManager(string connectionString)
        {
            _odbc = new OdbcClient(connectionString);
        }
        

        public async Task<IEnumerable<GamingSession>> GetGamingSessions(TimeRange request)
        {
            var sessions = await GetGamingSessions(request, null);
            return sessions;
        }

        public async Task<IEnumerable<GamingSession>> GetGamingSessions(TimeRange request, params long[] steamUserIDs)
        {
            var cmd = _odbc.CreateCommand();
            cmd.AddParam("@StartTime", request.StartTime);
            cmd.AddParam("@EndTime", request.EndTime);

            var sql = "SELECT * FROM Steam_GamingSessions " +
                      "WHERE (StartTime >= ? AND EndTime <= ?) ";
            if (steamUserIDs != null)
            {
                sql += " AND ( ";
                for (var i = 0; i < steamUserIDs.Length; i++)
                {
                    sql += " UserID = ?";
                    if (i < (steamUserIDs.Length - 1))
                        sql += " OR ";

                    cmd.AddParam("@UserID_" + i, steamUserIDs[i]);
                }
                sql += ")";
            }
            cmd.CommandText = sql;

            var dataTable = await cmd.ExecuteReader();
            var rows = dataTable.Rows.ToList();
            var dynamic = rows.Select(x => x.ToExpando()).ToList();
            var sessions = dynamic.Select(x => x.To<GamingSession>()).ToList();
            return sessions;
        }
        
    }
}
