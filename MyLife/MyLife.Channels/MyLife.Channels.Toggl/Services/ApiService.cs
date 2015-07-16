using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Toggl.DataObjects;
using Toggl.Interfaces;

namespace Toggl.Services
{
    public class ApiService : IApiService
    {
        private string ApiToken { get; set; }

        public Session Session { get; set; }

        public ApiService(string apiToken)
        {
            ApiToken = apiToken;
        }

        public void Initialize()
        {
            if (Session != null && !string.IsNullOrEmpty(Session.ApiToken))
            {
                return;
            }

            GetSession();
        }

        public async Task<Session> GetSession()
        {

            var args = new List<KeyValuePair<string, string>>();

            var response = await Get(ApiRoutes.Session.Me, args);
            Session = response.GetData<Session>();

            ApiToken = Session.ApiToken;

            return Session;
        }

        public async Task<ApiResponse> Get(string url)
        {
            return await Get(new ApiRequest
            {
                Url = url
            });
        }

        public async Task<ApiResponse> Get(string url, List<KeyValuePair<string, string>> args)
        {
            return await Get(new ApiRequest
            {
                Url = url,
                Args = args
            });
        }

        public async Task<TResponse> Get<TResponse>(string url, List<KeyValuePair<string, string>> args)
        {
            return await Get<TResponse>(new ApiRequest()
                                  {
                                      Url = url, Args = args
                                  });
        }

        public async Task<ApiResponse> Delete(string url)
        {
            return await Get(new ApiRequest
            {
                Url = url,
                Method = "DELETE"
            });
        }

        public async Task<ApiResponse> Delete(string url, List<KeyValuePair<string, string>> args)
        {
            return await Get(new ApiRequest
            {
                Url = url,
                Method = "DELETE",
                Args = args
            });
        }
        public async Task<ApiResponse> Post(string url, string data)
        {
            return await Get(
                new ApiRequest
                {
                    Url = url,
                    Method = "POST",
                    ContentType = "application/json",
                    Data = data
                });
        }

        public async Task<ApiResponse> Post(string url, List<KeyValuePair<string, string>> args, string data = "")
        {
            return await Get(
                new ApiRequest
                {
                    Url = url,
                    Args = args,
                    Method = "POST",
                    ContentType = "application/json",
                    Data = data
                });
        }

        public async Task<ApiResponse> Put(string url, string data)
        {
            return await Get(
                new ApiRequest
                {
                    Url = url,
                    Method = "PUT",
                    ContentType = "application/json",
                    Data = data
                });
        }

        public async Task<ApiResponse> Put(string url, List<KeyValuePair<string, string>> args, string data = "")
        {
            return await Get(
                new ApiRequest
                {
                    Url = url,
                    Args = args,
                    Method = "PUT",
                    ContentType = "application/json",
                    Data = data
                });
        }

        private async Task<TResponse> Get<TResponse>(ApiRequest apiRequest) 
        {
            string value = "";

            if (apiRequest.Args != null && apiRequest.Args.Count > 0)
            {
                apiRequest.Args.ForEach(e => value += e.Key + "=" + System.Uri.EscapeDataString(e.Value) + "&");
                value = value.Trim('&');
            }

            if (apiRequest.Method == "GET" && !string.IsNullOrEmpty(value))
            {
                apiRequest.Url += "?" + value;
            }

            var authRequest = (HttpWebRequest)HttpWebRequest.Create(apiRequest.Url);

            authRequest.Method = apiRequest.Method;

            authRequest.ContentType = apiRequest.ContentType;

            authRequest.Credentials = CredentialCache.DefaultNetworkCredentials;

            authRequest.Headers.Add(GetAuthHeader());

            var authResponse = (HttpWebResponse) await authRequest.GetResponseAsync();
            string content = "";
            using (var reader = new StreamReader(authResponse.GetResponseStream(), Encoding.UTF8))
            {
                content = reader.ReadToEnd();
            }

            var rsp = JsonConvert.DeserializeObject<TResponse>(content);              
            return rsp;
        }


        private async Task<ApiResponse> Get(ApiRequest apiRequest)
        {
            string value = "";

            if (apiRequest.Args != null && apiRequest.Args.Count > 0)
            {
                apiRequest.Args.ForEach(e => value += e.Key + "=" + System.Uri.EscapeDataString(e.Value) + "&");
                value = value.Trim('&');
            }

            if (apiRequest.Method == "GET" && !string.IsNullOrEmpty(value))
            {
                apiRequest.Url += "?" + value;
            }

            var authRequest = (HttpWebRequest)HttpWebRequest.Create(apiRequest.Url);

            authRequest.Method = apiRequest.Method;
			authRequest.ContentType = apiRequest.ContentType;
			authRequest.Credentials = CredentialCache.DefaultNetworkCredentials;
			
            authRequest.Headers.Add(GetAuthHeader());

            if (apiRequest.Method == "POST" || apiRequest.Method == "PUT")
            {
	            var utd8WithoutBom = new UTF8Encoding(false);

                value += apiRequest.Data;
                //authRequest.ContentLength = utd8WithoutBom.GetByteCount(value);
                var requestStream = await authRequest.GetRequestStreamAsync();
				using (var writer = new StreamWriter(requestStream, utd8WithoutBom))
                {
                    writer.Write(value);
                }
            }

            var authResponse = (HttpWebResponse) await authRequest.GetResponseAsync();
            string content = "";
            using (var reader = new StreamReader(authResponse.GetResponseStream(), Encoding.UTF8))
            {
                content = reader.ReadToEnd();
            }

            if ((string.IsNullOrEmpty(content)
                || content.ToLower() == "null")
                && authResponse.StatusCode == HttpStatusCode.OK
                && authResponse.Method == "DELETE")
            {
                var rsp = new ApiResponse();
                rsp.Data = new JObject();
                rsp.related_data_updated_at = DateTime.Now;
                rsp.StatusCode = authResponse.StatusCode;
                rsp.Method = authResponse.Method;

                return rsp;
            }

            try
            {
	            ApiResponse rsp = content.ToLower() == "null" 
					? new ApiResponse { Data = null } 
					: JsonConvert.DeserializeObject<ApiResponse>(content);
	            
                rsp.StatusCode = authResponse.StatusCode;
                rsp.Method = authResponse.Method;

                return rsp;
            }
            catch (Exception)
            {
                var jry = JArray.Parse(content);
                var rsp = new ApiResponse()
                    {
                        Data = jry,
                        related_data_updated_at = DateTime.Now,
                        StatusCode = authResponse.StatusCode,
                        Method = authResponse.Method
                    };

                return rsp;
            }

        }

        private string GetAuthHeader()
        {
            var encodedApiKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(ApiToken + ":api_token"));
            return "Authorization: Basic " + encodedApiKey;
        }
    }

}
