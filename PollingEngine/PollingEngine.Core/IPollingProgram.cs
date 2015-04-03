namespace PollingEngine.Core
{
    public interface IPollingProgram
    {
        void OnStarting(PollingContext context);
        void OnInterval(PollingContext context);
        void OnStopping(PollingContext context);
    }
}
