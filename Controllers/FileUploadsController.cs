using FootballManager.Models;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace FootballManager.Controllers
{
    public class FileUploadsController : Controller
    {
        private TeamContext db = new TeamContext();

        public ActionResult Index()
        {
            return View(db.UploadedFiles.ToList());
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "No file uploaded.");

            var allowedTypes = new[] { "image/jpeg", "image/png", "application/pdf" };
            if (!allowedTypes.Contains(file.ContentType))
                return new HttpStatusCodeResult(HttpStatusCode.UnsupportedMediaType, "Invalid file type.");

            if (file.ContentLength > 5 * 1024 * 1024)
                return new HttpStatusCodeResult(HttpStatusCode.RequestEntityTooLarge, "File too large.");

            var fileName = Path.GetFileName(file.FileName);
            var filePath = Path.Combine(Server.MapPath("~/UploadedFiles"), fileName);
            file.SaveAs(filePath);

            var fileRecord = new FileUpload
            {
                FileName = fileName,
                FilePath = "/UploadedFiles/" + fileName,
                UploadDate = DateTime.Now
            };

            db.UploadedFiles.Add(fileRecord);
            db.SaveChanges();


            var updatedList = db.UploadedFiles.OrderByDescending(f => f.UploadDate).ToList();
            return PartialView("_UploadedFilesList", updatedList); // ✅ Partial only!

        }



        public ActionResult Download(int id)
        {
            var file = db.UploadedFiles.Find(id);
            if (file == null) return HttpNotFound();

            var fullPath = Server.MapPath(file.FilePath);
            return File(fullPath, "application/octet-stream", file.FileName);
        }

        [HttpPost]
        public async Task<ActionResult> Delete(int id)
        {
            System.Diagnostics.Debug.WriteLine("Delete called for file ID: " + id); // 👈 for debug

           // if (Session["UserId"] == null)
               // return Json(new { success = false, message = "Unauthorized access." });

          //  if (Session["Role"]?.ToString() != "Admin")
              //  return Json(new { success = false, message = "Insufficient permissions." });

            var file = await db.UploadedFiles.FindAsync(id);
            if (file == null)
                return Json(new { success = false, message = "File not found." });

            var fullPath = Server.MapPath(file.FilePath);
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);

            db.UploadedFiles.Remove(file);
            await db.SaveChangesAsync();

            return Json(new { success = true });
        }


    }
}
