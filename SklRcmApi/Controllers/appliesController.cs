using System;
using System.Collections;
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
using Newtonsoft.Json;
using SklRcmApi.Models;

namespace SklRcmApi.Controllers
{
    public class appliesController : ApiController
    {
        private Entities db = new Entities();
        
        public class ApplyUpload
        {
            public string app_upload { get; set; }
        }
        
        // GET: api/applies
        public IQueryable<apply> Getapply(string up_user = "")
        {
            return db.apply.Where(x => x.app_user.Equals(up_user)); 
        }
        public List<apply> GetapplyApprove(bool? app_approve_check=false, string app_approve_user = "", int? app_approve = 2 )
        {
            Debug.WriteLine(app_approve_check);
            var result = new List<apply>();
            IQueryable<apply> data=db.apply ; 

            if (app_approve_user != "")
            {
                data =data.Where(x => x.app_approve_user.Equals(app_approve_user));
            }
            if (app_approve_check != false)
            {
               data= data.Where(x => x.app_approve_check==true);
            }
            if (app_approve != null)
            {
              data=  data.Where(x => x.app_approve == app_approve);
            }
            //result = data.ToList();
            return data.ToList();
        }
        /*
        [System.Web.Http.HttpGet]
        public IHttpActionResult Getapplies(string up_user = "")
        {
           
            var data = db.apply.Where(x => x.app_user.Equals(up_user));
           
            var  result = data.ToList();

            for (var i = 0; i < result.Count; i++)
            {
                var app_id = result[i].app_id;
                var uploadEntity = db.upload.Where(x => x.up_apply == app_id);
                var uploadData = uploadEntity.ToList();


           

                var appUpload = new List<string>();
                if (uploadData.Count != 0)
                {
                    for(var j = 0; j < uploadData.Count; j++)
                    {
                        //if (j != 0)
                        //{
                        //    result[i].app_upload += ",";
                        //}
                        //result[i].app_upload += uploadData[j].up_filename;
                       
                        appUpload.Add(uploadData[j].up_filename);

                    }

                }
                result[i].app_upload = JsonConvert.SerializeObject(appUpload);





            }
            return Ok(result);
        }
        */
        // GET: api/applies/5
        [ResponseType(typeof(apply))]
        public IHttpActionResult Getapply(int id)
        {
            apply apply = db.apply.Find(id);
            if (apply == null)
            {
                return NotFound();
            }    

            return Ok(apply);
        }

        // PUT: api/applies/5
        [ResponseType(typeof(void))]
        public IHttpActionResult Putapply(int id, apply apply)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != apply.app_id)
            {
                return BadRequest();
            }

            db.Entry(apply).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!applyExists(id))
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

        // POST: api/applies
        [ResponseType(typeof(apply))]
        public IHttpActionResult Postapply(apply apply)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.apply.Add(apply);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = apply.app_id }, apply);
        }

        // DELETE: api/applies/5
        [ResponseType(typeof(apply))]
        public IHttpActionResult Deleteapply(int id)
        {
            apply apply = db.apply.Find(id);
            if (apply == null)
            {
                return NotFound();
            }

            db.apply.Remove(apply);
            db.SaveChanges();

            return Ok(apply);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool applyExists(int id)
        {
            return db.apply.Count(e => e.app_id == id) > 0;
        }
    }
}