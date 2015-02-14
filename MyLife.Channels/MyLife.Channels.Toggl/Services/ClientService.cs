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
	using System.Resources;

	public class ClientService : IClientService
    {
        private static Dictionary<int, Client> cachedClients;

		private void EnsureCacheLoaded()
		{
			if (cachedClients == null)
				List();
		}

		public IApiService ToggleSrv { get; set; }
		
        public ClientService(string apiKey)
            : this(new ApiService(apiKey))
        {

        }

		public ClientService(IApiService srv)
        {
            ToggleSrv = srv;
        }

        /// <summary>
        /// https://github.com/toggl/toggl_api_docs/blob/master/chapters/clients.md#get-clients-visible-to-user
        /// </summary>
        /// <returns></returns>
        public async Task<List<Client>> List(bool includeDeleted = false)
        {
            var response = await ToggleSrv.Get(ApiRoutes.Client.ClientsUrl);
	        var result = response.GetData<List<Client>>();

	        cachedClients = result.ToDictionary(client => client.Id.Value, client => client);

			return includeDeleted 
				? result
				: result.Where(client => client.DeletedAt == null).ToList();
        }

        public async Task<Client> Get(int id)
        {
	        if (cachedClients != null && cachedClients.ContainsKey(id))
		        return cachedClients[id];

			var url = string.Format(ApiRoutes.Client.ClientUrl, id);
            var response = await ToggleSrv.Get(url);
            return response.GetData<Client>();
            
        }

	    public Client GetByName(string name)
	    {
		    EnsureCacheLoaded();

		    return cachedClients
				.Values
				.Single(client => client.Name == name && client.DeletedAt == null);
	    }

        /// <summary>
        /// https://github.com/toggl/toggl_api_docs/blob/master/chapters/clients.md#create-a-client
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public async Task<Client> Add(Client obj)
        {
	        cachedClients = null;
            var url = ApiRoutes.Client.ClientsUrl;
            var response = await ToggleSrv.Post(url, obj.ToJson());
            return response.GetData<Client>();

        }
        
        /// <summary>
        /// 
        /// https://www.toggl.com/public/api#put_clients
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public async Task<Client> Edit(Client obj)
        {
	        cachedClients = null;
            var url = string.Format(ApiRoutes.Client.ClientUrl, obj.Id);
            var response = await ToggleSrv.Put(url, obj.ToJson());
            return response.GetData<Client>();

        }

        /// <summary>
        /// 
        /// https://www.toggl.com/public/api#del_clients
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> Delete(int id)
        {
	        cachedClients = null;
            var url = string.Format(ApiRoutes.Client.ClientUrl, id);
            var res = await ToggleSrv.Delete(url);
            return (res.StatusCode==HttpStatusCode.OK);
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

		    cachedClients = null;

		    var result = new Dictionary<int, bool>(ids.Length);
		    foreach (var id in ids)
		    {
		        var b = await Delete(id);
			    result.Add(id, b);
		    }

		    return !result.ContainsValue(false);
	    }       
    }
}
