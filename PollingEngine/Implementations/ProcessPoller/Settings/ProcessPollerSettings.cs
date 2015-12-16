using System;
using System.Diagnostics;

namespace ProcessPoller
{
    public class ProcessPollerSettings : IProcessPollerSettings
    {
        public ProcessPollerSettings()
        {
            //DataApiBaseUrl = "";

            MachineName = Environment.MachineName;

            ProcessFilter = process =>
            {
                var res = true;
                try
                {
                    if (process.Id <= 0)
                        res = false;
#if DEBUG
                    else if (string.IsNullOrWhiteSpace(process.MainWindowTitle))
                        res = false;
#endif

                    if (process.ProcessName.ToLower() == "explorer")
                        res = true;
                }
                catch (Exception ex)
                {

                }
                finally
                {

                }
                return res;
            };
        }

        public string DataApiBaseUrl { get; set; }
        public string MachineName { get; set; }
        public Func<Process, bool> ProcessFilter { get; set; }
    }
}