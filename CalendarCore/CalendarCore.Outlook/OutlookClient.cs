using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CalendarCore.Http;
using Newtonsoft.Json.Linq;

namespace CalendarCore.Outlook
{
    public class OutlookClient : ICalendarServer
    {
        private string scope;
        private string client_id;
        private string client_secret;
        private string accessTokenUrl;
        private Uri signInUrl;
        private string apiUrl = @"https://apis.live.net/v5.0/";
        
        private string auth_code;
        private string access_token;
        private JObject _authData;
        
        private readonly HttpHelperBase httpHelper = new JsonHttpHelper();
        //private HttpHelperBase httpHelper = new HttpHelper();
        public Dictionary<string, string> tokenData = new Dictionary<string, string>();

        public event EventHandler<AuthorizationNeededEventArgs> OnAuthorizationNeeded;
        public event EventHandler OnAuthorized;


        public OutlookClient()
        {
            scope = "wl.basic wl.calendars wl.calendars_update wl.events_create";
            client_id = "0000000040141301";
            client_secret = "Gwfh2lFMtbD6AQQklJUbdo6dyx-DdPeY";
            accessTokenUrl = string.Format(@"https://login.live.com/oauth20_token.srf?client_id={0}&client_secret={1}&redirect_uri=https://login.live.com/oauth20_desktop.srf&grant_type=authorization_code&code=", client_id, client_secret);
            signInUrl = new Uri(string.Format(@"https://login.live.com/oauth20_authorize.srf?client_id={0}&redirect_uri=https://login.live.com/oauth20_desktop.srf&response_type=code&scope={1}", client_id, scope));
        }




        private AuthEventArgs _args;

        
        public async Task<bool> Authenticate()
        {
            Authenticate(false);


            var res = !string.IsNullOrEmpty(access_token) && !string.IsNullOrEmpty(auth_code);
            return res;
        }
        

        private async void Authenticate(bool force)
        {
            if (OnAuthorizationNeeded == null)
                throw new InvalidOperationException("Supply a function to OnAuthorizationNeeded");
            if (OnAuthorized == null)
                throw new InvalidOperationException("Supply a function to OnAuthorized");

            InitAuth(force);
            
            //await Yield(this);
            //await InitAccess(force);
        }


        private async void InitAuth(bool force)
        {
            // todo: make awaitable

            if (string.IsNullOrEmpty(auth_code) || force)
            {
                OnAuthorizationNeeded(this, new AuthorizationNeededEventArgs(signInUrl.ToString())
                {
                    OnUrlLoaded = OnUrlLoaded,
                });
            }
        }



        private static Awaiter Yield(OutlookClient client)
        {
            return new Awaiter(client);
        }

        private struct Awaiter : System.Runtime.CompilerServices.INotifyCompletion
        {
            private static readonly List<Awaiter> instances = new List<Awaiter>();

            private readonly OutlookClient _client;

            public Awaiter(OutlookClient client)
            {
                _client = client;
            }

            public Awaiter GetAwaiter()
            {
                if (!instances.Contains(this))
                    instances.Add(this);
                return this;
            }

            public bool IsCompleted
            {
                get
                {
                    var r = !string.IsNullOrEmpty(_client.auth_code);
                    return r;
                }
            }

            public async void OnCompleted(Action continuation)
            {
                //ThreadPool.QueueUserWorkItem((state) => ((Action)state)(), continuation);
                //Task.Run(continuation).Wait();

                if (!IsCompleted)
                    await GetAwaiter();
                else
                    await Task.Run(continuation);

                //Task.Run(() => { }).Wait(5000);
            }

            public void GetResult()
            {
                
            }
        }


        private async Task<string> InitAccess(bool force)
        {
            if (string.IsNullOrEmpty(access_token) || force)
            {
                var url = accessTokenUrl + auth_code;

                var request = new HttpHelperRequest
                {
                    Url = url,
                    Method = "GET",
                };
                var response = await MakeWebRequest<JObject>(request);
                _authData = response.Result;
                access_token = _authData.GetPropertyValue<string>("access_token");

                if (!string.IsNullOrEmpty(access_token))
                    OnAuthorized(this, EventArgs.Empty);
                return access_token;
            }
            return null;
        }


        private async void OnUrlLoaded(AuthEventArgs e)
        {
            var uri = new Uri(e.Url);

            if (uri.AbsoluteUri.Contains("code="))
            {
                auth_code = System.Text.RegularExpressions.Regex.Split(uri.AbsoluteUri, "code=")[1];
                e.Authorized = true;

                await InitAccess(false);
            }
        }


        //private void getAccessToken()
        //{
        //    if (!string.IsNullOrEmpty(auth_code))
        //    {
        //        makeAccessTokenRequest(accessTokenUrl + auth_code);
        //    }
        //    else
        //        throw new InvalidOperationException("No access token");
        //}

        
        //private void makeAccessTokenRequest(string requestUrl)
        //{
        //    WebClient wc = new WebClient();
        //    wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(accessToken_DownloadStringCompleted);
        //    wc.DownloadStringAsync(new Uri(requestUrl));
        //}

        //void accessToken_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        //{
        //    tokenData = deserializeJson(e.Result);
        //    if (tokenData.ContainsKey("access_token"))
        //    {
        //        App.Current.Properties.Add("access_token", tokenData["access_token"]);
        //        getUserInfo();
        //    }
        //}

        //private Dictionary<string, string> deserializeJson(string json)
        //{
        //    //var jss = new JavaScriptSerializer();
        //    //var d = jss.Deserialize<Dictionary<string, string>>(json);

        //    var d = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        //    return d;
        //}

        //private void getUserInfo()
        //{
        //    if (App.Current.Properties.Contains("access_token"))
        //    {
        //        makeApiRequest(apiUrl + "me?access_token=" + App.Current.Properties["access_token"]);
        //    }
        //}

        //private void makeApiRequest(string requestUrl)
        //{
        //    WebClient wc = new WebClient();
        //    wc.Encoding = System.Text.Encoding.UTF8;

        //    wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_DownloadStringCompleted);
        //    wc.DownloadStringAsync(new Uri(requestUrl));
        //}


        //private void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        //{
        //    try
        //    {
        //        changeView(e.Result);
        //    }
        //    catch (Exception ex)
        //    {
        //        //throw;
        //    }
        //}

        //private void changeView(string result)
        //{
        //    btnSignIn.Visibility = Visibility.Collapsed;
        //    txtUserInfo.Text = result;
        //    string imgUrl = apiUrl + "me/picture?access_token=" + App.Current.Properties["access_token"];
        //    imgUser.Source = new BitmapImage(new Uri(imgUrl, UriKind.RelativeOrAbsolute));
        //    txtToken.Text += "access_token = " + App.Current.Properties["access_token"] + "\r\n\r\n";
        //}

        //private void btnSignIn_Click(object sender, RoutedEventArgs e)
        //{
        //    BrowserWindow browser = new BrowserWindow();
        //    browser.Closed += new EventHandler(browser_Closed);
        //    browser.Show();
        //}

        private void Logout()
        {
            access_token = null;
            auth_code = null;
        }


        void browser_Closed(object sender, EventArgs e)
        {
            //getAccessToken();
        }


        public async Task<Calendar> GetCalendar(string id)
        {
            //await Authenticate();
            //await InitAccess(false);

            var url = Path.Combine(apiUrl, string.Format("{0}?access_token={1}", id, access_token));

            var request = new HttpHelperRequest
            {
                Url = url,
                Method = "GET",
                ContentType = "",
            };
            var response = await MakeWebRequest<JObject>(request);
            var calendar = JsonDtoConverter.ToCalendar(response.Result);
            return calendar;
        }


        public async Task<IEnumerable<Calendar>> GetCalendars()
        {
            //await Authenticate();
            //await InitAccess(false);


            var url = Path.Combine(apiUrl, string.Format("me/calendars?access_token={0}", access_token));

            var request = new HttpHelperRequest
            {
                Url = url,
                Method = "GET",
                ContentType = "",
            };
            var response = await MakeWebRequest<JObject>(request);
            var obj = response.Result;
            var arr = obj.GetPropertyValue<JArray>("data");
            var calendars = JsonDtoConverter.ToCalendars(arr);
            return calendars;
        }



        private async Task<IHttpHelperResponse<T>> MakeWebRequest<T>(IHttpHelperRequest request)
        {
            IHttpHelperResponse<T> response = null;
            try
            {
                response = await httpHelper.SendAsync<T>(request);
            }
            catch (WebException ex)
            {
                var errorResponse = ex.Response;
                var stream = errorResponse.GetResponseStream();
                string res;
                using (var sr = new StreamReader(stream, Encoding.UTF8))
                {
                    res = sr.ReadToEnd();
                }
                Debug.WriteLine("---Error in WebRequest---");
                Debug.WriteLine("Error: " + ex.Message);
                Debug.WriteLine("Request: {0}: {1}", request.Method, request.Url);
                Debug.WriteLine("Error Result: " + res);

                foreach (var header in errorResponse.Headers.AllKeys)
                {
                    Debug.WriteLine("Header: {0} = {1}", header, ex.Response.Headers[header]);
                }
                Debug.WriteLine("");

                throw;
            }
            return response;
        }


    }
}
