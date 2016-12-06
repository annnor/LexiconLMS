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
    public class CoursesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();


        //file handler written by Johan - it is not complete
        [HttpPost]
        public ActionResult Upload(string userName, int courseId, HttpPostedFileBase upload) //username is the Identity username(email) and Id is the course Id
        {
            if (upload != null && upload.ContentLength > 0)
            {
                var file = new File
                {
                    Path = System.IO.Path.GetFileName(upload.FileName),
                    FileType = FileType.Document,
                    ContentType = upload.ContentType,
                    CourseId = courseId,
                    

                };
                //using (var reader = new System.IO.BinaryReader(upload.InputStream))
                //{
                //    file.Content = reader.ReadBytes(upload.ContentLength);
                //}
                //student.Files = new List<File> { avatar };
            }


            return View();
        }


        // GET: Courses
        public ActionResult Index()
        {
            if (User.IsInRole("Student"))
            {
                return RedirectToAction("StudentHome");
            }
            return View(db.Courses.ToList());
        }

        // GET: Courses/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (User.IsInRole("Student"))
            {
                return RedirectToAction("StudentHome");
            }
            Course course = db.Courses.Find(id);
            if (course == null)
            {
                return HttpNotFound();
            }
            return View(course);
        }

        // GET: Courses/StudentHome
        public ActionResult StudentHome()
        {
            //ApplicationUserManager manager = new ApplicationUserManager()
            //var user = await UserManager.FindByNameAsync(User.Identity.Name);
            if (User.IsInRole("Student"))
            {
                var user = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
                if (user != null)
                {
                    Course course = db.Courses.Find(user.CourseId);
                    if (course != null)
                    {
                        return View(course);
                    }
                    return HttpNotFound();
                }
                return HttpNotFound();
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        // GET: Courses/Create
        [Authorize(Roles = "Teacher")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Courses/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        //modified parameter head to allow save multiple and redirect appropriately - test
        public ActionResult Create([Bind(Include = "Id,Name,StartDate,Description"),] Course course,string save1,string saveMultiple)
        {
            //kontroll av att startdatum inte är satt tidigare än idag
            if (course.StartDate<DateTime.Today)
            {
                //felmeddelande + returnera view
                //ViewBag.Message = "You tried to create a course with a startdate earlier than today. Please try again.";
                //använder tempdata istället
                TempData["CreateError"] = "You tried to create a course with a start date earlier than today. Please try again!";
                return View();
            }
            //kontroll av att kursnamnet är unikt
            foreach (var existingcourse in db.Courses)
                {
                    if (existingcourse.Name==course.Name)
                    {//kurs med det namnet existerar redan i databasen. skicka felmeddelande till klient.
                    TempData["CreateError"] = "You tried to create a course with a name that already exists. Please try again with another name!";
                    return View();
                    }
                }
            if (ModelState.IsValid)
            {
                db.Courses.Add(course);
                db.SaveChanges();
                //meddelande till klient att kursen är skapad
                TempData["Event"] = "Course " + course.Name + " added.";
                //return RedirectToAction("Index");//, new { Id = course.Id });
            }

            //code for appropriate redirects below
            if (save1 != null) return RedirectToAction("Index");
            if (saveMultiple != null) return RedirectToAction("Create");
            return View(course);//should not be able to reach this return
        }

        // GET: Courses/Edit/5
        [Authorize(Roles = "Teacher")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Course course = db.Courses.Find(id);
            if (course == null)
            {
                return HttpNotFound();
            }
            return View(course);
        }

        // POST: Courses/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public ActionResult Edit([Bind(Include = "Id,Name,StartDate,Description")] Course course)
        {
            var oldCourse = db.Courses.AsNoTracking().FirstOrDefault(c => c.Id == course.Id);
            if (oldCourse == null)
            {
                return HttpNotFound();
            }
            var modules = oldCourse.Modules;
            if (ModelState.IsValid)
            {
                if (oldCourse.StartDate < course.StartDate &&
                    oldCourse.Modules.Count > 0 &&
                    modules.OrderBy(m => m.StartDateTime)
                    .First().StartDateTime < course.StartDate)
                {
                    ModelState.AddModelError("StartDate", "StartDate overlaps first module.");
                    return View(course);
                }
                db.Entry(course).State = EntityState.Modified;
                db.SaveChanges();
                //text till klient
                TempData["Event"] = "Course " + course.Name + " edited.";
                return RedirectToAction("Index");
            }
            return View(course);
        }

        // GET: Courses/Delete/5
        [Authorize(Roles = "Teacher")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Course course = db.Courses.Find(id);
            if (course == null)
            {
                return HttpNotFound();
            }
            return View(course);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public ActionResult DeleteConfirmed(int id)
        {
            bool sucess = DeleteStudents(id);

            if (sucess)
            {
                Course course = db.Courses.Find(id);
                //Course course = db.Courses.Include(m => m.C(id);

                try
                {
                    db.Courses.Remove(course);
                    db.SaveChanges();
                    TempData["Event"] = "Course " + course.Name + " removed.";
                }
                catch (Exception e)
                {
                    TempData["NegativeEvent"] = e.Message;
                }
            }
            return RedirectToAction("Index");
        }
        private bool DeleteStudents(int id)
        {
            ApplicationDbContext newDbContext = new ApplicationDbContext();
            LexiconLMS.Models.ApplicationUser student;
            
            //Find students by courseId    
            var allStudents = newDbContext.Users.Where(u => u.CourseId == id);

                try
                {
                    //Delete students by course
                    foreach (var students in allStudents.ToList())
                    {
                        student = db.Users.Find(students.Id);
                        db.Users.Remove(student);
                        //db.SaveChanges();
                    }
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            //}
            
        }

        /// <summary>
        /// Shows some modules in a partial view to students. This will hopefully be expandable in the future.
        /// </summary>
        public ActionResult UpComingEvents(int courseId)
        {
            if (User.IsInRole("Student"))
            {
                List<Module> upComingEventsList = new List<Module>();
                //find lets say 3 modules for that course. use todays date and forward
                foreach(var module in db.Modules)
                {
                    if (module.CourseId==courseId)
                    {
                        //här behövs en kontroll så att samma modul inte läggs i listan upcomingevents
                        //om modulen inte finns i listan samt att startdatum för modulen är större än idag -5 dagar
                        
                        if (!upComingEventsList.Contains(module) && module.StartDateTime>=(DateTime.Today.AddDays(-5)))
                        {
                            //här behövs en datumkontroll så att inte gamla moduler läggs till
                            upComingEventsList.Add(module);
                        }
                    }
                    

                }
                //ordna listan
                upComingEventsList.OrderBy(o => o.StartDateTime);

                
                //trimma listan om behövs
                if (upComingEventsList.Count>3) 
                {
                    while (upComingEventsList.Count > 2)
                        upComingEventsList.RemoveAt(upComingEventsList.Count - 1);
                }
                //return the view.
                return PartialView("_UpComingEvents", upComingEventsList);

            }
            return View(); //bör ej nå hit
        }


        //metod för att visa eleverna på vald kursdetaljsidan
        public ActionResult StudentListForCourse(int courseId) 
        {                   
            ApplicationDbContext newDbContext = new ApplicationDbContext();
            //skapa en tom lista. fyll på med lämliga applicationusers i if-satserna
            List<UserViewModels> listOfUsers = new List<UserViewModels>();

            if (User.IsInRole("Teacher")) //lärare skall se alla studenter i kursen han är på i detaljsidan
            {
               foreach (var user in newDbContext.Users.ToList()) //sen måste eleverna som går den kursen fås fram från identitymodels och föras över till userviewmodel
                {
                    if (courseId == user.CourseId) //elev är med i kursen på detaljsidan
                    {                                                                                    
                        UserViewModels studenInSameCourse = new UserViewModels
                        {
                            Adress = user.Adress,
                            FirstName = user.FirstName,
                            //Id = int.Parse(user.Id),
                            LastName = user.LastName,
                            Email = user.Email
                           // CourseName = getCourseName
                        };
                        listOfUsers.Add(studenInSameCourse);
                    }
                }

            }
            //sätt globala static strängen i accountcontroller till partiallist så vi vet var vi ska redirecta
            //även details är en global static i accountcontrollern
            AccountController.selectedList = "PartialList";
            AccountController.details = courseId;
            //sen måste den skickas till klienten
            return PartialView("_StudentList", listOfUsers); 
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
