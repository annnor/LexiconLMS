﻿using System;
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
    [Authorize]
    public class ActivitiesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Activities
        public ActionResult Index()
        {
            var activities = db.Activities.Include(a => a.Type);
            return View(activities.ToList());
        }

        // GET: Activities/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Activity activity = db.Activities.Find(id);
            if (activity == null)
            {
                return HttpNotFound();
            }
            ViewBag.CourseName = db.Courses.FirstOrDefault(c => c.Id == activity.Module.CourseId).Name;
            //TempData["Event"] = "";
            return View(activity);
        }

        //Post: Activities/Details
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public ActionResult Details(HttpPostedFileBase upload, Activity activity)
        {
            if (upload != null && upload.ContentLength > 0)
            {
                var document = new File
                {                                   
                    FileName = System.IO.Path.GetFileName(upload.FileName),
                    //FileType = FileType.Document,
                    ContentType = upload.ContentType,
                    ActivityId = activity.Id
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
                catch(Exception e)
                {
                    TempData["NegativeEvent"] = e.Message;
                }
                            }
            return RedirectToAction("Details", "Activities", new { id = activity.Id });
            //return View(activity);
        }
        public ActionResult Download(int id)
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
            return RedirectToAction("Details",new { id = dataFile.ActivityId } );
        }
        // GET: Files 
        public ActionResult FilesList(int? id, bool publiclyVisible)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Activity activity = db.Activities.Find(id);
            if (activity == null)
            {
                return HttpNotFound();
            }
            ICollection<File> files = null;
            if (publiclyVisible)
            {
                files = activity.Files.Where(f => f.PubliclyVisible).ToList();
                ViewBag.Heading = "Activity related files";
            } else if (User.IsInRole("Teacher"))
            {
                files = activity.Files.Where(f => ! f.PubliclyVisible).ToList();
                ViewBag.Heading = "Files uploaded by students";
            } else
            {   // Studentens privata filer
                files = activity.Files.Where(f => f.User.Email == User.Identity.Name).ToList();
                ViewBag.Heading = "Uploaded files";
            }
            return PartialView("_FileList", files);
        }

        // GET: Activities/Create
        [Authorize(Roles = "Teacher")]
        public ActionResult Create(int moduleId)
        {
            var module = db.Modules.FirstOrDefault(m => m.Id == moduleId);
            if (module == null)
            {
                return HttpNotFound();
            }
            var course = db.Courses.FirstOrDefault(c => c.Id == module.CourseId);
            if (course == null)
            {
                return HttpNotFound();
            }
            ViewBag.CourseName = course.Name;
            ViewBag.CourseId = course.Id;
            ViewBag.ModuleId = module.Id;
            ViewBag.ModuleName = module.Name;
            ViewBag.ActivityTypeId = new SelectList(db.ActivityTypes, "Id", "Name");
            return View();
        }

        // POST: Activities/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public ActionResult Create([Bind(Include = "Id,ActivityTypeId,Name,StartDateTime,EndDateTime,Description, ModuleId, StudentUpload")] Activity activity, string save1, string saveMultiple)
        {
            var module = db.Modules.Find(activity.ModuleId);
            if (module == null)
            {
                return HttpNotFound();
            }
            var course = db.Courses.Find(module.CourseId);
            if (course == null)
            {
                return HttpNotFound();
            }
            if (ModelState.IsValid &&
                DateTimeNotOverlap(null, activity.StartDateTime, activity.EndDateTime, module))
            {
                db.Activities.Add(activity);
                db.SaveChanges();
                TempData["Event"] = "Activity " + activity.Name + " added.";
                if (save1 != null) return RedirectToAction("Details", "Modules", new { id = activity.ModuleId, courseName = course.Name });
                if (saveMultiple != null) return RedirectToAction("Create", "Activities", new { moduleId = activity.ModuleId });
            }
            ViewBag.ModuleId = module.Id;
            ViewBag.ModuleName = module.Name;
            ViewBag.ActivityTypeId = new SelectList(db.ActivityTypes, "Id", "Name", activity.ActivityTypeId);
            return View(activity);
        }

        // GET: Activities/Edit/5
        [Authorize(Roles = "Teacher")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Activity activity = db.Activities.Find(id);
            if (activity == null)
            {
                return HttpNotFound();
            }
            ViewBag.ActivityTypeId = new SelectList(db.ActivityTypes, "Id", "Name", activity.ActivityTypeId);
            ViewBag.CourseName = db.Courses.FirstOrDefault(c => c.Id == activity.Module.CourseId).Name;
            return View(activity);
        }

        // POST: Activities/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public ActionResult Edit([Bind(Include = "Id,ModuleId,ActivityTypeId,Name,StartDateTime,EndDateTime,Description, StudentUpload")] Activity activity)
        {
            var module = db.Modules.AsNoTracking().FirstOrDefault(m => m.Id == activity.ModuleId);
            if (module == null)
            {
                return HttpNotFound();
            }
            if (ModelState.IsValid && DateTimeNotOverlap(activity.Id, activity.StartDateTime, activity.EndDateTime, module))            {
            db.Entry(activity).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Event"] = "Activity " + activity.Name + " edited.";
                return RedirectToAction("Details", "Modules", new { id = activity.ModuleId });
            }
            var course = db.Courses.AsNoTracking().FirstOrDefault(c => c.Id == module.CourseId);
            if (course == null)
            {
                return HttpNotFound();
            }
            activity.Module = module;
            ViewBag.CourseName = course.Name;
            ViewBag.ActivityTypeId = new SelectList(db.ActivityTypes, "Id", "Name", activity.ActivityTypeId);
            return View(activity);
        }

        // GET: Activities/Delete/5
        [Authorize(Roles = "Teacher")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Activity activity = db.Activities.Find(id);
            if (activity == null)
            {
                return HttpNotFound();
            }
            ViewBag.CourseName = db.Courses.FirstOrDefault(c => c.Id == activity.Module.CourseId).Name;
            return View(activity);
        }

        // POST: Activities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public ActionResult DeleteConfirmed(int id)
        {
            FilesController file = new FilesController();
            Activity activity = db.Activities.AsNoTracking().FirstOrDefault(a => a.Id == id);
            var files = activity.Files.ToList();
            var moduleId = activity.ModuleId;
            bool result = file.DeleteFiles(files);

            if (result)
            {
                activity = db.Activities.Find(id);
                db.Activities.Remove(activity);
                db.SaveChanges();
                TempData["Event"] = "Activity " + activity.Name + " removed.";
            }
            else
            {
                TempData["NegativeEvent"] = "Activity " + activity.Name + " couldn´t be removed.";
            }

            return RedirectToAction("Details", "Modules", new { id = moduleId });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool DateTimeNotOverlap(int? id, DateTime startDateTime, DateTime endDateTime, Module module)
        {
            if (endDateTime <= startDateTime)
            {
                ModelState.AddModelError("EndDateTime", "The End Time cannot be earlier than the Start Time " + startDateTime.ToString("yyyy-MM-dd HH:mm"));
                return false;
            } else if (startDateTime < module.StartDateTime)
            {
                ModelState.AddModelError("StartDateTime", "The Start Time of the activity cannot be earlier than the Start Time of the module, " + module.StartDateTime.ToString("yyyy-MM-dd HH:mm"));
                return false;
            } else if (module.EndDateTime < endDateTime)
            {
                ModelState.AddModelError("EndDateTime", "The End Time of the activity cannot be later than the End Time of the module, " + module.EndDateTime.ToString("yyyy-MM-dd HH:mm"));
                return false;
            }
            ICollection<Activity> activities = module.Activities;
            // Collect the activities that starts before this new activity should end:
            var priorActivities = activities.OrderBy(a => a.StartDateTime)
                .Where(a => a.StartDateTime < endDateTime && a.Id != id);
            // Check that there is no overlap between adjacent activities
            var lastItem = priorActivities.LastOrDefault();
            if (lastItem == null || lastItem.EndDateTime <= startDateTime)
            {
                return true;
            } else if (endDateTime < lastItem.EndDateTime)
            {
                if (lastItem.StartDateTime <= startDateTime)
                {   // The whole new activity is in the time span of the other activity
                    ModelState.AddModelError("StartDateTime", "This Activity overlaps the activity '" + lastItem.Name + "' which End Time is " + lastItem.EndDateTime.ToString("yyyy-MM-dd HH:mm"));
                } else
                {
                    ModelState.AddModelError("EndDateTime", "This Activity overlaps the activity '" + lastItem.Name + "' which Start Time is " + lastItem.StartDateTime.ToString("yyyy-MM-dd HH:mm"));
                }
            } else
            {
                ModelState.AddModelError("StartDateTime", "This Activity overlaps the activity '" + lastItem.Name + "' which End Time is " + lastItem.EndDateTime.ToString("yyyy-MM-dd HH:mm"));
            }
            return false;
        }
    }
}
