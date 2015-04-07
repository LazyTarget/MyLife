using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using PollingEngine.Core;
using PortableSteam;
using PortableSteam.Interfaces.General.ISteamApps;
using PortableSteam.Interfaces.General.ISteamUser;
using PortableSteam.Interfaces.General.ISteamUserStats;

namespace SteamPoller
{
    public class SteamPoller : IPollingProgram
    {
        private static string _apiKey = "511DFA79B7394CEFA286165D20C46FC1";
        private SteamIdentity _steamPollIdentity = SteamIdentity.FromSteamID(76561197994923014);

        private readonly Dictionary<DateTime, GamingInfo> _data = new Dictionary<DateTime, GamingInfo>();
        private readonly List<GamingSession> _sessions = new List<GamingSession>();
        private GetUserStatsForGameResponseData _gamestatsBefore;
        private GetUserStatsForGameResponseData _gamestatsAfter;


        public async Task OnStarting(PollingContext context)
        {
            SteamWebAPI.SetGlobalKey(_apiKey);
            
        }

        public async Task OnInterval(PollingContext context)
        {
            await PollGamingInfo_Portable(context, _steamPollIdentity);
        }

        public async Task OnStopping(PollingContext context)
        {
            
        }

        
        private async Task PollGamingInfo_Portable(PollingContext context, SteamIdentity identity)
        {
            var time = DateTime.Now;
            var pollStartTime = context.TimeStarted;
            if (pollStartTime == DateTime.MinValue)
                pollStartTime = time;

            var lastInfo = _data.Select(x => x.Value).LastOrDefault();

            var players = await SteamWebAPI.General().ISteamUser().GetPlayerSummaries(identity).GetResponseAsync();
            var gamingInfo = new GamingInfo();
            gamingInfo.Time = time;
            _data.Add(time, gamingInfo);

            gamingInfo.Player = players.Data.Players.Single();

            if (!string.IsNullOrWhiteSpace(gamingInfo.Player.GameID))
            {
                //var games = await SteamWebAPI.General().ISteamApps().GetAppList().GetResponseAsync();
                //gamingInfo.App = games.Data.Apps.FirstOrDefault(x => x.AppID.ToString() == gamingInfo.Player.GameID);
                gamingInfo.App = new App
                {
                    AppID = Convert.ToInt32(gamingInfo.Player.GameID),
                    Name = gamingInfo.Player.GameExtraInfo,
                };
            }

            Func<GamingInfo, GamingInfo, bool> isDiffFunc =
                (pre, post) => pre == null || pre.Player.GameID != post.Player.GameID ||
                               (string.IsNullOrEmpty(post.Player.GameExtraInfo) && pre.Player.PersonaState != post.Player.PersonaState);

            TimeSpan duration = TimeSpan.Zero;
            var lastDiffStateGamingInfo =
                _data.Where(x => isDiffFunc(x.Value, gamingInfo))
                    .Select(x => x.Value)
                    .LastOrDefault();
            if (lastDiffStateGamingInfo != null)
                duration = gamingInfo.Time.Subtract(lastDiffStateGamingInfo.Time);
            else
                duration = gamingInfo.Time.Subtract(pollStartTime);

            var isDiff = isDiffFunc(lastInfo, gamingInfo);
            //Duration = lastInfo != null ? gamingInfo.Time.Subtract(lastInfo.Time) : TimeSpan.Zero;
            GamingSession session;
            if (isDiff)
            {
                Debug.WriteLine("Diff detected:");
                Debug.WriteLine("PersonaState: {0} vs {1}", lastInfo != null ? lastInfo.Player.PersonaState.ToString() : "NO LAST INFO NULL", gamingInfo.Player.PersonaState);
                Debug.WriteLine("GameID: {0} vs {1}", lastInfo != null ? lastInfo.Player.GameID ?? "NULL" : "NO LAST INTO", gamingInfo.Player.GameID ?? "NULL");
                Debug.WriteLine("GameExtraInfo: {0} vs {1}", lastInfo != null ? lastInfo.Player.GameExtraInfo ?? "NULL" : "NULL", gamingInfo.Player.GameExtraInfo ?? "NULL");
                if (_sessions.Any())
                {
                    session = _sessions.Last();
                    Debug.WriteLine("Session ended: \t\t" + session);
                    //Debug.WriteLine("Session ended: \t\t json::" + Environment.NewLine + JsonConvert.SerializeObject(session, Formatting.Indented));
                    await Steam_OnSessionEnded(new GamingSessionEventArgs { Session = session });
                }

                session = new GamingSession();
                session.Time = time;
                session.App = gamingInfo.App;
                session.Duration = TimeSpan.Zero;
                session.Player = gamingInfo.Player;
                _sessions.Add(session);

                Debug.WriteLine("Session stared: \t" + session);
                //Debug.WriteLine("Session stared: \t json::" + Environment.NewLine + JsonConvert.SerializeObject(session, Formatting.Indented));
                await Steam_OnSessionStarted(new GamingSessionEventArgs { Session = session });
            }
            else
            {
                session = _sessions.LastOrDefault();
                if (session != null)
                    session.Duration = duration;
            }

            Debug.WriteLine("Gaminginfo: \t\t" + gamingInfo);
            Debug.WriteLine("Session: \t\t\t" + session);
            Console.WriteLine(session);
        }




        private async Task Steam_OnSessionStarted(GamingSessionEventArgs e)
        {
            _gamestatsBefore = null;

            if (!string.IsNullOrEmpty(e.Session.Player.GameID))
            {
                try
                {
                    var gameID = Convert.ToInt32(e.Session.Player.GameID);
                    var stats = await SteamWebAPI.General().ISteamUserStats().GetUserStatsForGame(gameID, _steamPollIdentity).GetResponseAsync();
                    _gamestatsBefore = stats.Data;
                    Debug.WriteLine("game stats (pre), achieve count: " + (_gamestatsBefore.Achievements != null ? _gamestatsBefore.Achievements.Count : 0));
                    Debug.WriteLine("game stats (pre), stat count: " + (_gamestatsBefore.Stats != null ? _gamestatsBefore.Stats.Count : 0));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error getting game stats (pre):");
                    Debug.WriteLine(ex);
                    _gamestatsBefore = null;
                }
            }
        }


        private async Task Steam_OnSessionEnded(GamingSessionEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.Session.Player.GameID))
            {
                Debug.WriteLine("Player is not playing, ignoring cal event and odbc entry");
                return;
            }


            var gameName = e.Session.App != null ? e.Session.App.Name : e.Session.Player.GameExtraInfo;
            //if(gameName=="Counter-Strike: Global Offensive")
            //    gameName = 
            

            //var evt = new CalendarCore.Event
            //{
            //    Name = "Gaming",
            //    Location = gameName,
            //    Start = e.Session.Time,
            //    End = e.Session.Time.Add(e.Session.Duration),
            //    Description = "This event was automatically added by Peter's Steam project",
            //};

            //try
            //{
            //    Debug.WriteLine("Begin creating cal event");
            //    var accessToken = await OutlookClient.GetAccessTokenFromRefreshToken(_outlookRefreshToken);
            //    var calendarServer = new OutlookClient(accessToken);

            //    evt = await calendarServer.CreateEvent(evt);
            //    Console.WriteLine("Created calendar event: " + evt.Name);
            //}
            //catch (WebException ex)
            //{
            //    Console.WriteLine("Failed to create calendar event, Error: " + ex.Message);
            //    Extensions.DumpWebResponse((HttpWebResponse)ex.Response);
            //    //throw;
            //}

            try
            {
                Debug.WriteLine("Begin creating odbc entry");

                var start = e.Session.Time;
                var end = e.Session.Time.Add(e.Session.Duration);

                var text = string.Format("{0} was playing on Steam", e.Session.Player.PersonaName);
                var desc = string.Format("{0} was playing {1}", e.Session.Player.PersonaName, e.Session.Player.GameExtraInfo);
                if (e.Session.Duration != TimeSpan.Zero)
                {
                    var duration = e.Session.Duration;
                    if (duration.TotalMinutes < 1)
                        desc += string.Format(" for {0} seconds", duration.Seconds);
                    else if (duration.TotalHours < 1)
                        desc += string.Format(" for {0}m {1}s", duration.Minutes, duration.Seconds);
                    else
                        desc += string.Format(" for {0}h {1}m", duration.Hours, duration.Minutes);
                }


                try
                {
                    if (_gamestatsBefore == null || _gamestatsBefore.Stats == null || !_gamestatsBefore.Stats.Any())
                    {
                        Console.WriteLine("Missing game stats before game session, ignoring stats after");
                    }
                    else
                    {
                        var gameID = Convert.ToInt32(e.Session.Player.GameID);
                        var stats = await SteamWebAPI.General().ISteamUserStats().GetUserStatsForGame(gameID, _steamPollIdentity).GetResponseAsync();
                        if (stats != null && stats.Data != null)
                        {
                            _gamestatsAfter = stats.Data;
                            Debug.WriteLine("game stats (post), achieve count: " + (_gamestatsAfter.Achievements != null ? _gamestatsAfter.Achievements.Count : 0));
                            Debug.WriteLine("game stats (post), stat count: " + (_gamestatsAfter.Stats != null ? _gamestatsAfter.Stats.Count : 0));


                            try
                            {
                                var diffs = _gamestatsAfter.Achievements != null
                                    ? _gamestatsAfter.Achievements
                                        .Where(x => x.Achieved)
                                        .Where(x =>
                                        {
                                            var pre = _gamestatsBefore.Achievements.FirstOrDefault(y => x.Name == y.Name);
                                            return pre == null || !pre.Achieved;
                                        })
                                        .OrderByDescending(x => x.Achieved)
                                        .ThenBy(x => x.Name)
                                        .ToList()
                                    : new List<GetUserStatsForGameResponseAchievements>();

                                if (diffs.Any())
                                {
                                    desc += Environment.NewLine;
                                    desc += Environment.NewLine;

                                    desc += "Unlocked achivements: " + Environment.NewLine;
                                    foreach (var achive in diffs)
                                    {
                                        desc += string.Format("{0}{1}", achive.Name, Environment.NewLine);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("Error getting game achieves (post):");
                                Debug.WriteLine(ex);
                            }


                            try
                            {
                                var diffs = _gamestatsAfter.Stats != null
                                    ? _gamestatsAfter.Stats
                                        .Where(x => _gamestatsBefore.Stats.Any(y => y.Name == x.Name))
                                        .Where(x => x.Value != _gamestatsBefore.Stats.First(y => y.Name == x.Name).Value)
                                        .Select(x => new StatDiff
                                        {
                                            Pre = _gamestatsBefore.Stats.First(y => y.Name == x.Name),
                                            Post = x
                                        })
                                        .OrderByDescending(x => x.PercentualDiff)
                                        .ToList()
                                    : new List<StatDiff>();

                                if (diffs.Any())
                                {
                                    desc += Environment.NewLine;
                                    desc += Environment.NewLine;

                                    var pop = WritePopularStats(diffs, gameID);
                                    if (!string.IsNullOrEmpty(pop))
                                    {
                                        desc += string.Format("Popular stats: {0}{1}{0}", Environment.NewLine, pop);
                                    }

                                    desc += "Changed stats: " + Environment.NewLine;
                                    foreach (var statDiff in diffs)
                                    {
                                        desc += string.Format("{0}{1}", statDiff, Environment.NewLine);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("Error getting game stats (post):");
                                Debug.WriteLine(ex);
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error getting post game stats:");
                    Debug.WriteLine(ex);
                }


                var connectionString = "Driver={SQL Server};Server=.;UID=Developer;PWD=123456789;Database=OdbcTest";
                var cn = new OdbcConnection(connectionString);
                if (cn.State != ConnectionState.Open)
                    cn.Open();

                var sql = "INSERT INTO SteamEvents(Text, Description, StartTime, EndTime, Source) " +
                    //"VALUES (@Text, @Description, @StartTime, @EndTime, @Source)";
                            "VALUES (?, ?, ?, ?, ?)";
                var cmd = new OdbcCommand(sql, cn);
                cmd.Parameters.AddWithValue("@Text", text);
                cmd.Parameters.AddWithValue("@Description", desc);
                cmd.Parameters.AddWithValue("@StartTime", start);
                cmd.Parameters.AddWithValue("@EndTime", end);
                cmd.Parameters.AddWithValue("@Source", "Steam poller");

                var changes = cmd.ExecuteNonQuery();
                if (changes > 0)
                    Console.WriteLine("Created odbc event: " + desc);
                else
                    Console.WriteLine("No changes commited when creating odbc entry: " + desc);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to create odbc entry, Error: " + ex.Message);
                throw;
            }
        }


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
                    var diff = Post.Value - (double)Pre.Value;
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
                var res = string.Format("{0}: {1} => {2} = {3} ({4:p2})", Post.Name, Pre.Value, Post.Value, Differance, PercentualDiff);
                return res;
            }
        }


        public class GamingInfo
        {
            public GetPlayerSummariesResponsePlayer Player { get; set; }
            public App App { get; set; }
            public DateTime Time { get; set; }


            public override string ToString()
            {
                string res;
                if (!string.IsNullOrWhiteSpace(Player.GameExtraInfo))
                    res = string.Format("{0} is playing {1}", Player.PersonaName, Player.GameExtraInfo);
                else
                    res = string.Format("{0} is {1}", Player.PersonaName, Player.PersonaState);

                var gamingSession = this as GamingSession;
                if (gamingSession != null && gamingSession.Duration != TimeSpan.Zero)
                {
                    var duration = gamingSession.Duration;
                    if (duration.TotalMinutes < 1)
                        res += string.Format(" for {0} seconds", duration.Seconds);
                    else if (duration.TotalHours < 1)
                        res += string.Format(" for {0}m {1}s", duration.Minutes, duration.Seconds);
                    else
                        res += string.Format(" for {0}h {1}m", duration.Hours, duration.Minutes);
                }
                return res;
            }
        }

        public class GamingSession : GamingInfo
        {
            public TimeSpan Duration { get; set; }
        }


        public class GamingSessionEventArgs : EventArgs
        {
            public GamingSession Session { get; set; }
        }

    }
    
    internal class Extensions
    {
        public static void DumpWebResponse(HttpWebResponse response)
        {
            var stream = response.GetResponseStream();
            string res;
            using (var sr = new StreamReader(stream, Encoding.UTF8))
            {
                res = sr.ReadToEnd();
            }
            Debug.WriteLine("---Error in WebRequest---");
            //Debug.WriteLine("Error: " + ex.Message);
            Debug.WriteLine("Request: {0}: {1}", response.Method, response.ResponseUri);
            Debug.WriteLine("Error Result: " + res);

            foreach (var header in response.Headers.AllKeys)
            {
                Debug.WriteLine("Header: {0} = {1}", header, response.Headers[header]);
            }
            Debug.WriteLine("");
        }
    }

}
