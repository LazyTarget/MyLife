namespace CalendarCore.Outlook
{
    public class AuthEventArgs : System.EventArgs
    {
        public AuthEventArgs(string url)
        {
            Url = url;
        }

        public bool Authorized { get; internal set; }

        public string Url { get; set; }
    }
}