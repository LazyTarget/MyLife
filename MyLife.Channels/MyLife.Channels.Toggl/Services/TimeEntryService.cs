using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toggl.Interfaces;


namespace Toggl.Services
{
	using System.Net;

	public class TimeEntryService : ITimeEntryService
    {

        private IApiService ToggleSrv { get; set; }

        public TimeEntryService(string apiKey)
            : this(new ApiService(apiKey))
        {

        }
        
        public TimeEntryService(IApiService srv)
        {
            ToggleSrv = srv;
        }

        /// <summary>
        /// https://www.toggl.com/public/api#get_time_entries
        /// </summary>
        /// <returns></returns>
        public async Task<List<TimeEntry>> ListRecent()
        {
            throw new NotImplementedException();
        }

        public async Task<List<TimeEntry>> List()
        {
            return await List(new QueryObjects.TimeEntryParams());
        }
        /// <summary>
        /// 
        /// https://www.toggl.com/public/api#get_time_entries_by_range
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public async Task<List<TimeEntry>> List(QueryObjects.TimeEntryParams obj)
        {
            var response = await ToggleSrv.Get(ApiRoutes.TimeEntry.TimeEntriesUrl, obj.GetParameters());
            var entries = response
                        .GetData<List<TimeEntry>>()
                        .AsQueryable();
            
            if (obj.ProjectId.HasValue)
                entries = entries.Where(w => w.ProjectId == obj.ProjectId);

            return entries.Select(s => s).ToList();
        }

        /// <summary>
        /// 
        /// https://www.toggl.com/public/api#get_time_entry
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<TimeEntry> Get(long id)
        {
            var url = string.Format(ApiRoutes.TimeEntry.TimeEntryUrl, id);

            var response = await ToggleSrv.Get(url);
            var timeEntry = response.GetData<TimeEntry>();

            return timeEntry;
        }
        
        /// <summary>
        /// https://www.toggl.com/public/api#post_time_entries
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public async Task<TimeEntry> Add(TimeEntry obj)
        {
            var url = ApiRoutes.TimeEntry.TimeEntriesUrl;

            var response = await ToggleSrv.Post(url, obj.ToJson());
            var timeEntry = response.GetData<TimeEntry>();

            return timeEntry;
        }

        /// <summary>
        /// https://www.toggl.com/public/api#put_time_entries
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public async Task<TimeEntry> Edit(TimeEntry obj)
        {
            var url = string.Format(ApiRoutes.TimeEntry.TimeEntryUrl, obj.Id);

            var response = await ToggleSrv.Put(url, obj.ToJson());
            var timeEntry = response.GetData<TimeEntry>();

            return timeEntry;
        }

        /// <summary>
        /// 
        /// https://www.toggl.com/public/api#del_time_entries
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> Delete(long id)
        {
			var url = string.Format(ApiRoutes.TimeEntry.TimeEntryUrl, id);

            var rsp = await ToggleSrv.Delete(url);

            return rsp.StatusCode == HttpStatusCode.OK;
        }

		public async Task<bool> DeleteIfAny(long[] ids)
		{
			if (!ids.Any() || ids == null)
				return true;
			return await Delete(ids);
		}

		public async Task<bool> Delete(long[] ids)
		{
			if (!ids.Any() || ids == null)
				throw new ArgumentNullException("ids");

			var result = new Dictionary<long, bool>(ids.Length);
			foreach (var id in ids)
			{
			    var b = await Delete(id);
				result.Add(id, b);
			}

			return !result.ContainsValue(false);
		}       
    }
}
