using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using SklRcmApi.Models;

namespace SklRcmApi.Controllers
{
    public class uploadsController : ApiController
    {
        private Entities db = new Entities();

        // GET: api/uploads
        public IQueryable<upload> Getupload(string up_user="")
        {
            Debug.WriteLine("user/r/n");
            Debug.WriteLine(up_user);
            //upload upload = db.upload.Where(x=>x.up_user.Contains(user));
            return db.upload.Where(x => x.up_user.Equals(up_user) && x.up_apply.Equals(null));
        }
        public IQueryable<upload> GetUploadsApply(int up_apply)
        {
            return db.upload.Where(x => x.up_apply==up_apply);
        }
        // GET: api/uploads/5
        [ResponseType(typeof(upload))]
        public IHttpActionResult Getupload(int id)
        {
            upload upload = db.upload.Find(id);
            if (upload == null)
            {
                return NotFound();
            }

            return Ok(upload);
        }

        // PUT: api/uploads/5
        [ResponseType(typeof(void))]
        public IHttpActionResult Putupload(int id, upload upload)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != upload.up_id)
            {
                return BadRequest();
            }

            db.Entry(upload).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!uploadExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/uploads
        [ResponseType(typeof(upload))]
        public IHttpActionResult Postupload(upload upload)
        {
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.upload.Add(upload);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = upload.up_id }, upload);
        }

        // DELETE: api/uploads/5
        [ResponseType(typeof(upload))]
        public IHttpActionResult Deleteupload(int id)
        {
            upload upload = db.upload.Find(id);
            if (upload == null)
            {
                return NotFound();
            }

            db.upload.Remove(upload);
            db.SaveChanges();

            return Ok(upload);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool uploadExists(int id)
        {
            return db.upload.Count(e => e.up_id == id) > 0;
        }
    }
}