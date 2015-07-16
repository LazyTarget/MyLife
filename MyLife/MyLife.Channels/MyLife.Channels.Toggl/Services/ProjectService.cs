using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Toggl.Interfaces;

namespace Toggl.Services
{
    public class ProjectService : IProjectService
    {
        private readonly string ProjectsUrl = ApiRoutes.Project.ProjectsUrl;
        

        private IApiService ToggleSrv { get; set; }


        public ProjectService(string apiKey)
            : this(new ApiService(apiKey))
        {

        }

		public ProjectService(IApiService srv)
        {
            ToggleSrv = srv;
        }

        /// <summary>
        /// 
        /// https://www.toggl.com/public/api#get_projects
        /// </summary>
        /// <returns></returns>
        public async Task<List<Project>> List()
        {
            var lstProj = new List<Project>();
            var response = await ToggleSrv.Get(ApiRoutes.Workspace.ListWorkspaceUrl);
            var lstWrkSpc = response.GetData<List<Workspace>>();
            lstWrkSpc.ForEach(async e =>
                {
                    var projs = await ForWorkspace(e.Id.Value);
                    lstProj.AddRange(projs);
                });
            return lstProj;
        }
        
        public async Task<List<Project>> ForWorkspace (int id)
        {
            var url = string.Format(ApiRoutes.Workspace.ListWorkspaceProjectsUrl, id);
            var response = await ToggleSrv.Get(url);
            return response.GetData<List<Project>>();
        }

	    public async Task<List<Project>> ForClient(Client client)
	    {
		    if (!client.Id.HasValue)
				throw new InvalidOperationException("Client Id not set");
		    
			return await ForClient(client.Id.Value);
	    }

        public async Task<List<Project>> ForClient(int id)
        {
            var url = string.Format(ApiRoutes.Client.ClientProjectsUrl, id);
            var response = await ToggleSrv.Get(url);
            return response.GetData<List<Project>>();
        }

        public async Task<Project> Get(int id)
        {
            var l = await List();
            return l.Where(w => w.Id == id).FirstOrDefault();
        }

        public async Task<Project> Add(Project project)
        {
            var response = await ToggleSrv.Post(ProjectsUrl, project.ToJson());
            return response.GetData<Project>();
        }

	    public async Task<bool> Delete(int id)
	    {
		    var url = string.Format(ApiRoutes.Project.DetailUrl, id);
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
				ApiRoutes.Project.ProjectsBulkDeleteUrl,
				string.Join(",", ids.Select(id => id.ToString()).ToArray()));

			var rsp = await ToggleSrv.Delete(url);
			return rsp.StatusCode == HttpStatusCode.OK;
		}    
       
    }
}
