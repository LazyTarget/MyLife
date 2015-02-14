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
    /// <summary>
    /// https://github.com/toggl/toggl_api_docs/blob/master/chapters/workspaces.md#workspaces
    /// </summary>
    public class WorkspaceService : IWorkspaceService
    {
		private IApiService ToggleSrv { get; set; }

        public WorkspaceService(string apiKey)
            : this(new ApiService(apiKey))
        {

        }

        public WorkspaceService(IApiService srv)
        {
            ToggleSrv = srv;
        }

        public async Task<List<Workspace>> List()
        {
            var response = await ToggleSrv.Get(ApiRoutes.Workspace.ListWorkspaceUrl);
            return response.GetData<List<Workspace>>();
        }
        
        public async Task<List<User>> Users(int workspaceId)
        {
            var url = string.Format(ApiRoutes.Workspace.ListWorkspaceUsersUrl, workspaceId);
            var response = await ToggleSrv.Get(url);
            return response.GetData<List<User>>();
        }

       
        public async Task<List<Client>> Clients(int workspaceId)
        {
            var url = string.Format(ApiRoutes.Workspace.ListWorkspaceClientsUrl, workspaceId);
            var response = await ToggleSrv.Get(url);
            return response.GetData<List<Client>>();
        }

        public async Task<List<Project>> Projects(int workspaceId)
        {
            var url = string.Format(ApiRoutes.Workspace.ListWorkspaceProjectsUrl, workspaceId);
            var response = await ToggleSrv.Get(url);
            return response.GetData<List<Project>>();
        }


        public async Task<List<Task>> Tasks(int workspaceId)
        {
            var url = string.Format(ApiRoutes.Workspace.ListWorkspaceTasksUrl, workspaceId);
            var response = await ToggleSrv.Get(url);
            return response.GetData<List<Task>>();
        }

        public async Task<List<Tag>> Tags(int workspaceId)
        {
            var url = string.Format(ApiRoutes.Workspace.ListWorkspaceTagsUrl, workspaceId);
            var response = await ToggleSrv.Get(url);
            return response.GetData<List<Tag>>();
        }
        


    }
}
