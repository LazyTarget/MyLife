using System.Web.Http;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using ProcessLib.Models;

namespace MyLife.API.Server
{
    public static class ODataConfig
    {
        public static void Register(HttpConfiguration config)
        {
            //This has to be called before the following OData mapping, so also before WebApi mapping
            config.MapHttpAttributeRoutes();


            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();

            builder.EntitySet<Process>("process");
            builder.EntitySet<ProcessTitle>("processtitles");


            config.Routes.MapODataServiceRoute("ODataRoute", "api", builder.GetEdmModel());
        }
    }
}
