using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Odbc;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
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
        private readonly HttpClient _client;
        private readonly Dictionary<DateTime, ItemInfo> _data = new Dictionary<DateTime, ItemInfo>();

        public XbmcPoller()
        {
            var handler = new HttpClientHandler()
            {
                Credentials = new NetworkCredential("xbmc", "e4d5exd5"),
            };
            _client = new HttpClient(handler);
            _client.BaseAddress = new Uri("http://localhost:8082/jsonrpc");

            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }


        public async Task OnStarting(PollingContext context)
        {
            
        }

        public async Task OnInterval(PollingContext context)
        {
            var time = DateTime.Now;
            var pollStartTime = context.TimeStarted;
            if (pollStartTime == DateTime.MinValue || !_data.Any())
                pollStartTime = time;
            
            var lastInfo = _data.Select(x => x.Value).LastOrDefault();
            var lastPollTime = _data.Select(x => x.Key).LastOrDefault();
            if (lastPollTime == DateTime.MinValue)
                lastPollTime = time;
            
            bool isDiff;
            bool ended = false;
            ItemInfo itemInfo = null;
            try
            {
                itemInfo = await GetItemInfo();
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
            if (itemInfo != null)
            {
                if (lastInfo == null)
                {
                    // Started watching
                    isDiff = true;
                    //Console.WriteLine("Started watching '{0}'{1}", itemInfo.Title, TimeSpanToString(_timeSinceDiff));
                    diffType = DiffType.StartedWatching;
                }
                else
                {
                    // Compare watch info
                    isDiff = CompareTo(itemInfo, lastInfo) != 0;

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
                if (lastInfo == null)
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
            _data.Add(time, itemInfo);

            
            if (diffType == DiffType.None && isDiff)
            {
                _timeSinceDiff = TimeSpan.Zero;
            }


            var timeDiff = time.Subtract(lastPollTime);
            _timeSinceDiff = _timeSinceDiff.Add(timeDiff);

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
                    Console.WriteLine("Started watching '{0}'{1}", itemInfo.Title, TimeSpanToString(_timeSinceDiff));
                    break;
                case DiffType.StoppedWatching:
                    Console.WriteLine("Stopped watching '{0}'{1}", lastInfo.Title, TimeSpanToString(_timeSinceDiff));
                    break;
                case DiffType.StillWatching:
                    Console.WriteLine("Still watching '{0}'{1}", itemInfo.Title, TimeSpanToString(_timeSinceDiff));
                    break;
                case DiffType.ChangedProgram:
                    Console.WriteLine("Changed program from '{0}' to '{1}'{2}", lastInfo.Title, itemInfo.Title, TimeSpanToString(_timeSinceDiff));
                    break;
            }


            if (ended)
            {
                var start = time.Subtract(_timeSinceDiff);

                // Add to odbc database
                var watchInfo = new WatchInfo
                {
                    Item = lastInfo,
                    Start = start,
                    End = time,
                    Duration = _timeSinceDiff,
                };
                OnStoppedWatching(watchInfo);

                _timeSinceDiff = TimeSpan.Zero;
            }
        }

        public enum DiffType
        {
            None,
            StartedWatching,
            StoppedWatching,
            ChangedProgram,
            StillWatching,
        }

        public async Task OnStopping(PollingContext context)
        {
            
        }

        public void ApplyArguments(string[] args)
        {
            
        }


        private async Task<ItemInfo> GetItemInfo()
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
            
            ItemInfo itemInfo = null;
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
                    itemInfo = new ItemInfo();
                    itemInfo.Type = itemObj.GetPropertyValue<string>("type");
                    itemInfo.Label = itemObj.GetPropertyValue<string>("label");
                    if (itemInfo.Type == "episode")
                    {
                        itemInfo.Title = itemObj.GetPropertyValue<string>("showtitle");
                        itemInfo.EpisodeTitle = itemObj.GetPropertyValue<string>("title");
                    }
                    else
                        itemInfo.Title = itemObj.GetPropertyValue<string>("title");
                    
                    if (string.IsNullOrEmpty(itemInfo.Title))
                        itemInfo.Title = itemInfo.Label;

                    var runtime = itemObj.GetPropertyValue<int>("runtime");
                    itemInfo.Duration = TimeSpan.FromSeconds(runtime);
                }
            }
            return itemInfo;
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



        public void OnStoppedWatching(WatchInfo watchInfo)
        {
            try
            {
                Debug.WriteLine("Begin creating odbc entry");
                
                var text = string.Format("Watched {0} on XBMC", watchInfo.Item.Title);
                string desc;
                if (watchInfo.Item.Type == "episode")
                    desc = string.Format("Watched series '{1}', episode '{0}' XBMC", watchInfo.Item.EpisodeTitle, watchInfo.Item.Title);
                else
                    desc = string.Format("Watched {0} '{1}' on XBMC", watchInfo.Item.Type, watchInfo.Item.Title);

                if (watchInfo.Duration != TimeSpan.Zero && false)
                {
                    desc += TimeSpanToString(watchInfo.Duration);
                }


                //var connectionString = "Driver={SQL Server};Server=.;UID=Developer;PWD=123456789;Database=OdbcTest";
                var connectionString = ConfigurationManager.ConnectionStrings["PollingDatabase"].ConnectionString;
                var cn = new OdbcConnection(connectionString);
                if (cn.State != ConnectionState.Open)
                    cn.Open();

                var sql = "INSERT INTO XbmcEvents(Text, Description, StartTime, EndTime, Source) " +
                    //"VALUES (@Text, @Description, @StartTime, @EndTime, @Source)";
                            "VALUES (?, ?, ?, ?, ?)";
                var cmd = new OdbcCommand(sql, cn);
                cmd.Parameters.AddWithValue("@Text", text);
                cmd.Parameters.AddWithValue("@Description", desc);
                cmd.Parameters.AddWithValue("@StartTime", watchInfo.Start);
                cmd.Parameters.AddWithValue("@EndTime", watchInfo.End);
                cmd.Parameters.AddWithValue("@Source", "XBMC poller");

                var changes = cmd.ExecuteNonQuery();
                if (changes > 0)
                    Console.WriteLine("Created odbc event: '{0}'", text);
                else
                    Console.WriteLine("No changes commited when creating odbc entry: " + text);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to create odbc entry, Error: " + ex.Message);
                throw;
            }
        }


        public static int CompareTo(ItemInfo a, ItemInfo b)
        {
            int c;
            if (a == b)
                return 0;
            if (a == null)
                return 1;
            if (b == null)
                return -1;

            c = string.Compare(a.Type, b.Type);
            if (c != 0)
                return c;

            c = string.Compare(a.Title, b.Title);
            if (c != 0)
                return c;

            c = string.Compare(a.EpisodeTitle, b.EpisodeTitle);
            if (c != 0)
                return c;

            c = string.Compare(a.Label, b.Label);
            if (c != 0)
                return c;

            return 0;
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

    }

    public class WatchInfo
    {
        public ItemInfo Item { get; set; }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public TimeSpan Duration { get; set; }

    }
}
