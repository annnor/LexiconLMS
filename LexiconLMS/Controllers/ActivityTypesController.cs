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
    [Authorize(Roles = "Teacher")]
    public class ActivityTypesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: ActivityTypes
        public ActionResult Index()
        {
            return View(db.ActivityTypes.ToList());
        }

        // GET: ActivityTypes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ActivityType activityType = db.ActivityTypes.Find(id);
            if (activityType == null)
            {
                return HttpNotFound();
            }
            return View(activityType);
        }

        // GET: ActivityTypes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ActivityTypes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name")] ActivityType activityType, string save1, string saveMultiple)
        {
            var existingType = db.ActivityTypes.AsNoTracking().Where(a => a.Name == activityType.Name).FirstOrDefault();
            if (ModelState.IsValid)
            {
                if (existingType == null)
                {
                    db.ActivityTypes.Add(activityType);
                    db.SaveChanges();
                    TempData["Event"] = "Activity Type " + activityType.Name + " added.";
                    if (save1 != null) return RedirectToAction("Index");
                    if (saveMultiple != null) return RedirectToAction("Create");
                    return RedirectToAction("Index"); //this redirect is unreachable since the if's above handle that
                }
                ModelState.AddModelError("Name", "An Activity Type with the same name already exists!");
            }

            return View(activityType);
        }

        // GET: ActivityTypes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ActivityType activityType = db.ActivityTypes.Find(id);
            if (activityType == null)
            {
                return HttpNotFound();
            }
            return View(activityType);
        }

        // POST: ActivityTypes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name")] ActivityType activityType)
        {
            if (ModelState.IsValid)
            {
                db.Entry(activityType).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Event"] = "Activity Type " + activityType.Name + " edited.";
                return RedirectToAction("Index");
            }
            return View(activityType);
        }

        // GET: ActivityTypes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ActivityType activityType = db.ActivityTypes.Find(id);
            if (activityType == null)
            {
                return HttpNotFound();
            }
            if (activityType.activities.Count == 0)
            {
                return View(activityType);
            }
            TempData["NegativeEvent"] = "You cannot remove Activity Type " + activityType.Name + " since it is in use.";
            return RedirectToAction("Index");
        }

        // POST: ActivityTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ActivityType activityType = db.ActivityTypes.Find(id);
            db.ActivityTypes.Remove(activityType);
            db.SaveChanges();
            TempData["Event"] = "Activity Type " + activityType.Name + " removed.";
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
