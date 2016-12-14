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

        public bool DeleteFiles(ICollection<File> files)
        {
            bool result = true;
            if (files.Count > 0)
            {

                foreach (var f in files.ToList())
                {

                    try
                    {
                        File file = db.Files.Find(f.Id);
                        db.Files.Remove(file);
                        db.SaveChanges();
                        result = true;

                    }
                    catch (Exception e)
                    {
                        return false;
                    }
                }
            }

            return result;


        }
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

        public ActionResult ShowCourseFiles(int courseId)
        {
            List<File> filesToShow = new List<File>();

            foreach (var file in db.Files)
            {
                if (file.CourseId==courseId)
                {
                    filesToShow.Add(file);
                }
            }


            return PartialView("_courseFiles",filesToShow);
        }


        // Ny post Create av Johan - under test
        // POST: Files/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(HttpPostedFileBase upload, int activityId, string description)
         {
            if (upload != null && upload.ContentLength > 0)
            {
                if (User.IsInRole("Teacher"))
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
                document.Description = description;
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

                    //return RedirectToAction("Details", "Activities", new { id = activityId });

                } //end user is in role teacher

                if (User.IsInRole("Student"))
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
                document.Description = description;
                    // För tillfället bara lärare som laddar upp dokument -> PubliclyVisible = true
                    // XXX Behöver ändras om Elever ska kunna ladda upp dokument
                    document.PubliclyVisible = false;
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
            } //end user is in role student

           
                }
            return RedirectToAction("Details", "Activities", new { id = activityId });
        }


        // Ny post Create av Johan - för filer på course nivå
        //denna är en modifiering av create på aktivitetsnivå pga av tidsnöd
        // POST: Files/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCourseFile(HttpPostedFileBase upload, int courseId, string description)
        {
            if (upload != null && upload.ContentLength > 0)
            {
                if (User.IsInRole("Teacher"))
                {
                    var document = new File
                    {
                        FileName = System.IO.Path.GetFileName(upload.FileName),
                        //FileType = FileType.Document,
                        ContentType = upload.ContentType,
                        CourseId = courseId
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
                    document.Description = description;
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

                    //return RedirectToAction("Details", "Activities", new { id = activityId });

                } //end user is in role teacher
                
            }
            return RedirectToAction("Details", "Courses", new { id = courseId });
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
        public ActionResult StudentDownloadFile(int id)
        {
            File dataFile = db.Files.Find(id);
            byte[] fileData = dataFile.Content;
            Response.Clear();
            Response.ClearHeaders();
            Response.ClearContent();
            Response.ContentType = dataFile.ContentType;
            Response.AddHeader("Content-Disposition", string.Format(dataFile.ContentType));
            Response.BinaryWrite(fileData);
            Response.End();
            TempData["Event"] = "File " + dataFile.FileName + " downloaded.";
            return RedirectToAction("Studenthome","Courses");
        }

        /// <summary>
        /// delete hanterare av filer på kursnivå
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult DeleteCourseFile(int? id)
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
            int tempCourseId = file.CourseId.GetValueOrDefault();
            //nedan är alla viewbags som behövs för deletefönstret
            db.Files.Remove(file);
            db.SaveChanges();
            TempData["Event"] = "File " + file.FileName + " deleted from the LMS";
            return RedirectToAction("Details", "Courses", new { id = tempCourseId });
        }

        // POST: Files/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            File file = db.Files.Find(id);
            int activityId = file.ActivityId.GetValueOrDefault();
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
