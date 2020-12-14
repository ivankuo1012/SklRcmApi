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

                //foreach (string file in httpRequest.Files)
                //{
                //    var postedFile = httpRequest.Files[file]
                //    //Debug.Write(httpRequest.Files[file]);
                //    var filePath = HttpContext.Current.Server.MapPath("~/FileUploads/" + postedFile.FileName);

                //    postedFile.SaveAs(filePath);
                //    docfiles.Add(filePath);
                //}

                //Dictionary<string, dynamic> importDB = readExcelToDb(filePath);
                resultAry.Add("file_name", postedFile.FileName);
                resultAry.Add("file_path", filePath);
                FileInfo fi = new FileInfo(filePath);
                resultAry.Add("file_size", fi.Length);

                //resultAry.Add("import_result", importDB);


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
