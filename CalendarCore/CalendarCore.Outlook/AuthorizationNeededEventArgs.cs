namespace CalendarCore.Outlook
{
    public class AuthorizationNeededEventArgs : System.EventArgs
    {
        public AuthorizationNeededEventArgs(string loginUrl)
        {
            LoginUrl = loginUrl;
        }

        public System.Action<AuthEventArgs> OnUrlLoaded { get; internal set; }

        public string LoginUrl { get; private set; }
    }
}