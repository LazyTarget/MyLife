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
    builder.EntitySet<Process>("Process");
    builder.EntitySet<ProcessTitle>("ProcessTitles"); 
    config.Routes.MapODataServiceRoute("odata", "odata", builder.GetEdmModel());
    */
    public class ProcessController : ODataController
    {
        private readonly ProcessDataContext db = new ProcessDataContext();

        // GET: odata/Process
        [EnableQuery]
        public IQueryable<Process> GetProcess()
        {
            return db.Processes;
        }

        // GET: odata/Process(5)
        [EnableQuery]
        public SingleResult<Process> GetProcess([FromODataUri] long key)
        {
            return SingleResult.Create(db.Processes.Where(process => process.ID == key));
        }

        // PUT: odata/Process(5)
        public IHttpActionResult Put([FromODataUri] long key, Delta<Process> patch)
        {
            Validate(patch.GetEntity());

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Process process = db.Processes.Find(key);
            if (process == null)
            {
                return NotFound();
            }

            patch.Put(process);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProcessExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Updated(process);
        }

        // POST: odata/Process
        public IHttpActionResult Post(Process process)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Processes.Add(process);
            db.SaveChanges();

            return Created(process);
        }

        // PATCH: odata/Process(5)
        [AcceptVerbs("PATCH", "MERGE")]
        public IHttpActionResult Patch([FromODataUri] long key, Delta<Process> patch)
        {
            Validate(patch.GetEntity());

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Process process = db.Processes.Find(key);
            if (process == null)
            {
                return NotFound();
            }

            patch.Patch(process);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProcessExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Updated(process);
        }

        // DELETE: odata/Process(5)
        public IHttpActionResult Delete([FromODataUri] long key)
        {
            Process process = db.Processes.Find(key);
            if (process == null)
            {
                return NotFound();
            }

            db.Processes.Remove(process);
            db.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        // GET: odata/Process(5)/Title
        [EnableQuery]
        public SingleResult<ProcessTitle> GetTitle([FromODataUri] long key)
        {
            return SingleResult.Create(db.Processes.Where(m => m.ID == key).Select(m => m.Title));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ProcessExists(long key)
        {
            return db.Processes.Count(e => e.ID == key) > 0;
        }
    }
}
