namespace ProcessPoller
{
    public class ProcessEntity
    {
        public long ID { get; set; }

        //public IProcessRunInfo ProcessInfo { get; set; }

        public ProcessLib.Interfaces.IProcess ProcessInfo { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}] {1}", ID, ProcessInfo != null ? ProcessInfo.ToString() : base.ToString());
        }
    }
}