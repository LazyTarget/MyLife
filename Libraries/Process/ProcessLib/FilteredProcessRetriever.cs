using System;
using System.Collections.Generic;
using System.Linq;
using ProcessLib.Interfaces;

namespace ProcessLib
{
    public class FilteredProcessRetriever : IProcessRetriever
    {
        private readonly IProcessRetriever _processRetriever;
        private readonly Func<System.Diagnostics.Process, bool> _filter;

        public FilteredProcessRetriever(IProcessRetriever processRetriever)
        {
            if (processRetriever == null)
                throw new ArgumentNullException(nameof(processRetriever));
            _processRetriever = processRetriever;
        }

        public FilteredProcessRetriever(IProcessRetriever processRetriever, Func<System.Diagnostics.Process, bool> filter)
            : this(processRetriever)
        {
            _filter = filter;
        }


        public string MachineName { get { return _processRetriever.MachineName; } }


        public virtual System.Diagnostics.Process GetProcessById(int processId)
        {
            System.Diagnostics.Process res = System.Diagnostics.Process.GetProcessById(processId);
            return res;
        }

        public virtual IEnumerable<System.Diagnostics.Process> GetProcesses()
        {
            var enumerable = _processRetriever.GetProcesses();
            if (_filter != null)
                enumerable = enumerable.Where(_filter);
            return enumerable;
        }
    }
}