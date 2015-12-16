using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using ProcessLib.Data;

namespace MyLife.API.Server
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            //Database.SetInitializer(new MigrateDatabaseToLatestVersion<ProcessDataContext, ProcessLib.Migrations.Configuration>());
            //var ctx = new ProcessDataContext();
            //ctx.Database.Initialize(true);


            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(config =>
            {
                ODataConfig.Register(config);
                WebApiConfig.Register(config);
            });
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
