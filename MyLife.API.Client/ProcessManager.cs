using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Simple.OData.Client;

namespace MyLife.API.Client
{
    public class ProcessManager
    {
        private readonly IODataClient _client;

        public ProcessManager(IODataClient odataClient)
        {
            _client = odataClient;
        }


        public async Task<ProcessLib.Models.Process> UpdateProcess(ProcessLib.Models.Process process)
        {
            try
            {
                var t = process.Titles?.OrderBy(x => x.StartTime).ThenBy(x => x.ID).ToList();
                var titles = process.Titles?.OrderByDescending(x => x.StartTime)
                    .ThenByDescending(x => x.ID)
                    .ToList();
                if (process.ID > 0)
                {
                    var task = _client.For<ProcessLib.Models.Process>()
                        .Set(process)
                        .Key(process.ID)
                        .UpdateEntryAsync(true);
                    process = await task;
                }
                else
                {
                    var task = _client.For<ProcessLib.Models.Process>()
                        .Set(process)
                        .InsertEntryAsync(true);
                    process = await task;
                }

                
                var newTitles = new List<ProcessLib.Models.ProcessTitle>();
                for (var i = 0; titles != null && i < titles.Count; i++)
                {
                    var processTitle = titles.ElementAt(i);
                    if (processTitle.ProcessID <= 0)
                        processTitle.ProcessID = process.ID;
                    if (processTitle.ProcessID != process.ID)
                        continue;
                    if (i == 0 && process.HasExited && !processTitle.EndTime.HasValue)
                        processTitle.EndTime = process.ExitTime;
                    var prevTitle = titles.ElementAtOrDefault(i + 1);
                    if (prevTitle != null && !prevTitle.EndTime.HasValue)
                    {
                        prevTitle.EndTime = processTitle.StartTime;
                    }
                    processTitle = await UpdateProcessTitle(processTitle);
                    //processTitle.Process = process;
                    newTitles.Add(processTitle);
                }
                //process.Titles = titles.OrderBy(x => x.StartTime).ThenBy(x => x.ID).ToList();
                process.Titles = newTitles.OrderBy(x => x.StartTime).ThenBy(x => x.ID).ToList();
            
            }
            catch (Exception ex)
            {
                throw;
            }
            return process;
        }


        public async Task<ProcessLib.Models.ProcessTitle> UpdateProcessTitle(ProcessLib.Models.ProcessTitle processTitle)
        {
            try
            {
                if (processTitle == null)
                    throw new ArgumentNullException(nameof(processTitle));
                if (processTitle.ProcessID <= 0)
                    throw new ArgumentException("Invalid process id", nameof(processTitle));

                if (processTitle.ID > 0)
                {
                    var task = _client.For<ProcessLib.Models.ProcessTitle>()
                        .Set(processTitle)
                        .Key(processTitle.ID)
                        .UpdateEntryAsync(true);
                    processTitle = await task;
                }
                else
                {
                    var task = _client.For<ProcessLib.Models.ProcessTitle>()
                        .Set(processTitle)
                        .InsertEntryAsync(true);
                    processTitle = await task;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return processTitle;
        }

    }
}
