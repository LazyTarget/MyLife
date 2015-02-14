using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Toggl.Interfaces;
using global::Toggl.Extensions;
using global::Toggl.QueryObjects;

namespace Toggl.Services
{
	public class TaskService : ITaskService
    {
        private readonly string TogglTasksUrl = ApiRoutes.Task.TogglTasksUrl;

        private IApiService ToggleSrv { get; set; }

        public TaskService(string apiKey)
            : this(new ApiService(apiKey))
        {

        }

        public TaskService(IApiService srv)
        {
            ToggleSrv = srv;
        }

        public async Task<Task> Get(int id)
        {
	        var url = string.Format(ApiRoutes.Task.TogglTasksGet, id);
            var response = await ToggleSrv.Get(url);
			return response.GetData<Task>();
        }
       
        /// <summary>
        /// 
        /// https://www.toggl.com/public/api#post_tasks
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public async Task<Task> Add(Task t)
        {
            var response = await ToggleSrv.Post(TogglTasksUrl, t.ToJson());
            return response.GetData<Task>();
        }

        /// <summary>
        /// 
        /// https://www.toggl.com/public/api#put_tasks
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public async Task<Task> Edit(Task t)
        {
			var url = string.Format(ApiRoutes.Task.TogglTasksGet, t.Id);
            var response = await ToggleSrv.Put(url, t.ToJson());
			return response.GetData<Task>();
        }

        /// <summary>
        /// 
        /// https://www.toggl.com/public/api#del_tasks
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<bool> Delete(int id)
        {
			var url = string.Format(ApiRoutes.Task.TogglTasksGet, id);

			var rsp = await ToggleSrv.Delete(url);

			return rsp.StatusCode == HttpStatusCode.OK;            
        }
		
		public async Task<bool> DeleteIfAny(int[] ids)
		{
			if (!ids.Any() || ids == null)
				return true;
			return await Delete(ids);
		}

	    public async Task<bool> Delete(int[] ids)
	    {
			if (!ids.Any() || ids == null)
				throw new ArgumentNullException("ids");

		    var url = string.Format(
			    ApiRoutes.Task.TogglTasksGet,
			    string.Join(",", ids.Select(id => id.ToString()).ToArray()));

		    var rsp = await ToggleSrv.Delete(url);

		    return rsp.StatusCode == HttpStatusCode.OK;
	    }

		public async Task<Task> ForProjectByName(Project project, string taskName)
		{
			if (!project.Id.HasValue)
				throw new InvalidOperationException("Project Id not set");

			return await ForProjectByName(project.Id.Value, taskName);
		}

		public async Task<Task> ForProjectByName(int projectId, string taskName)
		{
			var projectTasks = await ForProject(projectId);
			return projectTasks.Single(task => task.Name == taskName);
		}

		public async Task<Task> TryGetForProjectByName(int projectId, string taskName)
		{
			var projectTasks = await ForProject(projectId);
			return projectTasks.SingleOrDefault(task => task.Name == taskName);
		}

        public async Task<List<Task>> ForProject(int id)
        {
            var url = string.Format(ApiRoutes.Project.ProjectTasksUrl, id);
            var response = await ToggleSrv.Get(url);
            return response.GetData<List<Task>>();
        }

		public async Task<List<Task>> ForProject(Project project)
		{
			if (!project.Id.HasValue)
				throw new InvalidOperationException("Project Id not set");

			return await ForProject(project.Id.Value);
		}

		public void Merge(Task masterTask, Task slaveTask, int workspaceId, string userAgent = "TogglAPI.Net")
		{
			if (!masterTask.Id.HasValue)
				throw new InvalidOperationException("Master task Id not set");

			if (!slaveTask.Id.HasValue)
				throw new InvalidOperationException("Slave task Id not set");

			Merge(masterTask.Id.Value, slaveTask.Id.Value, workspaceId, userAgent);
		}

		public async void Merge(int masterTaskId, int slaveTaskId, int workspaceId, string userAgent = "TogglAPI.Net")
	    {
		    var reportService = new ReportService(this.ToggleSrv);
		    var timeEntryService = new TimeEntryService(this.ToggleSrv);

			var reportParams = new DetailedReportParams()
								{
									UserAgent = userAgent,
									WorkspaceId = workspaceId,
									TaskIds = slaveTaskId.ToString(),
									Since = DateTime.Now.AddYears(-1).ToIsoDateStr()
								};

		    var result = await reportService.Detailed(reportParams);

			if (result.TotalCount > result.PerPage)
				result = await reportService.FullDetailedReport(reportParams);

		    foreach (var reportTimeEntry in result.Data)
		    {
			    var timeEntry = await timeEntryService.Get(reportTimeEntry.Id.Value);
				timeEntry.TaskId = masterTaskId;
			    try
			    {
				    var editedTimeEntry = await timeEntryService.Edit(timeEntry);
			    }
			    catch (Exception ex)
			    {
				    var res = ex.Data;
			    }			    
		    }

		    if (!await Delete(slaveTaskId))
				throw new InvalidOperationException(string.Format("Can't delte task #{0}", slaveTaskId));
	    }

		public async void Merge(int masterTaskId, int[] slaveTasksIds, int workspaceId, string userAgent = "TogglAPI.Net")
		{
			var reportService = new ReportService(this.ToggleSrv);
			var timeEntryService = new TimeEntryService(this.ToggleSrv);

			var reportParams = new DetailedReportParams()
			{
				UserAgent = userAgent,
				WorkspaceId = workspaceId,
				TaskIds = string.Join(",", slaveTasksIds.Select(id => id.ToString())),
				Since = DateTime.Now.AddYears(-1).ToIsoDateStr()
			};

			var result = await reportService.Detailed(reportParams);

			if (result.TotalCount > result.PerPage)
				result = await reportService.FullDetailedReport(reportParams);

			foreach (var reportTimeEntry in result.Data)
			{
				var timeEntry = await timeEntryService.Get(reportTimeEntry.Id.Value);
				timeEntry.TaskId = masterTaskId;
				var editedTimeEntry = await timeEntryService.Edit(timeEntry);
				if (editedTimeEntry == null)
					throw new ArgumentNullException(string.Format("Can't edit timeEntry #{0}", reportTimeEntry.Id));
			}

			foreach (var slaveTaskId in slaveTasksIds)
			{
				if (!await Delete(slaveTaskId))
					throw new InvalidOperationException(string.Format("Can't delte task #{0}", slaveTaskId));	
			}			
		}
    }
}
