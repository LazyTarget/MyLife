using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace XbmcPoller
{
    public class Kodi14HelixClient : IKodiClient
    {
        private HttpClient _client;
        private IXbmcPollerSettings _settings;

        public Kodi14HelixClient()
            : this(XbmcPollerSettingsConfigElement.LoadFromConfig())
        {
        }

        public Kodi14HelixClient(IXbmcPollerSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            _settings = settings;


            var handler = new HttpClientHandler()
            {
                Credentials = new NetworkCredential(_settings.ApiUsername, _settings.ApiPassword),
            };
            var uri = new Uri(_settings.ApiBaseUrl);
            _client = new HttpClient(handler);
            _client.BaseAddress = uri;
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        


        public async Task<bool> PingXbmc()
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


        public async Task<string> GetVersion()
        {
            throw new NotImplementedException();

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
                return "14";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return null;
            }
        }


        public async Task<VideoItemInfo> GetItemInfo()
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
            
            VideoItemInfo result = null;
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
                    var videoItemInfo =
                        result = itemObj.ToObject<VideoItemInfo>();
                    var runtime = itemObj.GetPropertyValue<int>("runtime");
                    videoItemInfo.Runtime = TimeSpan.FromSeconds(runtime);

                    if (videoItemInfo.Type == "unknown")
                    {
                        //result = null;
                    }
                    if (string.IsNullOrWhiteSpace(videoItemInfo.Label) &&
                        string.IsNullOrWhiteSpace(videoItemInfo.Title))
                    {
                        result = null;
                    }
                }
            }
            return result;
        }

        
        public async Task GetPlaybackInfo()
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
        
    }
}
