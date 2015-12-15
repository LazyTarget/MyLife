using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using System.Web.Http.OData;
using System.Web.Http.OData.Routing;
using ProcessLib.Data;
using ProcessLib.Models;

namespace MyLife.API.Server.Controllers
{
    /*
    The WebApiConfig class may require additional changes to add a route for this controller. Merge these statements into the Register method of the WebApiConfig class as applicable. Note that OData URLs are case sensitive.

    using System.Web.Http.OData.Builder;
    using System.Web.Http.OData.Extensions;
    using ProcessLib.Models;
    ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
    builder.EntitySet<ProcessTitle>("ProcessTitles");
    builder.EntitySet<Process>("Processes"); 
    config.Routes.MapODataServiceRoute("odata", "odata", builder.GetEdmModel());
    */
    public class ProcessTitlesController : ODataController
    {
        private ProcessDataContext db = new ProcessDataContext();

        // GET: odata/ProcessTitles
        [EnableQuery]
        public IQueryable<ProcessTitle> GetProcessTitles()
        {
            return db.ProcessTitles;
        }

        // GET: odata/ProcessTitles(5)
        [EnableQuery]
        public SingleResult<ProcessTitle> GetProcessTitle([FromODataUri] long key)
        {
            return SingleResult.Create(db.ProcessTitles.Where(processTitle => processTitle.ID == key));
        }

        // PUT: odata/ProcessTitles(5)
        public IHttpActionResult Put([FromODataUri] long key, Delta<ProcessTitle> patch)
        {
            var processTitle = patch.GetEntity();

            Validate(processTitle);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            processTitle = db.ProcessTitles.Find(key);
            if (processTitle == null)
            {
                return NotFound();
            }

            patch.Put(processTitle);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProcessTitleExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Updated(processTitle);
        }

        // POST: odata/ProcessTitles
        public IHttpActionResult Post(ProcessTitle processTitle)
        {
            Validate(processTitle);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var temp = processTitle.ID <= 0
                ? db.ProcessTitles
                    .OrderByDescending(x => x.StartTime)
                    .FirstOrDefault(x => x.ProcessID == processTitle.ProcessID && x.Title == processTitle.Title)
                : null;
            if (temp != null)
            {
                //var delta = new Delta<ProcessTitle>();
                //delta.Patch(processTitle);
                //return Put(temp.ID, delta);

                temp.StartTime = processTitle.StartTime;
                temp.EndTime = processTitle.EndTime;
                temp.Title = processTitle.Title;
                temp.ProcessID = processTitle.ProcessID;
                processTitle = temp;

                //return BadRequest("Invalid id");
            }
            else
            {
                db.ProcessTitles.Add(processTitle);
            }

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (ProcessTitleExists(processTitle.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return Created(processTitle);
        }

        // PATCH: odata/ProcessTitles(5)
        [AcceptVerbs("PATCH", "MERGE")]
        public IHttpActionResult Patch([FromODataUri] long key, Delta<ProcessTitle> patch)
        {
            var processTitle = patch.GetEntity();

            Validate(processTitle);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            processTitle = db.ProcessTitles.Find(key);
            if (processTitle == null)
            {
                return NotFound();
            }

            patch.Patch(processTitle);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProcessTitleExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Updated(processTitle);
        }

        // DELETE: odata/ProcessTitles(5)
        public IHttpActionResult Delete([FromODataUri] long key)
        {
            ProcessTitle processTitle = db.ProcessTitles.Find(key);
            if (processTitle == null)
            {
                return NotFound();
            }

            db.ProcessTitles.Remove(processTitle);
            db.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        // GET: odata/ProcessTitles(5)/Process
        [EnableQuery]
        public SingleResult<Process> GetProcess([FromODataUri] long key)
        {
            return SingleResult.Create(db.ProcessTitles.Where(m => m.ID == key).Select(m => m.Process));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ProcessTitleExists(long key)
        {
            return db.ProcessTitles.Any(e => e.ID == key);
        }
    }
}
