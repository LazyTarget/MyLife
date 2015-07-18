using SteamLib.Models;

namespace SteamLib
{
    public class SteamManager : ISteamManager
    {
        public SteamManager(string connString)
        {
            ActivityManager = new SteamActivityManager(connString);
            ReportManager = new SteamReportManager(connString, ActivityManager);
        }

        public ISteamActivityManager ActivityManager { get; private set; }
        public ISteamReportManager ReportManager { get; private set; }
    }
}
