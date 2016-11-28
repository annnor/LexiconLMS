namespace LexiconLMS.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<LexiconLMS.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(LexiconLMS.Models.ApplicationDbContext context)
        {

            // --------------- Roles -------------------------------------

            var roleStore = new RoleStore<IdentityRole>(context);
            var roleManager = new RoleManager<IdentityRole>(roleStore);

            if (!context.Roles.Any(r=>r.Name=="Teacher")) 
                {
                var role = new IdentityRole { Name = "Teacher" };
                var result = roleManager.Create(role);
                if (!result.Succeeded) 
                    {
                    throw new Exception(string.Join("\n", result.Errors));
                }
            }
            
            if (!context.Roles.Any(r=>r.Name=="Student")) 
                {
                var role= new IdentityRole { Name="Student"};
                var result = roleManager.Create(role);
                if (!result.Succeeded) {
                    throw new Exception(string.Join("\n", result.Errors));
                }
            }

            // --------------- Teachers -------------------------------------

            var userStore = new UserStore<ApplicationUser>(context);
            var userManager = new UserManager<ApplicationUser>(userStore);

            var emailString = "Anna.Larare@lexicon.se";

            if (!context.Users.Any(u => u.UserName == emailString)) {
                var user = new ApplicationUser {
                    FirstName = "Anna",
                    LastName = "Lärare",
                    Email = emailString,
                    UserName = emailString,
                    Adress = "Björkhagsvägen 1" };
                
                var result = userManager.Create(user,"Abc123!");
                if (!result.Succeeded) 
                    {
                    throw new Exception(string.Join("\n", result.Errors));
                    }
            }
            var teacher = userManager.FindByName(emailString);
            userManager.AddToRole(teacher.Id, "Teacher");

            // --------------- Courses -------------------------------------
            var courses = new Course[] {
                new Course {
                    Name = ".Net HT16",
                    StartDateTime = new DateTime(2016, 08, 29, 10, 0, 0),
                    Description = "DotNet utveckling och frontend."
                },
                new Course {
                    Name = "It-tekniker",
                    StartDateTime = new DateTime(2016, 08, 29, 13, 0, 0),
                    Description = "Utbildning för potentiella it-tekniker."
                }
            };
            context.Courses.AddOrUpdate(c => c.Name, courses);
            context.SaveChanges();

            // --------------- Students -------------------------------------

            var studentEmailString = "annika.nordqvist@lexicon.se";

            if (!context.Users.Any(u => u.UserName == studentEmailString)) {

                var user = new ApplicationUser {
                    FirstName = "Annika",
                    LastName = "Nordqvist",
                    Email = studentEmailString,
                    UserName = studentEmailString,
                    Adress = "Björkhagsvägen 2",
                    CourseId = courses[0].Id
                };
                var result = userManager.Create(user, "Abc123!"); 
                if (!result.Succeeded) {
                    throw new Exception(string.Join("\n", result.Errors));
                }
            }
            var student1 = userManager.FindByName(studentEmailString);
            userManager.AddToRole(student1.Id, "Student");
        }
    }
}
