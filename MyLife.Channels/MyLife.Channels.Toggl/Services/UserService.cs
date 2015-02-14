using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Toggl.Extensions;
using Toggl.Interfaces;

namespace Toggl.Services
{
    public class UserService : IUserService
    {
        
        private IApiService ToggleSrv { get; set; }


        public UserService(string apiKey)
            : this(new ApiService(apiKey))
        {

        }

        public UserService(IApiService srv)
        {
            ToggleSrv = srv;
        }

        
        public async Task<User> GetCurrent()
        {
            var url = ApiRoutes.User.CurrentUrl;

            var response = await ToggleSrv.Get(url);
            var obj = response.GetData<User>();

            return obj;
        }

        public async Task<UserExtended> GetCurrentExtended()
        {
            var url = ApiRoutes.User.CurrentExtendedUrl;

            var response = await ToggleSrv.Get(url);
            var obj = response.GetData<UserExtended>();

            return obj;
        }

        public async Task<UserExtended> GetCurrentChanged(DateTime since)
        {
            var url = string.Format(ApiRoutes.User.CurrentSinceUrl, since.ToUnixTime());

            var response = await ToggleSrv.Get(url);
            var obj = response.GetData<UserExtended>();

            return obj;
        }

        public async Task<User> Edit(User u)
        {
            var url = string.Format(ApiRoutes.User.EditUrl);
            var data = u.ToJson();

            var response = await ToggleSrv.Put(url, data);
            u = response.GetData<User>();
            
            return u;
        }

        public async Task<string> ResetApiToken()
        {
            throw new NotImplementedException();
        }

        public async Task<List<User>> GetForWorkspace(int id)
        {
            var url = string.Format(ApiRoutes.Workspace.ListWorkspaceUsersUrl, id);
            var response = await ToggleSrv.Get(url);
            return response.GetData<List<User>>();
        }

        public async Task<User> Add(User u)
        {
            var url = string.Format(ApiRoutes.User.AddUrl);
            var data = u.ToJson();

            var response = await ToggleSrv.Post(url, data);
            u = response.GetData<User>();

            return u;
        }
    }
}
