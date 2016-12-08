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
            return View(activity);
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
        public ActionResult Create([Bind(Include = "Id,ActivityTypeId,Name,StartDateTime,EndDateTime,Description, ModuleId")] Activity activity, string save1, string saveMultiple)
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
            return View(activity);
        }

        // POST: Activities/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public ActionResult Edit([Bind(Include = "Id,ModuleId,ActivityTypeId,Name,StartDateTime,EndDateTime,Description")] Activity activity)
        {
            var module = db.Modules.AsNoTracking().FirstOrDefault(m => m.Id == activity.ModuleId);
            if (module == null)
            {
                return HttpNotFound();
            }
            if (ModelState.IsValid && DateTimeNotOverlap(activity.Id, activity.StartDateTime, activity.EndDateTime, module))
            {
                db.Entry(activity).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Event"] = "Activity " + activity.Name + " edited.";
                return RedirectToAction("Details", "Modules", new { id = activity.ModuleId });
            }
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
            return View(activity);
        }

        // POST: Activities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public ActionResult DeleteConfirmed(int id)
        {
            Activity activity = db.Activities.Find(id);
            var moduleId = activity.ModuleId;
            db.Activities.Remove(activity);
            db.SaveChanges();
            TempData["Event"] = "Activity " + activity.Name + " removed.";
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
                {// The whole new activity is in the time span of the other activity
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
