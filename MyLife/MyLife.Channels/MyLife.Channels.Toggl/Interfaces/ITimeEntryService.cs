﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace Toggl.Interfaces
{
    public interface ITimeEntryService
    {
        /// <summary>
        /// https://www.toggl.com/public/api#get_time_entries
        /// </summary>
        /// <returns></returns>
        Task<List<TimeEntry>> ListRecent();

        Task<List<TimeEntry>> List();

        /// <summary>
        /// 
        /// https://www.toggl.com/public/api#get_time_entries_by_range
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        Task<List<TimeEntry>> List(QueryObjects.TimeEntryParams obj);

        /// <summary>
        /// 
        /// https://www.toggl.com/public/api#get_time_entry
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<TimeEntry> Get(long id);

        /// <summary>
        /// https://www.toggl.com/public/api#post_time_entries
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        Task<TimeEntry> Add(TimeEntry obj);

        /// <summary>
        /// https://www.toggl.com/public/api#put_time_entries
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        Task<TimeEntry> Edit(TimeEntry obj);

        /// <summary>
        /// 
        /// https://www.toggl.com/public/api#del_time_entries
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> Delete(long id);
    }
}