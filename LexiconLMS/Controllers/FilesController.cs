using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LexiconLMS.Models;

namespace LexiconLMS.Controllers
{
    public class FilesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Files
        public ActionResult Index()
        {
            var files = db.Files.Include(f => f.User);
            return View(files.ToList());
        }

        // GET: Files/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            File file = db.Files.Find(id);
            if (file == null)
            {
                return HttpNotFound();
            }
            return View(file);
        }

        // GET: Files/Create
        //public ActionResult Create()
        //{
        //    //ViewBag.UserId = new SelectList(db.ApplicationUsers, "Id", "FirstName");
        //    return View();
        //}


        // Ny post Create av Johan - under test
        // POST: Files/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(HttpPostedFileBase upload, int activityId)
         {
            if (upload != null && upload.ContentLength > 0)
            {
                var document = new File
                {
                    FileName = System.IO.Path.GetFileName(upload.FileName),
                    //FileType = FileType.Document,
                    ContentType = upload.ContentType,
                    ActivityId = activityId
                };
                using (var reader = new System.IO.BinaryReader(upload.InputStream))
                {
                    document.Content = reader.ReadBytes(upload.ContentLength);
                }
                var user = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
                if (user == null)
                {
                    return HttpNotFound();
                }
                document.UserId = user.Id;
                // För tillfället bara lärare som laddar upp dokument -> PubliclyVisible = true
                // XXX Behöver ändras om Elever ska kunna ladda upp dokument
                document.PubliclyVisible = true;
                try
                {
                    db.Files.Add(document);
                    db.SaveChanges();
                    TempData["Event"] = "File " + document.FileName + " is uploaded.";
                }
                catch (Exception e)
                {
                    TempData["NegativeEvent"] = e.Message;
                }
            }
            return RedirectToAction("Details", "Activities", new { id = activityId });
        }





        // POST: Files/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include = "Id,FileName,Description,Path,PubliclyVisible,ActivityId,ContentType,Content,FileType,UserId")] File file)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Files.Add(file);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }

        //    //ViewBag.UserId = new SelectList(db.ApplicationUsers, "Id", "FirstName", file.UserId);
        //    return View(file);
        //}

        // GET: Files/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            File file = db.Files.Find(id);
            if (file == null)
            {
                return HttpNotFound();
            }
            //ViewBag.UserId = new SelectList(db.ApplicationUsers, "Id", "FirstName", file.UserId);
            return View(file);
        }

        // POST: Files/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,FileName,Description,Path,PubliclyVisible,ActivityId,ContentType,Content,FileType,UserId")] File file)
        {
            if (ModelState.IsValid)
            {
                db.Entry(file).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            //ViewBag.UserId = new SelectList(db.ApplicationUsers, "Id", "FirstName", file.UserId);
            return View(file);
        }

        // GET: Files/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            File file = db.Files.Find(id);
            if (file == null)
            {
                return HttpNotFound();
            }

            //nedan är alla viewbags som behövs för deletefönstret
            ViewBag.ActivityId = file.ActivityId;
            Activity activity = db.Activities.FirstOrDefault(a => a.Id == file.ActivityId);
            Module module = db.Modules.FirstOrDefault(m => m.Id == activity.ModuleId);
            Course course = db.Courses.FirstOrDefault(c => c.Id == module.CourseId);
            ViewBag.CourseName = course.Name;
            ViewBag.CourseId = course.Id;
            ViewBag.ModuleId = module.Id;
            ViewBag.ModuleName = module.Name;
            ViewBag.ActivityName = activity.Name;
            return View(file);
        }

        // POST: Files/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            File file = db.Files.Find(id);
            int activityId = file.ActivityId;
            db.Files.Remove(file);
            db.SaveChanges();
            TempData["Event"] = "File " + file.FileName + " deleted from the LMS";
            

            return RedirectToAction("Details","Activities", new { id = activityId });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
