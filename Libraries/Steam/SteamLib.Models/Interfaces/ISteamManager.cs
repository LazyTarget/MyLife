namespace SteamLib.Models
{
    public interface ISteamManager
    {
        ISteamActivityManager ActivityManager { get; }

        ISteamReportManager ReportManager { get; }
    }
}
