using System;
using ProcessLib.Interfaces;

namespace ProcessLib.Models
{
    public class ProcessTitle : IProcessTitle
    {
        public long ID { get; set; }
        public long ProcessID { get; set; }
        public virtual Process Process { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Title { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}