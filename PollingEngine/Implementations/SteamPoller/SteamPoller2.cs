using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using PollingEngine.Core;
using PortableSteam;
using PortableSteam.Interfaces.General.ISteamUser;
using PortableSteam.Interfaces.General.ISteamUserStats;

namespace SteamPoller
{
    public class SteamPoller2 : IPollingProgram
    {
        private readonly Dictionary<SteamIdentity, GamerInfo> _data = new Dictionary<SteamIdentity, GamerInfo>();
        private OdbcConnection _odbc;
        private ISteamPollerSettings _settings;


        public SteamPoller2()
        {
            // All settings to be loaded via System.Configuration (app.config)
            var settings = new SteamPollerSettings();
            _settings = settings;
            settings.SteamApiKey = "511DFA79B7394CEFA286165D20C46FC1";
            settings.PollingDataConnString = ConfigurationManager.ConnectionStrings["PollingDatabase"].ConnectionString; ;
            settings.Identities = new List<long>
            {
                { 76561197994923014 },  // LazyTarget
                //{ 76561198000854855 },  // Moerta
                //{ 76561198038673865 },  // Exo
                { 76561198006316454 },  // Richard
                //{ 76561198071019307 },  // Snakeman
            };
        }


        public IList<SteamIdentity> Identities { get; private set; }



        public async Task OnStarting(PollingContext context)
        {
            SteamWebAPI.SetGlobalKey(_settings.SteamApiKey);

            _odbc = new OdbcConnection(_settings.PollingDataConnString);
            if (_odbc.State != ConnectionState.Open)
                _odbc.Open();

            Identities = new List<SteamIdentity>();
            var identities = _settings.Identities.Select(SteamIdentity.FromSteamID);
            foreach (var steamIdentity in identities)
            {
                Identities.Add(steamIdentity);
            }
        }

        public async Task OnInterval(PollingContext context)
        {
            await PollGamingInfo_Portable(context, Identities);
        }

        public async Task OnStopping(PollingContext context)
        {
            
        }

        public void ApplyArguments(string[] args)
        {
            if (args.Length >= 2)
            {
                var verb = args[0].ToLower();
                var subject = args[1].ToLower();
                var value = args.Length >= 3 ? args[2] : null;

                if (verb == "subscribe")
                {
                    value = subject;
                    long steamID;
                    if (long.TryParse(value, out steamID))
                    {
                        if (Identities.Any(x => x.SteamID == steamID))
                            Console.WriteLine("SteamIdentity already subscribed: " + value);
                        else
                        {
                            Identities.Add(SteamIdentity.FromSteamID(steamID));
                            Console.WriteLine("Subscribed to SteamIdentity: " + steamID);
                        }
                    }
                    else
                        Console.WriteLine("Failed to parse SteamIdentity: {0}. Must be numeric", value);
                }
                else if (verb == "unsubscribe")
                {
                    value = subject;
                    long steamID;
                    if (long.TryParse(value, out steamID))
                    {
                        if (Identities.Any(x => x.SteamID == steamID))
                        {
                            var matches = Identities.Where(x => x.SteamID == steamID).ToList();
                            foreach (var steamIdentity in matches)
                                Identities.Remove(steamIdentity);
                            Console.WriteLine("SteamIdentity unsubscribed: " + value);
                        }
                        else
                        {
                            Identities.Add(SteamIdentity.FromSteamID(steamID));
                            Console.WriteLine("Not subscribed to SteamIdentity: " + steamID);
                        }
                    }
                    else
                        Console.WriteLine("Failed to parse SteamIdentity: {0}. Must be numeric", value);
                }
            }
        }


        private async Task PollGamingInfo_Portable(PollingContext context, IList<SteamIdentity> identities)
        {
            var time = DateTime.UtcNow;
            //var pollStartTime = context.TimeStarted.ToUniversalTime();
            //if (pollStartTime == DateTime.MinValue)
            //    pollStartTime = time;
            
            var players = await SteamWebAPI.General().ISteamUser().GetPlayerSummaries(identities.ToArray()).GetResponseAsync();
            if (players == null || players.Data == null || players.Data.Players == null)
            {
                return;
            }

            foreach (var player in players.Data.Players)
            {
                if (player == null || player.Identity == null)
                    continue;

                var currentSession = new GamingSession
                {
                    Player = player,
                    Time = time,
                };

                var gamerInfo = _data.Where(x => x.Key.SteamID == player.Identity.SteamID).Select(x => x.Value).FirstOrDefault();
                if (gamerInfo == null)
                {
                    gamerInfo = new GamerInfo();
                    gamerInfo.Player = player;
                    _data[player.Identity] = gamerInfo;
                }

                var diff = gamerInfo.ActiveSession == null || SessionHasDiff(gamerInfo.ActiveSession, currentSession);
                if (diff)
                {
                    Debug.WriteLine("Diff detected:");
                    Debug.WriteLine("PersonaState: {0} vs {1}",     gamerInfo.ActiveSession != null ? gamerInfo.ActiveSession.Player.PersonaState.ToString() : "NO LAST INFO NULL", currentSession.Player.PersonaState);
                    Debug.WriteLine("GameID: {0} vs {1}",           gamerInfo.ActiveSession != null ? gamerInfo.ActiveSession.Player.GameID ?? "NULL" : "NO LAST INTO", currentSession.Player.GameID ?? "NULL");
                    Debug.WriteLine("GameExtraInfo: {0} vs {1}",    gamerInfo.ActiveSession != null ? gamerInfo.ActiveSession.Player.GameExtraInfo ?? "NULL" : "NULL", currentSession.Player.GameExtraInfo ?? "NULL");

                    if (gamerInfo.ActiveSession != null)
                    {
                        Debug.WriteLine("Session ended: \t\t" + gamerInfo.ActiveSession);
                        await Steam_OnSessionEnded(new GamingSessionEventArgs { Session = gamerInfo.ActiveSession, Identity = player.Identity });
                    }
                    gamerInfo.ActiveSession = currentSession;


                    // todo: clear previous?, or use X intervals to ensure user stopped playing
                    gamerInfo.Sessions.Add(gamerInfo.ActiveSession);

                    Debug.WriteLine("Session stared: \t" + gamerInfo.ActiveSession);
                    await Steam_OnSessionStarted(new GamingSessionEventArgs { Session = gamerInfo.ActiveSession, Identity = player.Identity });
                }
                else
                {
                    var timeDiff = time.Subtract(gamerInfo.ActiveSession.Time);
                    gamerInfo.ActiveSession.Duration = timeDiff;


                    if (!string.IsNullOrWhiteSpace(gamerInfo.Player.GameID))
                    {
                        var updateInterval = 40;
                        //var modulo = timeDiff.TotalSeconds % (context.Interval.TotalSeconds);
                        var modulo = context.IntervalSequence % updateInterval;
                        if (modulo == 0)
                        {
                            // Each 40th interval, update gaming session (at 15 sec/interval = each 10 minutes)
                            Console.WriteLine("Updating game session: " + gamerInfo.ActiveSession);
                            await StoreUserGameSession(gamerInfo.ActiveSession, true);
                        }
                    }
                }
                
                Debug.WriteLine("Session: \t\t\t" + gamerInfo.ActiveSession);
                Console.WriteLine(gamerInfo.ActiveSession);
            }
            
        }

        private bool SessionHasDiff(GamingSession pre, GamingSession post)
        {
            return pre == null || pre.Player.GameID != post.Player.GameID || (string.IsNullOrEmpty(post.Player.GameExtraInfo) && pre.Player.PersonaState != post.Player.PersonaState);
        }


        private async Task Steam_OnSessionStarted(GamingSessionEventArgs e)
        {
            try
            {
                await StoreUserGameSession(e.Session, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to store game session, Error: " + ex.Message);
                throw;
            }

            if (!string.IsNullOrWhiteSpace(e.Session.Player.GameID))
            {
                var gameID = Convert.ToInt32(e.Session.Player.GameID);
                await LoadUserGameStats(gameID, e.Identity, e);
            }
        }


        private async Task Steam_OnSessionEnded(GamingSessionEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.Session.Player.GameID))
            {
                Debug.WriteLine("Player is not playing, ignoring cal event and odbc entry");
                return;
            }


            int gameID;
            int.TryParse(e.Session.Player.GameID, out gameID);
            if (!string.IsNullOrWhiteSpace(e.Session.Player.GameID))
            {
                await LoadUserGameStats(gameID, e.Identity, e);
            }
            

            try
            {
                await StoreUserGameSession(e.Session, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to store game session, Error: " + ex.Message);
                throw;
            }
        }

        

        private async Task<GetUserStatsForGameResponseData> LoadUserGameStats(int gameID, SteamIdentity steamIdentity, GamingSessionEventArgs args)
        {
            GetUserStatsForGameResponse gameStats;
            try
            {
                gameStats = await SteamWebAPI.General().ISteamUserStats().GetUserStatsForGame(gameID, steamIdentity).GetResponseAsync();
                if (gameStats == null || gameStats.Data == null)
                {
                    return null;
                }

                
                var stats = await FilterStatsWithDifferance(gameID, steamIdentity, gameStats.Data.Stats);
                var achievements = await FilterAchievementsWithDifferance(gameID, steamIdentity, gameStats.Data.Achievements);

                await StoreUserGameStats(gameID, steamIdentity, stats, args.Session.SessionID);
                await StoreUserGameAchievements(gameID, steamIdentity, achievements, args.Session.SessionID);

                return gameStats.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                //throw;
                return null;
            }
        }


        private async Task<IList<GetUserStatsForGameResponseStats>> FilterStatsWithDifferance(int gameID, SteamIdentity steamIdentity, IList<GetUserStatsForGameResponseStats> stats)
        {
            if (_odbc.State != ConnectionState.Open)
                _odbc.Open();

            var result = new List<GetUserStatsForGameResponseStats>();
            result.AddRange(stats);
            try
            {
                var sql = "SELECT * " +
                          "FROM Steam_GameStats " +
                          "WHERE UserID = ? AND GameID = ? " +
                          "ORDER BY UserID, GameID, Time DESC, Name, Value DESC";
                var cmd = _odbc.CreateCommand();
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@UserID", steamIdentity.SteamID);
                cmd.Parameters.AddWithValue("@GameID", gameID);

                var oldStats = new List<GetUserStatsForGameResponseStats>();
                var reader = await cmd.ExecuteReaderAsync();
                using (reader)
                {
                    foreach (DbDataRecord record in reader)
                    {
                        var name = record.GetString(reader.GetOrdinal("Name"));
                        var value = record.GetInt32(reader.GetOrdinal("Value"));

                        oldStats.Add(new GetUserStatsForGameResponseStats
                        {
                            Name = name,
                            Value = value,
                        });

                        var existing = stats.FirstOrDefault(x => x.Name == name);
                        if (existing != null && existing.Value == value)
                            result.Remove(existing);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                //throw;
            }
            return result;
        }


        private async Task<IList<GetUserStatsForGameResponseAchievements>> FilterAchievementsWithDifferance(int gameID, SteamIdentity steamIdentity, IList<GetUserStatsForGameResponseAchievements> achievements)
        {
            if (_odbc.State != ConnectionState.Open)
                _odbc.Open();

            var result = new List<GetUserStatsForGameResponseAchievements>();
            result.AddRange(achievements);
            try
            {
                var sql = "SELECT * " +
                          "FROM Steam_GameAchievements " +
                          "WHERE UserID = ? AND GameID = ? " +
                          "ORDER BY UserID, GameID, Time DESC, Name, Achieved DESC";
                var cmd = _odbc.CreateCommand();
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@UserID", steamIdentity.SteamID);
                cmd.Parameters.AddWithValue("@GameID", gameID);

                var oldAchievements = new List<GetUserStatsForGameResponseAchievements>();
                var reader = await cmd.ExecuteReaderAsync();
                using (reader)
                {
                    foreach (DbDataRecord record in reader)
                    {
                        var name = record.GetString(reader.GetOrdinal("Name"));
                        var achieved = record.GetBoolean(reader.GetOrdinal("Achieved"));

                        oldAchievements.Add(new GetUserStatsForGameResponseAchievements
                        {
                            Name = name,
                            Achieved = achieved,
                        });

                        var existing = achievements.FirstOrDefault(x => x.Name == name);
                        if (existing != null && existing.Achieved == achieved)
                            result.Remove(existing);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                //throw;
            }
            return result;
        }


        private async Task StoreUserGameStats(int gameID, SteamIdentity steamIdentity, IList<GetUserStatsForGameResponseStats> stats, long sessionID)
        {
            if (stats == null || !stats.Any())
                return;
            if (_odbc.State != ConnectionState.Open)
                _odbc.Open();
            var time = DateTime.UtcNow;
            foreach (var stat in stats)
            {
                try
                {
                    var sql = "INSERT INTO Steam_GameStats(UserID, GameID, SessionID, Time, Name, Value) " +
                              "VALUES (?, ?, ?, ?, ?, ?)";
                    var cmd = _odbc.CreateCommand();
                    cmd.CommandText = sql;
                    cmd.Parameters.AddWithValue("@UserID", steamIdentity.SteamID);
                    cmd.Parameters.AddWithValue("@GameID", gameID);
                    cmd.Parameters.AddWithValue("@SessionID", sessionID > 0 ? (object)sessionID : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Time", time.ToUniversalTime());
                    cmd.Parameters.AddWithValue("@Name", stat.Name);
                    cmd.Parameters.AddWithValue("@Value", stat.Value);

                    var changes = await cmd.ExecuteNonQueryAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    //throw;
                }
            }
        }


        private async Task StoreUserGameAchievements(int gameID, SteamIdentity steamIdentity, IList<GetUserStatsForGameResponseAchievements> achievements, long sessionID)
        {
            if (achievements == null || !achievements.Any())
                return;
            if (_odbc.State != ConnectionState.Open)
                _odbc.Open();
            var time = DateTime.UtcNow;
            foreach (var achive in achievements)
            {
                try
                {
                    var sql = "INSERT INTO Steam_GameAchievements(UserID, GameID, SessionID, Time, Name, Achieved) " +
                              "VALUES (?, ?, ?, ?, ?, ?)";
                    var cmd = _odbc.CreateCommand();
                    cmd.CommandText = sql;
                    cmd.Parameters.AddWithValue("@UserID", steamIdentity.SteamID);
                    cmd.Parameters.AddWithValue("@GameID", gameID);
                    cmd.Parameters.AddWithValue("@SessionID", sessionID > 0 ? (object) sessionID : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Time", time.ToUniversalTime());
                    cmd.Parameters.AddWithValue("@Name", achive.Name);
                    cmd.Parameters.AddWithValue("@Achieved", achive.Achieved);

                    var changes = await cmd.ExecuteNonQueryAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    //throw;
                }
            }
        }


        private async Task StoreUserGameSession(GamingSession session, bool active)
        {
            if (_odbc.State != ConnectionState.Open)
                _odbc.Open();

            int gameID;
            int.TryParse(session.Player.GameID, out gameID);
            if (gameID <= 0)
                return;

            var start = session.Time.ToUniversalTime();
            var end = session.Time.Add(session.Duration).ToUniversalTime();
            var steamIdentity = session.Player.Identity;
            var cmd = _odbc.CreateCommand();
            var changes = 0;
            try
            {
                if (session.SessionID <= 0)
                {
                    // Close active sessions
                    var sql = "UPDATE Steam_GamingSessions " +
                              "SET Active = ? " +       // todo: append a Interval to the EndTime?
                              "WHERE UserID = ? AND GameID = ?";
                    cmd.CommandText = sql;
                    cmd.Parameters.AddWithValue("@Active", false);
                    cmd.Parameters.AddWithValue("@UserID", steamIdentity.SteamID);
                    cmd.Parameters.AddWithValue("@GameID", gameID);
                    changes += cmd.ExecuteNonQuery();       // todo: inteligently continue sessions?


                    // Append current session
                    cmd = _odbc.CreateCommand();
                    sql = "INSERT INTO Steam_GamingSessions(UserID, GameID, GameName, StartTime, EndTime, Active) " +
                          "VALUES (?, ?, ?, ?, ?, ?); SELECT ? = @@IDENTITY";
                    var sessionIDParam = new OdbcParameter("@SessionID", OdbcType.BigInt, 4)
                    {
                        Direction = ParameterDirection.Output,
                    };
                    cmd.CommandText = sql;
                    cmd.Parameters.AddWithValue("@UserID", steamIdentity.SteamID);
                    cmd.Parameters.AddWithValue("@GameID", gameID);
                    cmd.Parameters.AddWithValue("@GameName", session.Player.GameExtraInfo);
                    cmd.Parameters.AddWithValue("@StartTime", start);
                    cmd.Parameters.AddWithValue("@EndTime", end);
                    cmd.Parameters.AddWithValue("@Active", active);
                    cmd.Parameters.Add(sessionIDParam);
                    changes += cmd.ExecuteNonQuery();
                    session.SessionID = (long) sessionIDParam.Value;
                }
                else
                {
                    var sql = "UPDATE Steam_GamingSessions " +
                              "SET StartTime = ?, " +
                              "    EndTime = ?, " +
                              "    Active = ? " +
                              "WHERE ID = ?";
                    cmd.CommandText = sql;
                    //cmd.Parameters.AddWithValue("@UserID", steamIdentity.SteamID);
                    //cmd.Parameters.AddWithValue("@GameID", gameID);
                    //cmd.Parameters.AddWithValue("@GameName", session.Player.GameExtraInfo);
                    cmd.Parameters.AddWithValue("@StartTime", start);
                    cmd.Parameters.AddWithValue("@EndTime", end);
                    cmd.Parameters.AddWithValue("@Active", active);
                    cmd.Parameters.AddWithValue("@SessionID", session.SessionID);
                    changes += cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                //throw;
            }
        }



        [Obsolete("Should be moved to the application implementing using PollingData")]
        private string WritePopularStats(List<StatDiff> diffs, int gameID)
        {
            var res = "";
            try
            {
                if (gameID == 730) // CS:GO
                {
                    var dict = new Dictionary<string, string>
                    {
                        {"total_kills", "You killed {diff} people"},
                        {"total_deaths", "You died {diff} times"},
                        {"kill_death_ratio", null},
                        {"accuracy", null},
                        {"total_dominations", "You dominated {diff} people"},
                        {"total_damage_done", "Total damage done: {diff}"},
                        {"total_wins", "You won {diff} rounds"},
                        {"total_rounds_played", "You played {diff} rounds"},
                        {"total_mvps", "You earned {diff} MVP stars"},
                        {"total_kills_knife", "Killed {diff} people with the Knife"},
                        {"total_kills_m4a1", "Killed {diff} people with M4A1"},
                        {"total_kills_m4a1-s", "Killed {diff} people with M4A1-S"},
                        {"total_kills_ak47", "Killed {diff} people with AK47"},
                        {"total_kills_awp", "Killed {diff} people with AWP"},
                        {"awp_accuracy", null},
                    };

                    foreach (var pair in dict)
                    {
                        var d = diffs.FirstOrDefault(x => x.Name == pair.Key);
                        if (d != null)
                            res += pair.Value.Replace("{diff}", d.Differance.ToString()) + Environment.NewLine;
                        else
                        {
                            if (pair.Key == "kill_death_ratio")
                            {
                                var kills = diffs.FirstOrDefault(x => x.Name == "total_kills");
                                var deaths = diffs.FirstOrDefault(x => x.Name == "total_deaths");
                                if (kills != null && deaths != null)
                                {
                                    var ratio = kills.Differance/deaths.Differance;
                                    res += string.Format("You had {0} as k/d ratio", ratio) + Environment.NewLine;
                                }
                            }
                            else if (pair.Key == "awp_accuracy")
                            {
                                var awpShots = diffs.FirstOrDefault(x => x.Name == "total_shots_awp");
                                var awpHits = diffs.FirstOrDefault(x => x.Name == "total_hits_awp");
                                if (awpShots != null && awpHits != null)
                                {
                                    var accuracy = awpHits.Differance/awpShots.Differance;
                                    res += string.Format("You had {0:p1} accuracy with the AWP", accuracy) +
                                           Environment.NewLine;
                                }
                            }
                            else if (pair.Key == "accuracy")
                            {
                                var hits = diffs.FirstOrDefault(x => x.Name == "total_shots_hit");
                                var shots = diffs.FirstOrDefault(x => x.Name == "total_shots_fired");
                                if (shots != null && hits != null)
                                {
                                    var accuracy = hits.Differance/shots.Differance;
                                    res += string.Format("You had {0:p1} accuracy", accuracy) + Environment.NewLine;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error writing popular stats:");
                Debug.WriteLine(ex);
            }
            return res;
        }


        public class StatDiff
        {
            public string Name
            {
                get { return Post.Name; }
            }

            public GetUserStatsForGameResponseStats Pre { get; set; }

            public GetUserStatsForGameResponseStats Post { get; set; }


            public double Differance
            {
                get
                {
                    var pre = Pre != null ? (double) Pre.Value : 0;
                    var diff = Post.Value - pre;
                    return diff;
                }
            }
            
            public double PercentualDiff
            {
                get
                {
                    if (Post.Value <= 0)
                        return 0;
                    var procent = Differance/Post.Value;
                    return procent;
                }
            }

            public override string ToString()
            {
                var res = string.Format("{0}: {1} => {2} = {3} ({4:p2})", Post.Name, Pre != null ? Pre.Value : 0, Post.Value, Differance, PercentualDiff);
                return res;
            }
        }


        public class GamerInfo
        {
            public GamerInfo()
            {
                Sessions = new List<GamingSession>();
            }

            public GetPlayerSummariesResponsePlayer Player { get; set; }
            public List<GamingSession> Sessions { get; set; }
            public GamingSession ActiveSession { get; set; }
        }


        public class GamingSession
        {
            public long SessionID { get; set; }
            public GetPlayerSummariesResponsePlayer Player { get; set; }
            public DateTime Time { get; set; }
            public TimeSpan Duration { get; set; }

            public override string ToString()
            {
                string res;
                if (!string.IsNullOrWhiteSpace(Player.GameExtraInfo))
                    res = string.Format("{0} is playing {1}", Player.PersonaName, Player.GameExtraInfo);
                else
                    res = string.Format("{0} is {1}", Player.PersonaName, Player.PersonaState);

                if (Duration != TimeSpan.Zero)
                {
                    if (Duration.TotalMinutes < 1)
                        res += string.Format(" for {0} seconds", Duration.Seconds);
                    else if (Duration.TotalHours < 1)
                        res += string.Format(" for {0}m {1}s", Duration.Minutes, Duration.Seconds);
                    else
                        res += string.Format(" for {0}h {1}m", Duration.Hours, Duration.Minutes);
                }
                return res;
            }
        }

        
        public class GamingSessionEventArgs : EventArgs
        {
            public SteamIdentity Identity { get; set; }

            public GamingSession Session { get; set; }
        }

    }

}
