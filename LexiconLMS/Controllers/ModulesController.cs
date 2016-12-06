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
    public class ModulesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Modules
        public ActionResult Index(int courseId)
        {
            ApplicationDbContext newDbContext = new ApplicationDbContext();
            // Find course name by courseId
            var course = newDbContext.Courses.First(u => u.Id == courseId);
            ViewBag.CourseName = course.Name;
            ViewBag.CourseId = courseId;
            return View(db.Modules.ToList().Where(c => c.CourseId == courseId));
        }

        // GET: Modules/Details/5
        public ActionResult Details(int? id, string courseName)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Module module = db.Modules.Find(id);
            if (module == null)
            {
                return HttpNotFound();
            }
            if (courseName == null)
            {
                Course course = db.Courses.Find(module.CourseId);
                if (course == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                courseName = course.Name;
            }
            ViewBag.CourseName = courseName;
            return View(module);
        }

        // GET: Modules/Create
        [Authorize(Roles = "Teacher")]
        public ActionResult Create(int courseId, string courseName)
        {
                      
            ViewBag.CourseName = courseName;
            ViewBag.CourseId = courseId;
            return View();
        }

        // POST: Modules/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public ActionResult Create(string Add, [Bind(Include = "Id,CourseId,Name,StartDateTime,EndDateTime,Description")] Module module)
        {
            if (ModelState.IsValid)
            {
                ViewBag.CourseId = module.CourseId;
                //ApplicationDbContext newDbContext = new ApplicationDbContext();
                // Find course name by courseId
                var course = db.Courses.First(u => u.Id == module.CourseId);
                if (course.StartDate > module.StartDateTime)
                {
                    ModelState.AddModelError("StartDateTime", "Start date can´t be earlier than course start date");
                    return View();
                }
                if (module.StartDateTime >= module.EndDateTime)
                {
                    ModelState.AddModelError("StartDateTime", "Start date conflicts with end date ");
                    return View();
                }
                //Find modules by CourseId
                var modules = db.Modules.ToList().Where(c => c.CourseId == module.CourseId);

                foreach (var mod in modules)
                {
                    if (module.StartDateTime == mod.StartDateTime)
                    {
                        ModelState.AddModelError("StartDateTime", "Start date conflicts with Start date for another module");
                        return View();
                    }
                    else if (module.StartDateTime < mod.StartDateTime)
                    {
                        if (module.EndDateTime > mod.StartDateTime)
                        {
                            ModelState.AddModelError("EndDateTime", "End date conflicts with Start date for another module");
                            return View();
                        }
                    }
                    else if (module.StartDateTime > mod.StartDateTime)
                    {
                        if (module.StartDateTime < mod.EndDateTime)
                        {
                            ModelState.AddModelError("StartDateTime", "Start date conflicts with End date for another module");
                            return View();
                        }
                    }
                }
                db.Modules.Add(module);
                db.SaveChanges();
                TempData["Event"] = "Module " + module.Name + " added.";

                switch (Add)
                {
                    case "Save":
                        return RedirectToAction("Index", new { courseId = module.CourseId });
                    case "Save & add new module":
                        ModelState.Clear();
                        return View();
                    default:
                        throw new Exception();
                        //break;
                }
            }
            return View(module);
        }

        // GET: Modules/Edit/5
        [Authorize(Roles = "Teacher")]
        public ActionResult Edit(int? id, string courseName)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Module module = db.Modules.Find(id);
            if (module == null)
            {
                return HttpNotFound();
            }
            ViewBag.CourseName = courseName;
            return View(module);
        }

        // POST: Modules/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public ActionResult Edit([Bind(Include = "Id,CourseId,Name,StartDateTime,EndDateTime,Description")] Module module)
        {
            if (ModelState.IsValid)
            {
                if(module.StartDateTime >= module.EndDateTime)
                {
                    ModelState.AddModelError("StartDateTime", "Start date conflicts with end date ");
                    return View(module);
                }
                //Find modules by CourseId
                var modules = db.Modules.AsNoTracking().ToList().Where(c => c.CourseId == module.CourseId);

                foreach (var mod in modules)
                {
                    if (module.Id != mod.Id)
                    {
                        if (module.StartDateTime == mod.StartDateTime)
                        {
                            ModelState.AddModelError("StartDateTime", "Start date conflicts with Start date for another module");
                            return View(module);
                        }
                        else if (module.StartDateTime < mod.StartDateTime)
                        {
                            if (module.EndDateTime > mod.StartDateTime)
                            {
                                ModelState.AddModelError("EndDateTime", "End date conflicts with Start date for another module");
                                return View(module);
                            }
                        }
                        else if (module.StartDateTime > mod.StartDateTime)
                        {
                            if (module.StartDateTime < mod.EndDateTime)
                            {
                                ModelState.AddModelError("StartDateTime", "Start date conflicts with End date for another module");
                                return View(module);
                            }
                        }
                    }
                }
                db.Entry(module).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Event"] = "Module " + module.Name + " edited.";
                return RedirectToAction("Index", new { courseId = module.CourseId });
            }
            return View(module);
        }

        // GET: Modules/Delete/5
        [Authorize(Roles = "Teacher")]
        public ActionResult Delete(int? id, string courseName)
        { 
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Module module = db.Modules.Find(id);
            if (module == null)
            {
                return HttpNotFound();
            }
            ViewBag.CourseName = courseName;
            return View(module);
        }

        // POST: Modules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public ActionResult DeleteConfirmed(int id)
        {
            
            Module module = db.Modules.Find(id);
            //ViewBag.CourseId = module.CourseId;
            db.Modules.Remove(module);
            db.SaveChanges();
            TempData["Event"] = "Module " + module.Name + " removed.";
            return RedirectToAction("Index", new { courseId = module.CourseId });
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
