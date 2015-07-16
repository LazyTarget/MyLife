using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Settings Flyout item template is documented at http://go.microsoft.com/fwlink/?LinkId=273769
using MyLife.App.Common;

namespace MyLife.App.Settings
{
    public sealed partial class UserSettingsFlyout : SettingsFlyout
    {
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private ApplicationData applicationData;
        private ApplicationDataContainer roamingSettings;


        public UserSettingsFlyout()
        {
            this.InitializeComponent();
            applicationData = ApplicationData.Current;
            roamingSettings = applicationData.RoamingSettings;
        }

        /// <summary>
        /// Gets the DefaultViewModel. This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        private void SettingsFlyout_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var authData = roamingSettings.GetContainerOrDefault("Auth");
            panLogout.Visibility = authData != null ? Visibility.Visible : Visibility.Collapsed;

            DefaultViewModel["Username"] = authData != null ? authData.Values["Username"] as string : null;
            //DefaultViewModel["Password"] = authData.Values["Password"] as string;
        }


        private void btnLogout_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            roamingSettings.DeleteContainer("Auth");
            applicationData.SignalDataChanged();


            var rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof (Login));     // transition?
        }



    }
}
