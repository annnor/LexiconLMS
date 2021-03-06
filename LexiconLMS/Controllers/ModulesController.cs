﻿ using System;
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
                ViewBag.CourseId = course.Id;
                courseName = course.Name;
            }
            
            ViewBag.CourseName = courseName;
            
            return View(module);
        }

        // GET: Modules/Create
        [Authorize(Roles = "Teacher")]
        public ActionResult Create(int courseId, string courseName)
        {

            //gör denna cyklisk så att viewbag alltid är populerad även när man trycker på refresh hos klienten
           Course course= db.Courses.Find(courseId);

            ViewBag.CourseName = course.Name;
            ViewBag.CourseId = course.Id;
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
                    ModelState.AddModelError("StartDateTime", "Start date can´t be earlier than course start date " + course.StartDate.ToString("yyyy-MM-dd HH:mm"));
                    //in med viewbags för modules
                    ViewBag.CourseName = course.Name;
                    ViewBag.CourseId = course.Id;
                    return View();
                }
                if (module.StartDateTime >= module.EndDateTime)
                {
                    ModelState.AddModelError("StartDateTime", "The End Time cannot be earlier than the Start Time " + module.StartDateTime.ToString("yyyy-MM-dd HH:mm"));
                    //in med viewbags för modules
                    ViewBag.CourseName = course.Name;
                    ViewBag.CourseId = course.Id;
                    return View();
                }
                //Find modules by CourseId
                var modules = db.Modules.ToList().Where(c => c.CourseId == module.CourseId);

                foreach (var mod in modules)
                {
                    if (module.StartDateTime == mod.StartDateTime)
                    {
                        ModelState.AddModelError("StartDateTime", "Start Time of the module overlaps with Start date for module '" + mod.Name + "' that starts " + mod.StartDateTime.ToString("yyyy-MM-dd HH:mm"));
                        //in med viewbags för modules
                        ViewBag.CourseName = course.Name;
                        ViewBag.CourseId = course.Id;
                        return View();
                    }
                    else if (module.StartDateTime < mod.StartDateTime)
                    {
                        if (module.EndDateTime > mod.StartDateTime)
                        {
                            ModelState.AddModelError("EndDateTime", "End Time overlaps Start Time for module '" + mod.Name + "' that starts " + mod.StartDateTime.ToString("yyyy-MM-dd HH:mm"));
                            //in med viewbags för modules
                            ViewBag.CourseName = course.Name;
                            ViewBag.CourseId = course.Id;
                            return View();
                        }
                    }
                    else if (module.StartDateTime > mod.StartDateTime)
                    {
                        if (module.StartDateTime < mod.EndDateTime)
                        {
                            ModelState.AddModelError("StartDateTime", "Start Time overlaps End Time for module '" + mod.Name + "' that ends '" + mod.EndDateTime.ToString("yyyy-MM-dd HH:mm"));
                            //in med viewbags för modules
                            ViewBag.CourseName = course.Name;
                            ViewBag.CourseId = course.Id;
                            return View();
                        }
                    }
                }
                try
                {
                db.Modules.Add(module);
                db.SaveChanges();
                TempData["Event"] = "Module " + module.Name + " added.";

                switch (Add)
                {
                    case "Save":
                        return RedirectToAction("Index", new { courseId = module.CourseId });
                    case "Save & add new module":
                        ModelState.Clear();
                            //in med viewbags för modules
                            ViewBag.CourseName = course.Name;
                            ViewBag.CourseId = course.Id;
                            return View();
                    default:
                        throw new Exception();
                        //break;
                }

            }
                catch (Exception e)
                {
                    TempData["NegativeEvent"] = e.Message;
                    return View();
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
            ViewBag.CourseId = module.CourseId;
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
                var course = db.Courses.First(u => u.Id == module.CourseId);
                ViewBag.CourseId = course.Id;
                ViewBag.CourseName = course.Name;
                if (course.StartDate > module.StartDateTime)
                {
                    ModelState.AddModelError("StartDateTime", "Start date can´t be earlier than course start date " + course.StartDate.ToString("yyyy-MM-dd HH:mm"));
                    return View(module);
                }
                if (module.StartDateTime >= module.EndDateTime)
                {
                    ModelState.AddModelError("StartDateTime", "The End Time cannot be earlier than the Start Time " + module.StartDateTime.ToString("yyyy-MM-dd HH:mm"));
                    return View(module);
                }
                
                //Find modules by CourseId
                var modules = db.Modules.AsNoTracking().ToList().Where(c => c.CourseId == module.CourseId);
                //var activities = module.Activities.ToList();
                foreach (var mod in modules)
                {   
                    
                    if (module.Id == mod.Id)
                    {
                        foreach (var activity in mod.Activities)
                        {
                            if (module.StartDateTime > activity.StartDateTime)
                            {
                                TempData["NegativeEvent"] = "You can't save because you have an activity that starts earlier than the new module startdate";
                                return View(module);
                            }
                            else if (module.EndDateTime < activity.EndDateTime)
                            {
                                TempData["NegativeEvent"] = "You can't save because you have an activity that ends later than the new module enddate";
                                return View(module);
                            }
                        }
                    }
                }
                foreach (var mod in modules)
                {
                        if (module.Id != mod.Id)
                    {
                        if (module.StartDateTime == mod.StartDateTime)
                        {
                            ModelState.AddModelError("StartDateTime", "Start Time of the module overlaps with Start date for module '" + mod.Name + "' that starts " + mod.StartDateTime.ToString("yyyy-MM-dd HH:mm"));
                            return View(module);
                        }
                        else if (module.StartDateTime < mod.StartDateTime)
                        {
                            if (module.EndDateTime > mod.StartDateTime)
                            {
                                ModelState.AddModelError("EndDateTime", "End Time overlaps Start Time for module '" + mod.Name + "' that starts " + mod.StartDateTime.ToString("yyyy-MM-dd HH:mm"));
                                return View(module);
                            }
                        }
                        else if (module.StartDateTime > mod.StartDateTime)
                        {
                            if (module.StartDateTime < mod.EndDateTime)
                            {
                                ModelState.AddModelError("StartDateTime", "Start Time overlaps End Time for module '" + mod.Name + "' that ends '" + mod.EndDateTime.ToString("yyyy-MM-dd HH:mm"));
                                return View(module);
                            }
                        }
                    }
                }
                try
                {
                db.Entry(module).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Event"] = "Module " + module.Name + " edited.";
                return RedirectToAction("Index", new { courseId = module.CourseId });
            }
                catch(Exception e)
                {
                    TempData["NegativeEvent"] = e.Message;
            return View(module);
        }
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
            ViewBag.CourseId = module.CourseId;
            return View(module);
        }

        // POST: Modules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public ActionResult DeleteConfirmed(int id)
        {
            var file = new FilesController();

            Module module = db.Modules.AsNoTracking().FirstOrDefault(a => a.Id == id);                 //Find(id);
            var activities = module.Activities.ToList();
            
            foreach (var a in activities)
            {
                var files = a.Files.ToList();
                file.DeleteFiles(files);
            }

            var moduleFiles = module.Files.ToList();
            file.DeleteFiles(moduleFiles);
            //ViewBag.CourseId = module.CourseId;
            try
            {
            module = db.Modules.Find(id);
            db.Modules.Remove(module);
            db.SaveChanges();
            TempData["Event"] = "Module " + module.Name + " removed.";
            }
            catch (Exception e)
            {
                TempData["NegativeEvent"] = e.Message;
            }
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
