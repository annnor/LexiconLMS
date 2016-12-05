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

            var teacherRole = "Teacher";
            var studentRole = "Student";
            foreach (var role in new[] { teacherRole, studentRole })
            {
                if (!context.Roles.Any(r => r.Name == role))
                {
                    var roleObj = new IdentityRole { Name = role };
                    var result = roleManager.Create(roleObj);
                    if (!result.Succeeded)
                    {
                        throw new Exception(string.Join("\n", result.Errors));
                    }
                }
            }
            // --------------- Teachers -------------------------------------

            var userStore = new UserStore<ApplicationUser>(context);
            var userManager = new UserManager<ApplicationUser>(userStore);

            string[,] teachers = {
                { "Anna", "Larnia"},
                { "Bo", "Larka"},
                { "Curt", "Schola"},
                { "Bert", "Bengtsen"},
                { "Sven", "Svala"},
                { "Aron", "Arlof"},
                { "Frank", "Fidell"},
                { "Franco", "Mills"},
                { "Markus", "Thalen"},
                { "Tore", "Bertzen"},
                { "Arvid", "Fidell"},
                { "Katarina", "Mills"},
                { "Paula", "Thalen"},
                { "Nils", "Bertzen"},
                { "Thore", "Fridell"},
                { "Lars", "Bills"},
                { "Markus", "Agarth"},
                { "Karl", "Bertzen"}
            };
            int upperBound = teachers.GetUpperBound(0);
            for (int i = 0; i <= upperBound; i++)
            {
                var firstName = teachers[i, 0];
                var lastName = teachers[i, 1];
                var email = firstName + "." + lastName + "@lexicon.se";
                var streetNo = i + 1;
                if (!context.Users.Any(u => u.UserName == email))
                {
                    var user = new ApplicationUser
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        Email = email,
                        UserName = email,
                        Adress = "Malmövägen " + streetNo
                    };

                    var result = userManager.Create(user, "Abc123!");
                    if (!result.Succeeded)
                    {
                        throw new Exception(string.Join("\n", result.Errors));
                    }
                }
                var teacher = userManager.FindByName(email);
                userManager.AddToRole(teacher.Id, teacherRole);
            }

            // --------------- Courses -------------------------------------
            var courses = new Course[] {
                new Course {
                    Name = ".Net HT16",
                    StartDate = new DateTime(2016, 08, 29, 10, 0, 0),
                    Description = "DotNet utveckling och frontend."
                },
                new Course {
                    Name = "It-tekniker",
                    StartDate = new DateTime(2016, 08, 29, 13, 0, 0),
                    Description = "Utbildning för potentiella it-tekniker."
                },
                new Course {
                    Name = "Java4W",
                    StartDate = new DateTime(2016, 08, 29, 13, 0, 0),
                    Description = "Java-utveckling för tjejer."
                }
            };
            context.Courses.AddOrUpdate(c => c.Name, courses);
            context.SaveChanges();

            int amountOfCourses = courses.Length;

            // --------------- Modules -------------------------------------
            var modules = new Module[] {
                new Module {
                    Name = "C#",
                    StartDateTime = new DateTime(2016, 08, 29, 10, 0, 0),
                    EndDateTime = new DateTime(2016, 09, 16, 15, 0, 0),
                    Description = "Programmering i C#.",
                    CourseId = courses[0].Id
                },
                new Module {
                    Name = "MVC",
                    StartDateTime = new DateTime(2016, 09, 19, 09, 0, 0),
                    EndDateTime = new DateTime(2016, 09, 30, 15, 0, 0),
                    Description = "DotNet utveckling och frontend.",
                    CourseId = courses[0].Id
                },
                new Module {
                    Name = "Bootstrap",
                    StartDateTime = new DateTime(2016, 10, 02, 09, 0, 0),
                    EndDateTime = new DateTime(2016, 10, 06, 15, 0, 0),
                    Description = "Responsiva webb-sidonr.",
                    CourseId = courses[0].Id
                }
            };
            context.Modules.AddOrUpdate(m => m.Name, modules);
            context.SaveChanges();

            // --------------- Activity Types -------------------------------------

            var activityTypes = new ActivityType[] {
                new ActivityType {
                    Name = "Föreläsning"
                },
                new ActivityType {
                    Name = "E-learning"
                },
                new ActivityType {
                    Name = "Övning"
                },
                new ActivityType {
                    Name = "Tentamen"
                }
            };
            context.ActivityTypes.AddOrUpdate(t => t.Name, activityTypes);
            context.SaveChanges();

            // --------------- Activities -------------------------------------

            var activities = new Activity[] {
                new Activity {
                    Name = "C# Intro",
                    ActivityTypeId = activityTypes[0].Id, // Föreläsning
                    StartDateTime = new DateTime(2016, 08, 30, 8, 30, 0),
                    EndDateTime = new DateTime(2016, 08, 30, 17, 0, 0),
                    Description = "C# Intro - med Adrian",
                    ModuleId = modules[0].Id
                },
                new Activity {
                    Name = "Intro + C# Fundamentals with Visual Studio 2015, Kap 1-2",
                    ActivityTypeId = activityTypes[1].Id, // E-learning
                    StartDateTime = new DateTime(2016, 08, 29, 13, 0, 0),
                    EndDateTime = new DateTime(2016, 08, 29, 17, 0, 0),
                    Description = "Scott Allens föreläsning: https://app.pluralsight.com/library/courses/c-sharp-fundamentals-with-visual-studio-2015/table-of-contents",
                    ModuleId = modules[0].Id
                },
                new Activity {
                    Name = "C# Fundamentals with Visual Studio 2015, Kap 4-5",
                    ActivityTypeId = activityTypes[1].Id, // E-learning
                    StartDateTime = new DateTime(2016, 08, 31, 13, 0, 0),
                    EndDateTime = new DateTime(2016, 08, 31, 17, 0, 0),
                    Description = "Scott Allens föreläsning: https://app.pluralsight.com/library/courses/c-sharp-fundamentals-with-visual-studio-2015/table-of-contents",
                    ModuleId = modules[0].Id
                },
                new Activity {
                    Name = "C# Fundamentals with Visual Studio 2015, Kap 3",
                    ActivityTypeId = activityTypes[1].Id, // E-learning
                    StartDateTime = new DateTime(2016, 08, 31, 8, 30, 0),
                    EndDateTime = new DateTime(2016, 08, 31, 12, 0, 0),
                    Description = "Scott Allens föreläsning: https://app.pluralsight.com/library/courses/c-sharp-fundamentals-with-visual-studio-2015/table-of-contents",
                    ModuleId = modules[0].Id
                },
                new Activity {
                    Name = "C# Grunderna",
                    ActivityTypeId = activityTypes[0].Id, // Föreläsning
                    StartDateTime = new DateTime(2016, 9, 1, 8, 30, 0),
                    EndDateTime = new DateTime(2016, 9, 1, 17, 0, 0),
                    Description = "C# Grunderna - med Adrian",
                    ModuleId = modules[0].Id
                },
                new Activity {
                    Name = "C# Fundamentals with Visual Studio 2015, Kap 6-7",
                    ActivityTypeId = activityTypes[1].Id, // E-learning
                    StartDateTime = new DateTime(2016, 9, 2, 8, 30, 0),
                    EndDateTime = new DateTime(2016, 9, 2, 12, 0, 0),
                    Description = "Scott Allens föreläsning: https://app.pluralsight.com/library/courses/c-sharp-fundamentals-with-visual-studio-2015/table-of-contents",
                    ModuleId = modules[0].Id
                },
                new Activity {
                    Name = "C# Fundamentals with Visual Studio 2015, Kap 8",
                    ActivityTypeId = activityTypes[1].Id, // E-learning
                    StartDateTime = new DateTime(2016, 9, 2, 13, 0, 0),
                    EndDateTime = new DateTime(2016, 9, 2, 17, 0, 0),
                    Description = "Scott Allens föreläsning: https://app.pluralsight.com/library/courses/c-sharp-fundamentals-with-visual-studio-2015/table-of-contents",
                    ModuleId = modules[0].Id
                }
            };
            context.Activities.AddOrUpdate(a => a.Name, activities);
            context.SaveChanges();
            
            // --------------- Students -------------------------------------

            string[,] studentList = {
                { "Bengt", "Carlen" },
                { "Carl",  "Bengtsson"},
                { "Anna", "Andersson" },
                { "David", "Davidsson" },
                { "Erik", "Eriksson" },
                { "Anna", "Persson" },
                { "Eric", "Ericsson" },
                { "Fredrik", "Henriksson" },
                { "Andrea", "Persson" },
                { "Henrik", "Fredriksson" },
                { "Ivar", "Ivarsson" },
                { "Anneli", "Jansson" },
                { "Joakim", "Jarlen" },
                { "Karl", "Larsson" },
                { "Sofia", "Svensson" },
                { "Lars", "Karlsson" },
                { "Magnus", "Nilsson" },
                { "Lena", "Svensson" },
                { "Nils", "Magnusson" },
                { "Olof", "Persson" },
                { "Lena", "Karlsson" },
                { "Per", "Nilsson" },
                { "Quintus", "Magnusson" },
                { "Rosanna", "Paulsson" },
                { "Rolf", "Magnusson" },
                { "Stella", "Nilsson" },
                { "Eva", "Paulsson" },
                { "Thomas", "Magnusson" },
                { "Ulf", "Nilsson" },
                { "Evelina", "Gustavsson" },
                { "Viktor", "Magnusson" },
                { "Wiktor", "Nilsson" }
            };

            upperBound = studentList.GetUpperBound(0);
            for (int i = 0; i <= upperBound; i++)
            {
                // cIndex - index to use for courses list:
                var cIndex = i % amountOfCourses;

                var firstName = studentList[i, 0];
                var lastName = studentList[i, 1];
                var email = firstName + "." + lastName + "@lexicon.se";

                if (!context.Users.Any(u => u.UserName == email))
                {
                    var streetNr = i + 1;

                    var user = new ApplicationUser
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        Email = email,
                        UserName = email,
                        Adress = "Studentgatan " + streetNr,
                        CourseId = courses[cIndex].Id
                    };
                    var result = userManager.Create(user, "Abc123!");
                    if (!result.Succeeded)
                    {
                        throw new Exception(string.Join("\n", result.Errors));
                    }
                }
                var student = userManager.FindByName(email);
                userManager.AddToRole(student.Id, studentRole);
            }
        }
    }
}
