using SklRcmApi.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace SklRcmApi.Controllers
{
    public class UploadFileController : ApiController
    {
        
        public HttpResponseMessage Upload()
        {
            var db = new Entities();
        HttpResponseMessage result;
            var httpRequest = HttpContext.Current.Request;

            Debug.WriteLine("upload start");
            var resultAry = new Dictionary<string, dynamic>();

            if (httpRequest.Files.Count > 0)
            {
                var docfiles = new List<string>();
                var postedFile = httpRequest.Files[0];
                Debug.WriteLine(httpRequest.Files[0].FileName);
                var filePath = HttpContext.Current.Server.MapPath("~/FileUploads/" + postedFile.FileName);
                postedFile.SaveAs(filePath);

                resultAry.Add("file_name", postedFile.FileName);
                resultAry.Add("file_path", filePath);
                FileInfo fi = new FileInfo(filePath);
                resultAry.Add("file_size", fi.Length);

                //resultAry.Add("import_result", importDB);
                Debug.WriteLine(httpRequest["up_user"]);
                upload upload = new upload();
                upload.up_user = httpRequest["up_user"];
                upload.up_message = httpRequest["up_message"];
                upload.up_path = Path.GetDirectoryName(filePath);
                upload.up_filename = postedFile.FileName;
                upload.up_date = DateTime.Now;
                upload.up_size = fi.Length;
                db.upload.Add(upload);
                db.SaveChanges();
                result = Request.CreateResponse(HttpStatusCode.Created, resultAry);


            }
            else
            {

                result = Request.CreateResponse(HttpStatusCode.BadRequest);

            }
            return result;
        }
    }
}
