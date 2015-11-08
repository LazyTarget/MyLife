using System;

namespace ProcessLib.Interfaces
{
    public interface IProcessTitle
    {
        long ProcessID { get; }
        DateTime StartTime { get; }
        DateTime EndTime { get; }
        string Title { get; }
    }
}
