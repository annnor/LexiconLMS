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
            //int courseId = 3;
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
        public ActionResult Create([Bind(Include = "Id,CourseId,Name,StartDateTime,EndDateTime,Description")] Module module/*, string save1, string saveMultiple*/)
        {
            if (ModelState.IsValid)
            {
                db.Modules.Add(module);
                db.SaveChanges();
                //TempData["Event"] = "You have created Module " + module.Name;
                
                //var courseNameToPassToCreateMethod = db.Courses.First(c => c.Id == module.CourseId).Name;
                

                ////code for appropriate redirects below
                //if (save1 != null) return RedirectToAction("Index",new {courseId=module.CourseId});//skicka med rätt courseid
                //if (saveMultiple != null) return RedirectToAction("Create",new {courseId= module.CourseId, courseName = courseNameToPassToCreateMethod });
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
                db.Entry(module).State = EntityState.Modified;
                db.SaveChanges();
                TempData["message"] = "You have edited Module " + module.Name;
                return RedirectToAction("Index");
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
            db.Modules.Remove(module);
            db.SaveChanges();
            TempData["message"] = "You have removed Module " + module.Name;
            return RedirectToAction("Index");
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
