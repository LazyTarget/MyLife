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

            TimeSpan duration = TimeSpan.Zero;
            var lastDiffStateGamingInfo =
                _data.Where(x => x.Value.Player.PersonaState != gamingInfo.Player.PersonaState || x.Value.Player.GameID != gamingInfo.Player.GameID)
                    .Select(x => x.Value)
                    .LastOrDefault();
            if (lastDiffStateGamingInfo != null)
                duration = gamingInfo.Time.Subtract(lastDiffStateGamingInfo.Time);
            else
                duration = gamingInfo.Time.Subtract(pollStartTime);

            var isDiff = lastInfo == null || lastInfo.Player.GameID != gamingInfo.Player.GameID ||
                         (string.IsNullOrEmpty(gamingInfo.Player.GameExtraInfo) &&
                          lastInfo.Player.PersonaState != gamingInfo.Player.PersonaState);
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
                    var stats = await SteamWebAPI.General().ISteamUserStats().GetUserStatsForGame(Convert.ToInt32(e.Session.Player.GameID), _steamPollIdentity).GetResponseAsync();
                    _gamestatsBefore = stats.Data;
                }
                catch (Exception ex)
                {

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
                    var stats = await SteamWebAPI.General().ISteamUserStats().GetUserStatsForGame(Convert.ToInt32(e.Session.Player.GameID), _steamPollIdentity).GetResponseAsync();
                    _gamestatsAfter = stats.Data;

                    var diffs =
                        _gamestatsAfter.Stats
                            .Where(x => x.Value != _gamestatsBefore.Stats.First(y => y.Name == x.Name).Value)
                            .Select(x => new StatDiff {Pre = _gamestatsBefore.Stats.First(y => y.Name == x.Name), Post = x})
                            .OrderByDescending(x => x.PercentualDiff)
                            .ToList();

                    desc += Environment.NewLine;
                    if (diffs.Any())
                    {
                        desc += "Changed stats: " + Environment.NewLine;
                        foreach (var statDiff in diffs)
                        {
                            desc += string.Format("{0}{1}", statDiff, Environment.NewLine);
                        }
                    }
                }
                catch (Exception ex)
                {

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



        public class StatDiff
        {
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
                var res = string.Format("{0}: {1}=>{2} ({3:p0})", Post.Name, Pre.Value, Post.Value, PercentualDiff);
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
