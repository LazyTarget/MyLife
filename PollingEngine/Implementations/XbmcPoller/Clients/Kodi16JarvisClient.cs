using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AustinHarris.JsonRpc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace XbmcPoller
{
    public class Kodi16JarvisClient : IKodiClient
    {
        private JsonRpcClient _client;
        private IXbmcPollerSettings _settings;

        public Kodi16JarvisClient()
            : this(XbmcPollerSettingsConfigElement.LoadFromConfig())
        {
        }

        public Kodi16JarvisClient(IXbmcPollerSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            _settings = settings;


            ICredentials credentials = new NetworkCredential(settings.ApiUsername, settings.ApiPassword);

            var uri = new Uri(_settings.ApiBaseUrl);
            _client = new JsonRpcClient(uri, credentials);
        }


        private async Task<T> InvokeUntilResult<T>(string json)
        {
            var autoReset = new AutoResetEvent(false);
            try
            {
                T result = default(T);
                Exception ex = null;
                var obs = _client.Invoke<T>(json, TaskPoolScheduler.Default);
                var sub = obs.Subscribe(
                    onNext: (x) =>
                    {
                        result = x.Result;
                    },
                    onError: (x) =>
                    {
                        result = default(T);
                        ex = x;
                        autoReset?.Set();
                    },
                    onCompleted: () =>
                    {
                        autoReset?.Set();
                    }
                );
                using (sub)
                {
                    autoReset.WaitOne();
                }

                if (ex != null)
                    throw ex;
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return default(T);
            }
            finally
            {
                autoReset?.Dispose();
                autoReset = null;
            }
        }


        public async Task<bool> PingXbmc()
        {
            try
            {
                var uri = "";
                var obj = new Dictionary<string, object>
                {
                    {"jsonrpc", "2.0"},
                    {"method", "Player.GetActivePlayers"},
                };

                var json = JsonConvert.SerializeObject(obj);
                var req = new JsonRequest();
                req.Method = "Player.GetActivePlayers";

                var result = await InvokeUntilResult<JObject>(json);
                var success = result != null;
                return success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }


        public async Task<string> GetVersion()
        {
            var result = await PingXbmc();
            if (result)
                return "16";
            return null;
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
                //{ "id", 1}, 
                { "params", JsonConvert.DeserializeObject<JObject>("{\"playerid\":1,\"properties\":[\"title\",\"season\",\"episode\",\"plot\",\"runtime\",\"showtitle\",\"thumbnail\"]}") },
            };
            
            var json = JsonConvert.SerializeObject(obj);
            
            var request = new JsonRequest();
            //request.Id = 1;
            request.Method = "Player.GetItem";
            request.Params = obj["params"];

            var responseData = await InvokeUntilResult<JObject>(json);
            
            VideoItemInfo videoItemInfo = null;
            var error = responseData?.SelectTokenOrDefault<JObject>("error");
            var errorMessage = error?.GetPropertyValue<string>("message");
            if (!string.IsNullOrEmpty(errorMessage))
            {
                Console.WriteLine("Error getting item info, error: " + errorMessage);
            }
            else
            {
                var itemObj = responseData?.SelectTokenOrDefault<JObject>("result.item");
                if (itemObj != null)
                {
                    videoItemInfo = itemObj.ToObject<VideoItemInfo>();
                    var runtime = itemObj.GetPropertyValue<int>("runtime");
                    videoItemInfo.Runtime = TimeSpan.FromSeconds(runtime);
                }
            }
            return videoItemInfo;
        }

        
        public async Task GetPlaybackInfo()
        {
            var uri = "";
            var obj = new Dictionary<string, object>
            {
                { "jsonrpc", "2.0" },
                { "method", "Player.GetProperties" },
                //{ "id", 1}, 
                { "params", JsonConvert.DeserializeObject<JObject>("{\"playerid\":1,\"properties\":[\"playlistid\",\"speed\",\"position\",\"totaltime\",\"time\"]}") },
            };
            
            var json = JsonConvert.SerializeObject(obj);
            var request = new JsonRequest();
            //request.Id = 1;
            request.Method = "Player.GetProperties";
            request.Params = obj["params"];

            var responseData = await InvokeUntilResult<JObject>(json);
            
            var error = responseData?.SelectTokenOrDefault<JObject>("error");
            var errorMessage = error?.GetPropertyValue<string>("message");
            if (!string.IsNullOrEmpty(errorMessage))
            {
                Console.WriteLine("Error getting payback info, error: " + errorMessage);
            }
            else
            {
                var itemObj = responseData?.SelectTokenOrDefault<JObject>("result");
                if (itemObj != null)
                {
                    var speed = itemObj.GetPropertyValue<int>("speed");
                    var paused = speed == 0;
                }
            }
        }
        
    }
}
