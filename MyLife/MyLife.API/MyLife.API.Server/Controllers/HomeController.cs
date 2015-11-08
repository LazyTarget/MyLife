using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.OData;
using System.Web.Mvc;
using ProcessLib.Models;

namespace MyLife.API.Server.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        public ActionResult Test()
        {
            try
            {
                var proc = new Process
                {
                    FileName = "QWERTY.exe",
                    Title = new ProcessTitle
                    {
                        Title = "test",
                    },
                };

                var sut = new ProcessController();
                var res = sut.Post(proc);

                var t = sut.GetProcess(proc.ID);
                proc = t.Queryable.FirstOrDefault();
                
                //proc.TitleID = 0;
                proc.Title = new ProcessTitle
                {
                    //ID = 0,
                    //ProcessID = proc.ID,
                    Title = "My title #2",
                };
                
            }
            catch (Exception ex)
            {
                
            }

            return RedirectToAction("Index");
        }
    }
}
