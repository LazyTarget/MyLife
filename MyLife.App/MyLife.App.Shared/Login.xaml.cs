using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using MyLife.App.Common;
using MyLife.App.Data;
using MyLife.Models;

namespace MyLife.App
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class Login : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private ApplicationData applicationData;
        private ApplicationDataContainer roamingSettings;


        /// <summary>
        /// Gets the NavigationHelper used to aid in navigation and process lifetime management.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the DefaultViewModel. This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        public Login()
        {
            InitializeComponent();
            navigationHelper = new NavigationHelper(this);
            navigationHelper.LoadState += this.NavigationHelper_LoadState;
            navigationHelper.SaveState += this.NavigationHelper_SaveState;
            applicationData = ApplicationData.Current;
            roamingSettings = applicationData.RoamingSettings;
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            var authData = roamingSettings.GetContainerOrDefault("Auth");
            var username = (e.PageState != null ? e.PageState["Username"] as string : null) ?? (authData != null ? authData.Values["Username"] as string : null);
            var password = (e.PageState != null ? e.PageState["Password"] as string : null) ?? (authData != null ? authData.Values["Password"] as string : null);
            DefaultViewModel["Username"] = username;
            DefaultViewModel["Password"] = password;
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private async void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            e.PageState["Username"] = DefaultViewModel["Username"];
            e.PageState["Password"] = DefaultViewModel["Password"];
        }

        
        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="Common.NavigationHelper.LoadState"/>
        /// and <see cref="Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion


        public static async Task<User> AuthenticateUserFromRoamingSettingsAsync()
        {
            var applicationData = ApplicationData.Current;
            var roamingSettings = applicationData.RoamingSettings;
            var authData = roamingSettings.GetContainerOrDefault("Auth");
            var username = authData != null ? authData.Values["Username"] as string : null;
            var password = authData != null ? authData.Values["Password"] as string : null;
            var user = await MyLifeDataSource.AuthenticateUserAsync(username, password);
            return user;
        }


        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            var username = DefaultViewModel["Username"] as string;
            var password = DefaultViewModel["Password"] as string;

            var user = await MyLifeDataSource.AuthenticateUserAsync(username, password);
            if (user != null)
            {
                var authData = roamingSettings.CreateContainer("Auth", ApplicationDataCreateDisposition.Always);
                authData.Values["Username"] = username;
                authData.Values["Password"] = password;
                applicationData.SignalDataChanged();

                Frame.Navigate(typeof (FeedPage));
            }

        }
    }
}