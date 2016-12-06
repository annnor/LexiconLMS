using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using LexiconLMS.Models;
using System.Collections.Generic;
using System.Web.UI.WebControls.WebParts;
using System.Net;

namespace LexiconLMS.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        
        public static string selectedList { get; set; }
        public static int details { get; set; }

        public AccountController()
        {
        }


        // GET: Own made edit by Johan/Edit/5
        [Authorize(Roles = "Teacher")]
        public ActionResult Edit(string email)
        {
            ApplicationDbContext newDbContext = new ApplicationDbContext();
            
            if (email == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = newDbContext.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                return HttpNotFound();
            }

            //user är hittad. sätt värden till placeholdern userviewmodel och skicka till klienten
            UserViewModels userToEdit = new UserViewModels
            {
                Adress = user.Adress,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
            };
            return View(userToEdit);
        }

        //hårt modifierad Post metod för edit av Johan S.
        // POST: users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        //public ActionResult Edit([Bind(Include = "Id,Name,StartDateTime,Description")] Course course)
        public ActionResult Edit(string firstName, string lastName, string adress, string email,string oldEmail)
        {
            //skapa temporär anslutning till databasen
            ApplicationDbContext newDbContext = new ApplicationDbContext();



            if (oldEmail == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //kontroll av att användaren finns i databasen
            var originalUser = newDbContext.Users.FirstOrDefault(u => u.Email == oldEmail);
            bool hasUpdated = false;
            bool emailChanged = false;
            if (originalUser!=null)
            {
                //uppdatera fälten - nullcheck innan. kan behövas att e-postformatet är korrekt. detta bör ske på klientsidan (antar jag)
                if (email!=null && email!=oldEmail) //om nya email har ett värde och att värdet inte är samma som gamla värdet så fortsätt
                {   //om eposten är ändrad-i så fall skall användaren loggas ut efter ändringen och meddelande skall skickas till klient

                    originalUser.Email = email;
                    originalUser.UserName = email;
                    hasUpdated = true;
                    emailChanged = true;
                }
                if (firstName!=null && firstName!=originalUser.FirstName)
                {
                    originalUser.FirstName = firstName;
                    hasUpdated = true;
                    TempData["NegativeEvent"] = "Since you have changed your e-mail adress you are automatically logged out and must log in with your new e-mailadress";
                }
                if (lastName!=null &&lastName!=originalUser.LastName)
                {
                    originalUser.LastName = lastName;
                    hasUpdated = true;
                }
                if (adress!=null &&adress!=originalUser.Adress)
                {
                    originalUser.Adress = adress;
                    hasUpdated = true;
                }
                //spara till databasen 
                newDbContext.SaveChanges();

                if (hasUpdated)
                {
                    //skicka ok meddelande till klient.
                    TempData["Event"] = originalUser.FullName + " updated.";
                }
                if (emailChanged)
                {
                    var AutheticationManager = HttpContext.GetOwinContext().Authentication;
                    AuthenticationManager.SignOut();
                }
                //redirecta till rätt lista

                if (selectedList== "PartialList")
                {
                    //här skall redirect till coursescontroller med korrekt indata av en int till till 
                    return RedirectToAction("Details", "Courses", new {id = details});
                }

                if (selectedList == "Teacherlist")
                {
                    return RedirectToAction("TeacherList");
                }
                else return RedirectToAction("StudentList");
                
            }
            //om vi har kommit hit är originaluser inte hittad. skicka felmeddelande till klient 
            TempData["NegativeEvent"] ="User not found. Please try again or navigate using the navbar on top of the page.";
            return View(); 
        }

        // GET: Users/Delete/By Johan. Delete user, regardless of a teacher or student. Must be a teacher to perform this.
        [Authorize(Roles = "Teacher")]
        public ActionResult Delete(string email)
        {
            if (email == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationDbContext newDbContext = new ApplicationDbContext();
            var user = newDbContext.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                return HttpNotFound();
            }
            if (user.Email==User.Identity.Name) //användare kan inte ta bort själv. Remove-knappen är borta men detta är en extra koll
            {
                TempData["NegativeEvent"] = "You cannot remove yourself. Ask a colleague to remove your account.";
                return RedirectToAction("Index", "Courses");
            }

            //här måste user konverteras till viewmodel som presenteras till läraren
            UserViewModels userToDelete = new UserViewModels
            {
                Adress = user.Adress,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                
               
            };
            if (user.CourseId!=null) //user går en kurs. denna kurs skall presenteras till delete view
            {
                Course course = newDbContext.Courses.Find(user.CourseId);
                string courseName = course.Name;
                TempData["courseName"] = "Studying "+courseName;
            }

            //även rollen skall med
            var teacherRoleInformation = newDbContext.Roles.FirstOrDefault(n => n.Name == "Teacher");
            var allTeachers = newDbContext.Users.Where(u => u.Roles.FirstOrDefault().RoleId == teacherRoleInformation.Id);
            //nu har vi ett objekt all teachers. nu måste vi kolla om personen finns med i objektet. 
            foreach (var teacher in allTeachers)
            {
                if (teacher.Email==user.Email)
                {
                    TempData["userRole"] = "This user is a teacher.";
                }
            }

            var studentRoleInformation = newDbContext.Roles.FirstOrDefault(n => n.Name == "Student");
            var allStudents = newDbContext.Users.Where(u => u.Roles.FirstOrDefault().RoleId == studentRoleInformation.Id);
            foreach (var student in allStudents)
            {
                if (student.Email == user.Email)
                {
                    TempData["userRole"] = "This user is a student.";
                }
            }


            return View(userToDelete);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public ActionResult DeleteConfirmed(string email)
        {
            ApplicationDbContext newDbContext = new ApplicationDbContext();
            //hitta user genom eposten
            ApplicationUser user = newDbContext.Users.FirstOrDefault(u => u.Email == email);
            if (user!=null)
            {
                string presentedName = user.FullName;
                newDbContext.Users.Remove(user);
                newDbContext.SaveChanges();

                //in med konfirmeringsmeddelande nedan
                TempData["Event"] = presentedName + " deleted from the LMS.";

                if (selectedList == "PartialList")
                {
                    //här skall redirect till coursescontroller med korrekt indata av en int till till 
                    return RedirectToAction("Details", "Courses", new { id = details });
                }

                if (selectedList == "Teacherlist")
                {
                    return RedirectToAction("TeacherList");
                }
                else
                {
                    return RedirectToAction("StudentList");
                }
            }
            //om vi har kommit hit är originaluser inte hittad. skicka felmeddelande till klient 
            TempData["NegativeEvent"] = "User not found. Please try again or navigate using the navbar on top of the page.";
            return View();
        }

        public ActionResult TeacherList()
        {

            if (!User.IsInRole("Teacher"))
            {
                return RedirectToAction("StudentHome", "Courses");
            }//end rollkoll-if


            List<UserViewModels> listOfTeachers = new List<UserViewModels>();
            ApplicationDbContext newDbContext = new ApplicationDbContext();

            var studentRoleInformation = newDbContext.Roles.FirstOrDefault(n => n.Name == "Teacher");
            var allTeachers = newDbContext.Users.Where(u => u.Roles.FirstOrDefault().RoleId == studentRoleInformation.Id);

            //transfer over allteachers to be compatible with the UserViewModel
            foreach (var teachers in allTeachers.ToList())
            {
                //string getCourseName = "";
                //var firstOrDefault = newDbContext.Courses.FirstOrDefault(c => c.Id == teachers.CourseId);
                //if (firstOrDefault != null)
                //    getCourseName = firstOrDefault.Name;
                UserViewModels teacher = new UserViewModels
                {
                    Adress = teachers.Adress,
                    FirstName = teachers.FirstName,
                    //Id = int.Parse(user.Id),
                    LastName = teachers.LastName,
                    Email = teachers.Email,
                   //CourseName = getCourseName
                };
                listOfTeachers.Add(teacher); //add objects one by one to the list to be presented
            }
            
            selectedList = "Teacherlist";
            return View(listOfTeachers);
        }



        public ActionResult StudentList()
        {
            ApplicationDbContext newDbContext = new ApplicationDbContext();
            //skapa en tom lista. fyll på med lämliga applicationusers i if-satserna
            List<UserViewModels> listOfUsers = new List<UserViewModels>();

            //lärare skall se alla lärare och elever nu visas alla lärare och elever oordnat i if-satsen nedan, måste städas upp.
            if (User.IsInRole("Teacher"))
            {
                //find student object in roles (with ID's), then get all students based on that
                var studentRoleInformation = newDbContext.Roles.FirstOrDefault(n => n.Name == "Student");
                var allStudents = newDbContext.Users.Where(u => u.Roles.FirstOrDefault().RoleId == studentRoleInformation.Id);

                //transfer over allStudents to be compatible with the UserViewModel
                foreach (var students in allStudents.ToList())
                {
                    var getCourseName = newDbContext.Courses.FirstOrDefault(c => c.Id == students.CourseId).Name;//students can only attend 1 course at a tiem

                    UserViewModels studentInSameCourse = new UserViewModels
                    {
                        Adress = students.Adress,
                        FirstName = students.FirstName,
                        //Id = int.Parse(user.Id) -denna går ju icke att parsa. låter vara orörd tills vidare
                        LastName = students.LastName,
                        Email = students.Email,
                        CourseName = getCourseName
                    };
                    listOfUsers.Add(studentInSameCourse); //add objects one by one to the list to be presented
                }
            } else if (User.IsInRole("Student"))
            {
                //    //studenter skall endast se (ev sin lärare) och sina klasskamrater. hämta studenten som är inloggads studentid
                var studentInSession = newDbContext.Users.FirstOrDefault(s => s.Email == User.Identity.Name);
                //    var studentInSession = db.Users.FirstOrDefault(s => s.Email == User.Identity.Name);
                var studentInSessionCourseId = studentInSession.CourseId;

                foreach (var user in newDbContext.Users.ToList())
                {
                    if (studentInSessionCourseId == user.CourseId)
                    {
                        var getCourseName = newDbContext.Courses.FirstOrDefault(c => c.Id == user.CourseId).Name;//students can only attend 1 course at a tiem
                                                                                                                 //för över till viewmodeln från gamla
                        UserViewModels studenInSameCourse = new UserViewModels
                        {
                            Adress = user.Adress,
                            FirstName = user.FirstName,
                            //Id = int.Parse(user.Id),
                            LastName = user.LastName,
                            Email = user.Email,
                            CourseName = getCourseName
                        };
                        listOfUsers.Add(studenInSameCourse);
                    }
                }
            }
            selectedList = "StudentList";//sätt global variabel till studentlist så redirect blir korrekt
            return View(listOfUsers); //returnera case user är student
        }


        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [Authorize(Roles = "Teacher")]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [Authorize(Roles = "Teacher")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model, string save1, string saveMultiple)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, FirstName = model.FirstName, LastName = model.LastName, Adress = model.Adress };
                var result = await UserManager.CreateAsync(user, model.Password);
                var teacher = UserManager.FindByName(user.Email); //new
                UserManager.AddToRole(teacher.Id, "Teacher"); //new
                if (result.Succeeded)
                {
                    //await SignInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false);

                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    //return RedirectToAction("Index", "Home");

                    //send ok to client
                    TempData["Event"] = teacher.FullName + " added to LMS.";
                    ModelState.Clear();

                    //redirect to right place depending on button chosen
                    if (save1 != null) return RedirectToAction("TeacherList");
                    if (saveMultiple != null) return RedirectToAction("Register");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // GET: /Account/RegisterStudent
        [Authorize(Roles = "Teacher")]
        public ActionResult RegisterStudent(int courseId)
        {
            var db = new ApplicationDbContext();
            var courseName = db.Courses.Find(courseId)?.Name;
            if (courseName == null)
            {
                return View("Error");
            }
            ViewBag.CourseId = courseId;
            ViewBag.CourseName = courseName;
            return View();
        }
        // POST: /Account/RegisterStudent
        [HttpPost]
        [Authorize(Roles = "Teacher")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterStudent(RegisterViewModel model,string save1, string saveMultiple)
        {
            var db = new ApplicationDbContext();
            var courseName = db.Courses.Find(int.Parse(model.CourseId))?.Name;
            if (courseName == null)
            {
                return View("Error");
            }
            ViewBag.CourseName = courseName;
            ViewBag.CourseId = model.CourseId;

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, FirstName = model.FirstName, LastName = model.LastName, Adress = model.Adress, CourseId = int.Parse(model.CourseId) };
                var result = await UserManager.CreateAsync(user, model.Password);
                var student = UserManager.FindByName(user.Email); //new
                UserManager.AddToRole(student.Id, "Student"); //new
                if (result.Succeeded)
                {
                    //await SignInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false);

                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    ModelState.Clear();
                    //send ok message to client
                    TempData["Event"] = student.FullName + " added to course.";
                    if (save1 != null) return RedirectToAction("Details","Courses", new {id=model.CourseId}); //här skall teacher redirectas till rätt coursedetail course
                    if (saveMultiple != null) return RedirectToAction("RegisterStudent",new { model.CourseId});//här skall teacher redirectas till RegisterStudent med kursid

                    return View();
                }
                AddErrors(result);
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login", "Account");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Courses");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}