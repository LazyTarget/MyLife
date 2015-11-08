using System.Web.Http;

namespace MyLife.API.Server
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            //MapHttpRoute for controllers inheriting ApiController
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
