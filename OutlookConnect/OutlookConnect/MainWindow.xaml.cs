#define TRACE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;

namespace OutlookConnect
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static string client_id = "0000000040141301";
        static string client_secret = "Gwfh2lFMtbD6AQQklJUbdo6dyx-DdPeY";
        static string accessTokenUrl = String.Format(@"https://login.live.com/oauth20_token.srf?client_id={0}&client_secret={1}&redirect_uri=https://login.live.com/oauth20_desktop.srf&grant_type=authorization_code&code=", client_id, client_secret);
        static string apiUrl = @"https://apis.live.net/v5.0/";
        public Dictionary<string, string> tokenData = new Dictionary<string, string>();


        public MainWindow()
        {
            InitializeComponent();
        }


        private void getAccessToken()
        {
            if (App.Current.Properties.Contains("auth_code"))
            {
                makeAccessTokenRequest(accessTokenUrl + App.Current.Properties["auth_code"]);
            }
        }

        private void makeAccessTokenRequest(string requestUrl)
        {
            WebClient wc = new WebClient();
            wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(accessToken_DownloadStringCompleted);
            wc.DownloadStringAsync(new Uri(requestUrl));
        }

        void accessToken_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            tokenData = deserializeJson(e.Result);
            if (tokenData.ContainsKey("access_token"))
            {
                App.Current.Properties.Add("access_token", tokenData["access_token"]);
                getUserInfo();
            }
        }

        private Dictionary<string, string> deserializeJson(string json)
        {
            //var jss = new JavaScriptSerializer();
            //var d = jss.Deserialize<Dictionary<string, string>>(json);

            var d = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            return d;
        }

        private void getUserInfo()
        {
            if (App.Current.Properties.Contains("access_token"))
            {
                makeApiRequest(apiUrl + "me?access_token=" + App.Current.Properties["access_token"]);
            }
        }

        private void makeApiRequest(string requestUrl)
        {
            WebClient wc = new WebClient();
            wc.Encoding = System.Text.Encoding.UTF8;

            wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_DownloadStringCompleted);
            wc.DownloadStringAsync(new Uri(requestUrl));
        }

        private void makeApiRequest2(string requestUrl, string method = "GET")
        {
            var request = WebRequest.Create(requestUrl);
            request.Method = method;
            Trace.WriteLine(request.Method + ": " + requestUrl);
            try
            {
                var response = request.GetResponse();
                using (var stream = response.GetResponseStream())
                {
                    var reader = new StreamReader(stream, Encoding.UTF8);
                    var json = reader.ReadToEnd();
                    Debug.WriteLine("json: " + json);
                }
            }
            catch (WebException ex)
            {
                Trace.WriteLine(ex.Message);

                foreach (var header in ex.Response.Headers.AllKeys)
                {
                    Trace.WriteLine(string.Format("Header: {0} = {1}", header, ex.Response.Headers[header]));
                }

                var httpWebResponse = (HttpWebResponse) ex.Response;
                using (var stream = httpWebResponse.GetResponseStream())
                {
                    var reader = new StreamReader(stream, Encoding.UTF8);
                    var json = reader.ReadToEnd();
                    Trace.WriteLine("json: " + json);
                }
                
            }
        }

        void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                changeView(e.Result);
            }
            catch (Exception ex)
            {
                //throw;
            }
        }

        private void changeView(string result)
        {
            btnSignIn.Visibility = Visibility.Collapsed;
            txtUserInfo.Text = result;
            string imgUrl = apiUrl + "me/picture?access_token=" + App.Current.Properties["access_token"];
            imgUser.Source = new BitmapImage(new Uri(imgUrl, UriKind.RelativeOrAbsolute));
            txtToken.Text += "access_token = " + App.Current.Properties["access_token"] + "\r\n\r\n";
        }

        private void btnSignIn_Click(object sender, RoutedEventArgs e)
        {
            BrowserWindow browser = new BrowserWindow();
            browser.Closed += new EventHandler(browser_Closed);
            browser.Show();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Properties.Clear();
            btnSignIn.Visibility = Visibility.Visible;
            txtToken.Text = "";
            imgUser.Source = null;
            txtUserInfo.Text = "";
        }


        void browser_Closed(object sender, EventArgs e)
        {
            getAccessToken();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }







        private void GetCalendars()
        {
            makeApiRequest2(apiUrl + "me/calendars?access_token=" + App.Current.Properties["access_token"]);
        }

        private void GetCalendar()
        {
            string id = "3ae2a03e43b87c56.b3eab7507ea5429d9015a169b8dfcd19";
            makeApiRequest2(apiUrl + string.Format("me/calendar.{0}?access_token={1}", id, App.Current.Properties["access_token"]));
        }

    }
}
