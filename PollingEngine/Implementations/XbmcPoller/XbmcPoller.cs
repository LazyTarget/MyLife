using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PollingEngine.Core;

namespace XbmcPoller
{
    public class XbmcPoller : IPollingProgram
    {
        private TimeSpan _timeSinceDiff;
        private HttpClient _client;
        //private readonly Dictionary<DateTime, VideoItemInfo> _data = new Dictionary<DateTime, VideoItemInfo>();
        private WatchSessionInfo _sessionInfo;
        private IXbmcPollerSettings _settings;

        public XbmcPoller()
        {
            _settings = XbmcPollerSettingsConfigElement.LoadFromConfig();
        }

        public IXbmcPollerSettings Settings
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
            var handler = new HttpClientHandler()
            {
                Credentials = new NetworkCredential(_settings.ApiUsername, _settings.ApiPassword),
            };
            _client = new HttpClient(handler);
            _client.BaseAddress = new Uri(_settings.ApiBaseUrl);
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }


        /*
        public async Task OnInterval2(PollingContext context)
        {
            var time = DateTime.Now;
            _sessionInfo = new WatchSessionInfo
            {
                StartTime = time,
                EndTime = time,
                Active = true,
                SessionID = _lastSessionInfo.SessionID,
                LastPollTime = time,
                VideoItem = null,
            };
            

            
            bool isDiff;
            bool ended = false;
            VideoItemInfo videoItemInfo = null;
            try
            {
                videoItemInfo = await GetItemInfo();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting item info: {0}", ex.Message);
            }

            //await GetPlaybackInfo();


            //var index = _data.Select(x => x.Value).ToList().FindLastIndex(x => CompareTo(x, itemInfo) != 0);
            //var newIndex = Math.Min(index + 1, _data.Count - 1);
            //var startInfoPair = index >= 0 ? _data.ElementAtOrDefault(newIndex) : new KeyValuePair<DateTime, ItemInfo>();
            //var start = startInfoPair.Key;
            //if (start == DateTime.MinValue)
            //    start = pollStartTime;

            //var duration = time.Subtract(start);

            DiffType diffType;
            if (videoItemInfo != null)
            {
                if (_lastSessionInfo == null || _lastSessionInfo.VideoItem == null)
                {
                    // Started watching
                    isDiff = true;
                    //Console.WriteLine("Started watching '{0}'{1}", itemInfo.Title, TimeSpanToString(_timeSinceDiff));
                    diffType = DiffType.StartedWatching;
                }
                else
                {
                    // Compare watch info
                    isDiff = VideoItemInfo.CompareTo(videoItemInfo, _lastSessionInfo.VideoItem) != 0;
                    if (!isDiff)
                    {
                        // Still watching
                        //Console.WriteLine("Still watching '{0}'{1}", itemInfo.Title, TimeSpanToString(_timeSinceDiff));
                        diffType = DiffType.StillWatching;
                    }
                    else
                    {
                        // Changed program
                        ended = true;
                        //Console.WriteLine("Changed program from '{0}' to '{1}'{2}", lastInfo.Title, itemInfo.Title, TimeSpanToString(_timeSinceDiff));
                        diffType = DiffType.ChangedProgram;
                    }
                }
            }
            else
            {
                if (_lastSessionInfo == null || _lastSessionInfo.VideoItem == null)
                {
                    // no diff
                    isDiff = false;
                    //Console.WriteLine("Not watching anything {0}", TimeSpanToString(_timeSinceDiff));
                    diffType = DiffType.None;
                }
                else
                {
                    // Stopped watching
                    isDiff = true;
                    ended = true;
                    //Console.WriteLine("Stopped watching '{0}'{1}", lastInfo.Title, TimeSpanToString(_timeSinceDiff));
                    diffType = DiffType.StoppedWatching;
                }
            }
            //_data.Add(time, videoItemInfo);
            
            if (diffType == DiffType.None && isDiff)
            {
                _timeSinceDiff = TimeSpan.Zero;
            }


            var timeDiff = time.Subtract(_sessionInfo.LastPollTime);
            _timeSinceDiff = _timeSinceDiff.Add(timeDiff);
            _sessionInfo.LastPollTime = time;

            if (diffType == DiffType.StartedWatching && isDiff)
            {
                _timeSinceDiff = TimeSpan.Zero;
            }


            switch (diffType)
            {
                case DiffType.None:
                    Console.WriteLine("Not watching anything {0}", TimeSpanToString(_timeSinceDiff));
                    break;
                case DiffType.StartedWatching:
                    Console.WriteLine("Started watching '{0}'{1}", videoItemInfo.Title, TimeSpanToString(_timeSinceDiff));
                    break;
                case DiffType.StoppedWatching:
                    Console.WriteLine("Stopped watching '{0}'{1}", _lastSessionInfo.VideoItem.Title, TimeSpanToString(_timeSinceDiff));
                    break;
                case DiffType.StillWatching:
                    Console.WriteLine("Still watching '{0}'{1}", videoItemInfo.Title, TimeSpanToString(_timeSinceDiff));
                    break;
                case DiffType.ChangedProgram:
                    Console.WriteLine("Changed program from '{0}' to '{1}'{2}", _lastSessionInfo.VideoItem.Title, videoItemInfo.Title, TimeSpanToString(_timeSinceDiff));
                    break;
            }


            if (ended)
            {
                var start = time.Subtract(_timeSinceDiff);

                // Add to odbc database
                var sessionInfo = new WatchSessionInfo
                {
                    VideoItem = lastInfo,
                    StartTime = start,
                    EndTime = time,
                    Active = diffType == DiffType.StillWatching || 
                             diffType == DiffType.StartedWatching || 
                             diffType == DiffType.ChangedProgram,
                };
                StoreSessionInfo(sessionInfo);

                _timeSinceDiff = TimeSpan.Zero;
            }

            _lastSessionInfo = _sessionInfo;
        }
        */


        public async Task OnInterval(PollingContext context)
        {
            var time = DateTime.UtcNow;


            bool pingOk;
            try
            {
                pingOk = await PingXbmc();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("PingXbmc() failed: " + ex);
                pingOk = false;
            }


            if (pingOk)
            {
                // Xbmc is running
                if (_sessionInfo == null)
                {
                    var initializeNew = true;
                    if (_settings.MergeSessionPeriod > TimeSpan.Zero)
                    {
                        var lastSession = await GetLastSession();
                        if (lastSession != null)
                        {
                            var diff = time.Subtract(lastSession.EndTime);
                            if (diff <= _settings.MergeSessionPeriod)
                            {
                                // Continue on recent session
                                initializeNew = false;
                                _sessionInfo = lastSession;
                                Console.WriteLine("Continuing Kodi session");
                            }
                        }
                    }

                    if (initializeNew)
                    {
                        // Initialize new session
                        _sessionInfo = new WatchSessionInfo
                        {
                            StartTime = time,
                            EndTime = time,
                            LastPollTime = time,
                        };
                        Console.WriteLine("Initialized session with Kodi");
                    }
                }

                //_sessionInfo.LastPollTime = time;
                _sessionInfo.EndTime = time;
                _sessionInfo.Active = pingOk;
                await StoreSessionInfo(_sessionInfo);      // todo: optimize, save only each X intervals...
            }
            else
            {
                // Xbmc has been closed
                if (_sessionInfo != null)
                {
                    await CloseSession();

                    Console.WriteLine("Kodi session closed");
                }
                return;
            }
            
            
            bool isDiff;
            bool ended = false;
            VideoItemInfo videoItemInfo = null;
            try
            {
                videoItemInfo = await GetItemInfo();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting item info: {0}", ex.Message);
            }

            //await GetPlaybackInfo();

            
            var lastVideo = _sessionInfo.ActiveVideo != null ? _sessionInfo.ActiveVideo.Video : null;

            DiffType diffType;
            if (videoItemInfo != null)
            {
                if (lastVideo == null)
                {
                    // Started watching
                    isDiff = true;
                    //Console.WriteLine("Started watching '{0}'{1}", itemInfo.Title, TimeSpanToString(_timeSinceDiff));
                    diffType = DiffType.StartedWatching;


                    // Update database that the session for a video has been stopped
                    foreach (var sessionVideo in _sessionInfo.Videos)
                    {
                        if (!sessionVideo.Active)
                            continue;
                        sessionVideo.Active = false;
                        //sessionVideo.EndTime = time;        // not to be updated in StartedWatching (the only differance from ChangedProgram)
                        await StoreSessionVideo(sessionVideo);
                    }

                    // Set the active video
                    _sessionInfo.ActiveVideo = new CrSessionVideo
                    {
                        StartTime = time,
                        EndTime = time,
                        SessionID = _sessionInfo.SessionID,
                        Active = true,
                        Video = videoItemInfo,
                    };
                    await StoreSessionVideo(_sessionInfo.ActiveVideo);
                    _sessionInfo.Videos.Add(_sessionInfo.ActiveVideo);
                }
                else
                {
                    // Compare watch info
                    isDiff = VideoItemInfo.CompareTo(videoItemInfo, lastVideo) != 0;
                    if (!isDiff)
                    {
                        // Still watching
                        //Console.WriteLine("Still watching '{0}'{1}", itemInfo.Title, TimeSpanToString(_timeSinceDiff));
                        diffType = DiffType.StillWatching;

                        _sessionInfo.ActiveVideo.Active = true;
                        _sessionInfo.ActiveVideo.EndTime = time;
                        await StoreSessionVideo(_sessionInfo.ActiveVideo);        // todo: optimize number of requests
                    }
                    else
                    {
                        // Changed program
                        ended = true;
                        //Console.WriteLine("Changed program from '{0}' to '{1}'{2}", lastInfo.Title, itemInfo.Title, TimeSpanToString(_timeSinceDiff));
                        diffType = DiffType.ChangedProgram;


                        
                        // Update database that the session for a video has been stopped
                        foreach (var sessionVideo in _sessionInfo.Videos)
                        {
                            if (!sessionVideo.Active)
                                continue;
                            sessionVideo.Active = false;
                            sessionVideo.EndTime = time;
                            await StoreSessionVideo(sessionVideo);            // todo: optimize number of requests?
                        }

                        // Set the active video
                        _sessionInfo.ActiveVideo = new CrSessionVideo
                        {
                            StartTime = time,
                            EndTime = time,
                            SessionID = _sessionInfo.SessionID,
                            Active = true,
                            Video = videoItemInfo,
                        };
                        await StoreSessionVideo(_sessionInfo.ActiveVideo);
                        _sessionInfo.Videos.Add(_sessionInfo.ActiveVideo);
                    }
                }
            }
            else
            {
                if (lastVideo == null)
                {
                    // no diff
                    isDiff = false;
                    //Console.WriteLine("Not watching anything {0}", TimeSpanToString(_timeSinceDiff));
                    diffType = DiffType.None;
                }
                else
                {
                    // Stopped watching
                    isDiff = true;
                    ended = true;
                    //Console.WriteLine("Stopped watching '{0}'{1}", lastInfo.Title, TimeSpanToString(_timeSinceDiff));
                    diffType = DiffType.StoppedWatching;

                    
                    // Update database that the session for a video has been stopped
                    foreach (var sessionVideo in _sessionInfo.Videos)
                    {
                        if (!sessionVideo.Active)
                            continue;
                        if (sessionVideo == _sessionInfo.ActiveVideo)
                            continue;
                        sessionVideo.Active = false;
                        sessionVideo.EndTime = time;
                        await StoreSessionVideo(sessionVideo);            // todo: optimize number of requests?
                    }

                    // Set the active video
                    _sessionInfo.ActiveVideo.Active = false;
                    _sessionInfo.ActiveVideo.EndTime = time;
                    await StoreSessionVideo(_sessionInfo.ActiveVideo);
                    _sessionInfo.ActiveVideo = null;
                }
            }
            //_data.Add(time, videoItemInfo);
            
            if (diffType == DiffType.None && isDiff)
            {
                _timeSinceDiff = TimeSpan.Zero;
            }


            var timeDiff = time.Subtract(_sessionInfo.LastPollTime);
            _timeSinceDiff = _timeSinceDiff.Add(timeDiff);
            _sessionInfo.LastPollTime = time;

            if (diffType == DiffType.StartedWatching && isDiff)
            {
                _timeSinceDiff = TimeSpan.Zero;
            }


            try
            {
                switch (diffType)
                {
                    case DiffType.None:
                        Console.WriteLine("Not watching anything {0}", TimeSpanToString(_timeSinceDiff));
                        break;
                    case DiffType.StartedWatching:
                        Console.WriteLine("Started watching '{0}'{1}", videoItemInfo.Title, TimeSpanToString(_timeSinceDiff));
                        break;
                    case DiffType.StoppedWatching:
                        Console.WriteLine("Stopped watching '{0}'{1}", lastVideo.Title, TimeSpanToString(_timeSinceDiff));
                        break;
                    case DiffType.StillWatching:
                        Console.WriteLine("Still watching '{0}'{1}", videoItemInfo.Title, TimeSpanToString(_timeSinceDiff));
                        break;
                    case DiffType.ChangedProgram:
                        Console.WriteLine("Changed program from '{0}' to '{1}'{2}", lastVideo.Title, videoItemInfo.Title, TimeSpanToString(_timeSinceDiff));
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error logging the poll status (XbmcPoller)");
                Console.WriteLine(ex);
            }
        }


        private async Task CloseSession()
        {
            var time = DateTime.UtcNow;
            if (_sessionInfo == null)
                return;

            // Close active videos
            for (var i = 0; i < _sessionInfo.Videos.Count; i++)
            {
                var sessionVideo = _sessionInfo.Videos[i];
                if (sessionVideo == _sessionInfo.ActiveVideo)
                    continue;
                if (sessionVideo.Active)
                    sessionVideo.EndTime = time;        // correct?
                sessionVideo.Active = false;

                var duration = sessionVideo.EndTime.Subtract(sessionVideo.StartTime);
                if (_settings.MinVideoLength > TimeSpan.Zero &&
                    duration < _settings.MinVideoLength)
                {
                    await DeleteSessionVideo(sessionVideo);
                    if (_sessionInfo.Videos.Remove(sessionVideo))
                        i--;
                }
                else
                    await StoreSessionVideo(sessionVideo);        // todo: optimize?
            }

            // Close session
            if (_sessionInfo.Active)
                _sessionInfo.EndTime = time;
            _sessionInfo.Active = false;
            if (!_sessionInfo.Videos.Any())
                await DeleteSessionInfo(_sessionInfo);
            else
                await StoreSessionInfo(_sessionInfo);
            _sessionInfo = null;
        }



        public async Task OnStopping(PollingContext context)
        {
            await CloseSession();

            // move to timed purge?
            await CloseActiveVideos();
            await CloseActiveSessions();
        }



        public void ApplyArguments(string[] args)
        {
            
        }


        private async Task<bool> PingXbmc()
        {
            try
            {
                var uri = "";
                var obj = new Dictionary<string, object>
                {
                    { "jsonrpc", "2.0" },
                    { "method", "Player.GetActivePlayers" },
                };

                var json = JsonConvert.SerializeObject(obj);
                var request = new HttpRequestMessage(HttpMethod.Post, uri);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                var responseData = JsonConvert.DeserializeObject<JObject>(responseJson);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }


        private async Task<VideoItemInfo> GetItemInfo()
        {
            var uri = "";
            var obj = new Dictionary<string, object>
            {
                { "jsonrpc", "2.0" },
                { "method", "Player.GetItem" },
                //{ "params", new Dictionary<string, object>
                //    {
                //        { "playerid", "1", },
                //        { "properties", "\"title\",\"season\",\"episode\",\"plot\",\"runtime\",\"showtitle\",\"thumbnail\""},
                //    }
                //},
                { "id", 1}, 
                { "params", JsonConvert.DeserializeObject<JObject>("{\"playerid\":1,\"properties\":[\"title\",\"season\",\"episode\",\"plot\",\"runtime\",\"showtitle\",\"thumbnail\"]}") },
            };
            
            var json = JsonConvert.SerializeObject(obj);
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var responseData = JsonConvert.DeserializeObject<JObject>(responseJson);
            
            VideoItemInfo videoItemInfo = null;
            var error = responseData.SelectTokenOrDefault<JObject>("error");
            var errorMessage = error.GetPropertyValue<string>("message");
            if (!string.IsNullOrEmpty(errorMessage))
            {
                Console.WriteLine("Error getting item info, error: " + errorMessage);
            }
            else
            {
                var itemObj = responseData.SelectTokenOrDefault<JObject>("result.item");
                if (itemObj != null)
                {
                    videoItemInfo = itemObj.ToObject<VideoItemInfo>();
                    var runtime = itemObj.GetPropertyValue<int>("runtime");
                    videoItemInfo.Runtime = TimeSpan.FromSeconds(runtime);
                }
            }
            return videoItemInfo;
        }
        
        private async Task GetPlaybackInfo()
        {
            var uri = "";
            var obj = new Dictionary<string, object>
            {
                { "jsonrpc", "2.0" },
                { "method", "Player.GetProperties" },
                { "id", 1}, 
                { "params", JsonConvert.DeserializeObject<JObject>("{\"playerid\":1,\"properties\":[\"playlistid\",\"speed\",\"position\",\"totaltime\",\"time\"]}") },
            };
            
            var json = JsonConvert.SerializeObject(obj);
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var responseData = JsonConvert.DeserializeObject<JObject>(responseJson);
            
            var error = responseData.SelectTokenOrDefault<JObject>("error");
            var errorMessage = error.GetPropertyValue<string>("message");
            if (!string.IsNullOrEmpty(errorMessage))
            {
                Console.WriteLine("Error getting payback info, error: " + errorMessage);
            }
            else
            {
                var itemObj = responseData.SelectTokenOrDefault<JObject>("result");
                if (itemObj != null)
                {
                    var speed = itemObj.GetPropertyValue<int>("speed");
                    var paused = speed == 0;
                }
            }
        }



        public async Task StoreSessionInfo(WatchSessionInfo session)
        {
            try
            {
                Debug.WriteLine("Begin creating odbc entry");

                //var connectionString = "Driver={SQL Server};Server=.;UID=Developer;PWD=123456789;Database=OdbcTest";
                //var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MyLifeDatabase"].ConnectionString;
                var connectionString = _settings.ConnString;
                var odbc = new OdbcConnection(connectionString);
                if (odbc.State != ConnectionState.Open)
                    odbc.Open();

                var cmd = odbc.CreateCommand();
                var changes = 0;
                if (session.SessionID <= 0)
                {
                    // Initialize new session
                    // todo: Close already active sessions?

                    var sql = "INSERT INTO Kodi_VideoSessions(StartTime, EndTime, Active) " +
                              "VALUES (?, ?, ?); SELECT ? = @@IDENTITY";
                    var sessionIDParam = new OdbcParameter("@SessionID", OdbcType.BigInt, 4)
                    {
                        Direction = ParameterDirection.Output,
                    };
                    cmd.CommandText = sql;
                    cmd.Parameters.AddWithValue("@StartTime", session.StartTime);
                    cmd.Parameters.AddWithValue("@EndTime", session.EndTime);
                    cmd.Parameters.AddWithValue("@Active", session.Active);
                    cmd.Parameters.Add(sessionIDParam);
                    changes += cmd.ExecuteNonQuery();
                    session.SessionID = (long)sessionIDParam.Value;
                }
                else
                {
                    var sql = "UPDATE Kodi_VideoSessions " +
                              "SET StartTime = ?, " +
                              "    EndTime = ?, " +
                              "    Active = ? " +
                              "WHERE ID = ?";
                    cmd.CommandText = sql;
                    cmd.Parameters.AddWithValue("@StartTime", session.StartTime);
                    cmd.Parameters.AddWithValue("@EndTime", session.EndTime);
                    cmd.Parameters.AddWithValue("@Active", session.Active);
                    cmd.Parameters.AddWithValue("@SessionID", session.SessionID);
                    changes += cmd.ExecuteNonQuery();
                }

                if (changes > 0)
                    Console.WriteLine("Successfully created/updated video session");
                else
                    Console.WriteLine("No changes commited when trying to create/update video session");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to create odbc entry, Error: " + ex.Message);
                throw;
            }
        }

        public async Task DeleteSessionVideo(CrSessionVideo sessionVideo)
        {
            try
            {
                if (sessionVideo == null || sessionVideo.SessionID <= 0 || sessionVideo.ViewedVideoID <= 0)
                    return;
                
                var connectionString = _settings.ConnString;
                var odbc = new OdbcConnection(connectionString);
                if (odbc.State != ConnectionState.Open)
                    odbc.Open();
                
                Debug.WriteLine("Begin deleting odbc entry");
                var cmd = odbc.CreateCommand();
                cmd.CommandText = "DELETE FROM Kodi_CrSessionVideos " +
                                  "WHERE SessionID = ? AND VideoID = ?";
                cmd.Parameters.AddWithValue("@SessionID", sessionVideo.SessionID);
                cmd.Parameters.AddWithValue("@VideoID", sessionVideo.ViewedVideoID);
                var changes = cmd.ExecuteNonQuery();
                if (changes > 0)
                    Console.WriteLine("Successfully deleted video session (too short)");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to create odbc entry, Error: " + ex.Message);
                throw;
            }
        }

        public async Task DeleteSessionInfo(WatchSessionInfo session)
        {
            try
            {
                if (session == null || session.SessionID <= 0)
                    return;
                
                var connectionString = _settings.ConnString;
                var odbc = new OdbcConnection(connectionString);
                if (odbc.State != ConnectionState.Open)
                    odbc.Open();
                
                var cmd = odbc.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM Kodi_CrSessionVideos WHERE SessionID = ?";
                cmd.Parameters.AddWithValue("@SessionID", session.SessionID);
                var hasVideos = (int)cmd.ExecuteScalar() > 0;
                if (!hasVideos)
                {
                    Debug.WriteLine("Begin deleting odbc entry");
                    cmd = odbc.CreateCommand();
                    cmd.CommandText = "DELETE FROM Kodi_VideoSessions " +
                                      "WHERE ID = ?";
                    cmd.Parameters.AddWithValue("@SessionID", session.SessionID);
                    var changes = cmd.ExecuteNonQuery();
                    if (changes > 0)
                        Console.WriteLine("Successfully deleted view session (too short)");
                }
                else
                    Debug.WriteLine("Ignoring deletion of SessionInfo as it has videos (with valid length)");

            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to create odbc entry, Error: " + ex.Message);
                throw;
            }
        }


        public async Task<WatchSessionInfo> GetLastSession()
        {
            try
            {
                var connectionString = _settings.ConnString;
                var odbc = new OdbcConnection(connectionString);
                if (odbc.State != ConnectionState.Open)
                    odbc.Open();

                var cmd = odbc.CreateCommand();
                cmd.CommandText = "SELECT TOP(1) * FROM Kodi_VideoSessions ORDER BY ID DESC";
                WatchSessionInfo session = null;
                using (OdbcDataReader reader = cmd.ExecuteReader())
                {
                    var cSessionID = reader.GetOrdinal("ID");
                    var cStartTime = reader.GetOrdinal("StartTime");
                    var cEndTime = reader.GetOrdinal("EndTime");
                    var cActive = reader.GetOrdinal("Active");
                    foreach (IDataRecord record in reader)
                    {
                        session = new WatchSessionInfo();
                        session.SessionID = record.GetInt64(cSessionID);
                        session.StartTime = record.GetDateTime(cStartTime);
                        session.EndTime = session.LastPollTime = record.GetDateTime(cEndTime);
                        session.Active = record.GetBoolean(cActive);

                        try
                        {
                            var cmd2 = odbc.CreateCommand();
                            cmd2.CommandText = "SELECT * FROM Kodi_CrSessionVideos " +
                                               "WHERE SessionID = ?";
                            cmd2.Parameters.AddWithValue("@SessionID", session.SessionID);
                            using (OdbcDataReader reader2 = cmd2.ExecuteReader())
                            {
                                var cSessionID2 = reader2.GetOrdinal("SessionID");
                                var cVideoID = reader2.GetOrdinal("VideoID");
                                var cStartTime2 = reader2.GetOrdinal("StartTime");
                                var cEndTime2 = reader2.GetOrdinal("EndTime");
                                var cActive2 = reader2.GetOrdinal("Active");
                                foreach (IDataRecord record2 in reader2)
                                {
                                    var sessionVideo = new CrSessionVideo();
                                    sessionVideo.SessionID = record2.GetInt64(cSessionID2);
                                    sessionVideo.ViewedVideoID = record2.GetInt64(cVideoID);
                                    //sessionVideo.Video = // todo: load video as well
                                    sessionVideo.Active = record2.GetBoolean(cActive2);
                                    sessionVideo.StartTime = record2.GetDateTime(cStartTime2);
                                    sessionVideo.EndTime = record2.GetDateTime(cEndTime2);
                                    sessionVideo.SessionVideoCreated = true;
                                    session.Videos.Add(sessionVideo);
                                    session.ActiveVideo = sessionVideo;
                                    sessionVideo.Video = await GetVideo(sessionVideo.ViewedVideoID);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            
                        }
                        return session;
                    }
                }
                return session;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to create odbc entry, Error: " + ex.Message);
                throw;
            }
        }


        public async Task<VideoItemInfo> GetVideo(long videoID)
        {
            try
            {
                var connectionString = _settings.ConnString;
                var odbc = new OdbcConnection(connectionString);
                if (odbc.State != ConnectionState.Open)
                    odbc.Open();

                var cmd = odbc.CreateCommand();
                cmd.CommandText = "SELECT * FROM Kodi_ViewedVideos " +
                                  "WHERE ID = ?";
                cmd.Parameters.AddWithValue("@VideoID", videoID);
                VideoItemInfo video = null;
                using (OdbcDataReader reader = cmd.ExecuteReader())
                {
                    //var cVideoID = reader.GetOrdinal("ID");
                    var cType = reader.GetOrdinal("Type");
                    var cTitle = reader.GetOrdinal("Title");
                    var cShowtitle = reader.GetOrdinal("Showtitle");
                    var cLabel = reader.GetOrdinal("Label");
                    var cRuntime = reader.GetOrdinal("Runtime");
                    var cSeason = reader.GetOrdinal("Season");
                    var cEpisode = reader.GetOrdinal("Episode");
                    var cThumbnail = reader.GetOrdinal("Thumbnail");
                    foreach (IDataRecord record in reader)
                    {
                        // todo: null protection
                        video = new VideoItemInfo();
                        video.Type = record.GetString(cType);
                        video.Title = record.GetString(cTitle);
                        video.Showtitle = record.GetString(cShowtitle);
                        video.Label = record.GetString(cLabel);
                        video.Runtime = TimeSpan.FromSeconds(record.GetInt32(cRuntime));
                        video.Season = record.GetInt32(cSeason);
                        video.Episode = record.GetInt32(cEpisode);
                        video.Thumbnail = record.GetString(cThumbnail);
                        return video;
                    }
                }
                return video;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to create odbc entry, Error: " + ex.Message);
                throw;
            }
        }


        public async Task StoreSessionVideo(CrSessionVideo sessionVideo)
        {
            try
            {
                var connectionString = _settings.ConnString;
                var odbc = new OdbcConnection(connectionString);
                if (odbc.State != ConnectionState.Open)
                    odbc.Open();

                OdbcCommand cmd;
                var changes = 0;
                var video = sessionVideo.Video;
                var sql = "";
                if (sessionVideo.ViewedVideoID <= 0)
                {
                    // Find matching video entity
                    sql = "SELECT ID FROM Kodi_ViewedVideos " +
                          "WHERE Type = ? AND " +
                          "      Title = ? AND " +
                          "      Showtitle = ? AND " +
                          "      Label = ? AND " +
                        //"      Runtime = ? AND " +
                          "      Episode = ? AND " +
                          "      Season = ?";
                    cmd = odbc.CreateCommand();
                    cmd.CommandText = sql;
                    cmd.Parameters.AddWithValue("@Type", !string.IsNullOrEmpty(video.Type) ? (object)video.Type : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Title", !string.IsNullOrEmpty(video.Title) ? (object)video.Title : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Showtitle", !string.IsNullOrEmpty(video.Showtitle) ? (object)video.Showtitle : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Label", !string.IsNullOrEmpty(video.Label) ? (object)video.Label : DBNull.Value);
                    //cmd.Parameters.AddWithValue("@Runtime", (int)Math.Ceiling(video.Runtime.TotalSeconds));
                    cmd.Parameters.AddWithValue("@Episode", video.Episode);
                    cmd.Parameters.AddWithValue("@Season", video.Season);
                    var obj = cmd.ExecuteScalar();
                    sessionVideo.ViewedVideoID = obj != null && obj != DBNull.Value ? (long) obj : 0;
                }
                if (sessionVideo.ViewedVideoID <= 0)
                {
                    // Add viewed video
                    sql = "INSERT INTO Kodi_ViewedVideos(Type, Title, Showtitle, Label, Runtime, Episode, Season, Thumbnail) " +
                          "VALUES (?, ?, ?, ?, ?, ?, ?, ?); SELECT ? = @@IDENTITY";
                    var videoIDParam = new OdbcParameter("@VideoID", OdbcType.BigInt, 4)
                    {
                        Direction = ParameterDirection.Output,
                    };
                    cmd = odbc.CreateCommand();
                    cmd.CommandText = sql;
                    cmd.Parameters.AddWithValue("@Type", !string.IsNullOrEmpty(video.Type) ? (object)video.Type : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Title", !string.IsNullOrEmpty(video.Title) ? (object)video.Title : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Showtitle", !string.IsNullOrEmpty(video.Showtitle) ? (object)video.Showtitle : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Label", !string.IsNullOrEmpty(video.Label) ? (object)video.Label : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Runtime", (int)Math.Ceiling(video.Runtime.TotalSeconds));
                    cmd.Parameters.AddWithValue("@Episode", video.Episode);
                    cmd.Parameters.AddWithValue("@Season", video.Season);
                    cmd.Parameters.AddWithValue("@Thumbnail", !string.IsNullOrEmpty(video.Thumbnail) ? (object)video.Thumbnail : DBNull.Value);
                    cmd.Parameters.Add(videoIDParam);
                    changes += cmd.ExecuteNonQuery();
                    sessionVideo.ViewedVideoID = (long)videoIDParam.Value;
                }

                if (!sessionVideo.SessionVideoCreated)
                {
                    // Add CrSessionVideo
                    sql = "INSERT INTO Kodi_CrSessionVideos(SessionID, VideoID, StartTime, EndTime, Active) " +
                          "VALUES (?, ?, ?, ?, ?)";
                    cmd = odbc.CreateCommand();
                    cmd.CommandText = sql;
                    cmd.Parameters.AddWithValue("@SessionID", sessionVideo.SessionID);
                    cmd.Parameters.AddWithValue("@VideoID", sessionVideo.ViewedVideoID);
                    cmd.Parameters.AddWithValue("@StartTime", sessionVideo.StartTime);
                    cmd.Parameters.AddWithValue("@EndTime", sessionVideo.EndTime);
                    cmd.Parameters.AddWithValue("@Active", sessionVideo.Active);
                    changes += cmd.ExecuteNonQuery();
                    sessionVideo.SessionVideoCreated = true;
                }
                else
                {
                    // Update CrSessionVideo
                    sql = "UPDATE Kodi_CrSessionVideos " +
                              "SET StartTime = ?, " +
                              "    EndTime = ?, " +
                              "    Active = ? " +
                              "WHERE SessionID = ? AND VideoID = ?";
                    cmd = odbc.CreateCommand();
                    cmd.CommandText = sql;
                    cmd.Parameters.AddWithValue("@StartTime", sessionVideo.StartTime);
                    cmd.Parameters.AddWithValue("@EndTime", sessionVideo.EndTime);
                    cmd.Parameters.AddWithValue("@Active", sessionVideo.Active);
                    cmd.Parameters.AddWithValue("@SessionID", sessionVideo.SessionID);
                    cmd.Parameters.AddWithValue("@VideoID", sessionVideo.ViewedVideoID);
                    changes += cmd.ExecuteNonQuery();
                }


                if (changes > 0)
                    Console.WriteLine("Successfully added/updated viewed video");
                else
                    Console.WriteLine("No changes commited when trying to add video to viewed");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to create odbc entry, Error: " + ex.Message);
                throw;
            }
        }



        public async Task CloseActiveVideos()
        {
            try
            {
                var connectionString = _settings.ConnString;
                var odbc = new OdbcConnection(connectionString);
                if (odbc.State != ConnectionState.Open)
                    odbc.Open();

                var cmd = odbc.CreateCommand();
                var changes = 0;
                var sql = "UPDATE Kodi_CrSessionVideos " +
                            "SET Active = ? " +
                            "WHERE Active = ?";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@Active", false);
                cmd.Parameters.AddWithValue("@Active", true);
                changes += cmd.ExecuteNonQuery();

                if (changes > 0)
                    Console.WriteLine("Closed '{0}' active videos", changes);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to create odbc entry, Error: " + ex.Message);
                throw;
            }
        }


        public async Task CloseActiveSessions()
        {
            try
            {
                var connectionString = _settings.ConnString;
                var odbc = new OdbcConnection(connectionString);
                if (odbc.State != ConnectionState.Open)
                    odbc.Open();

                var cmd = odbc.CreateCommand();
                var changes = 0;
                var sql = "UPDATE Kodi_VideoSessions " +
                            "SET Active = ? " +
                            "WHERE Active = ?";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@Active", false);
                cmd.Parameters.AddWithValue("@Active", true);
                changes += cmd.ExecuteNonQuery();

                if (changes > 0)
                    Console.WriteLine("Closed '{0}' active sessions", changes);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to create odbc entry, Error: " + ex.Message);
                throw;
            }
        }


        public string TimeSpanToString(TimeSpan timeSpan)
        {
            var res = "";
            if (timeSpan != TimeSpan.Zero)
            {
                if (timeSpan.TotalMinutes < 1)
                    res += string.Format(" for {0} seconds", timeSpan.Seconds);
                else if (timeSpan.TotalHours < 1)
                    res += string.Format(" for {0}m {1}s", timeSpan.Minutes, timeSpan.Seconds);
                else
                    res += string.Format(" for {0}h {1}m", timeSpan.Hours, timeSpan.Minutes);
            }
            return res;
        }


        private enum DiffType
        {
            None,
            StartedWatching,
            StoppedWatching,
            ChangedProgram,
            StillWatching,
        }

    }
}
