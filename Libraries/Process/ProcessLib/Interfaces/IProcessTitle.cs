using System;

namespace ProcessLib.Interfaces
{
    public interface IProcessTitle
    {
        long ID { get; }
        long ProcessID { get; }
        DateTime? StartTime { get; }
        DateTime? EndTime { get; }
        string Title { get; }

        void CopyFrom(IProcessTitle title);
    }
}
