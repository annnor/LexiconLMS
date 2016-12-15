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
                    StartDate = new DateTime(2016, 10, 31),
                    Description = "DotNet utveckling och frontend."
                },
                new Course {
                    Name = "It-tekniker",
                    StartDate = new DateTime(2016, 08, 29),
                    Description = "Utbildning för potentiella it-tekniker."
                },
                new Course {
                    Name = "Java4W",
                    StartDate = new DateTime(2016, 08, 29),
                    Description = "Java-utveckling för tjejer."
                }
            };

            int courseNetHt16 = 0;

            context.Courses.AddOrUpdate(c => c.Name, courses);
            context.SaveChanges();

            int amountOfCourses = courses.Length;

            // --------------- Modules -------------------------------------
            // -------- till kursen '.Net HT16'
            var modules = new Module[] {
                // Index 0
                new Module {
                    Name = "C#",
                    StartDateTime = new DateTime(2016, 10, 29, 8, 30, 0),
                    EndDateTime = new DateTime(2016, 11, 16, 17, 0, 0),
                    Description = "Programmering i C#.",
                    CourseId = courses[courseNetHt16].Id
                },
                // Index 1
                new Module {
                    Name = "MVC",
                    StartDateTime = new DateTime(2016, 11, 19, 09, 0, 0),
                    EndDateTime = new DateTime(2016, 11, 30, 17, 0, 0),
                    Description = "DotNet utveckling och frontend.",
                    CourseId = courses[courseNetHt16].Id
                },
                // Index 2
                new Module {
                    Name = "Bootstrap",
                    StartDateTime = new DateTime(2016, 12, 01, 8, 0, 0),
                    EndDateTime = new DateTime(2016, 12, 02, 17, 0, 0),
                    Description = "Responsiva webb-sidor.",
                    CourseId = courses[courseNetHt16].Id
                },
                // Index 3
                new Module {
                    Name = "App-utveckling",
                    StartDateTime = new DateTime(2016, 12, 5, 8, 0, 0),
                    EndDateTime = new DateTime(2016, 12, 9, 17, 0, 0),
                    Description = "UX-design, AngularJS, Client vs Server",
                    CourseId = courses[courseNetHt16].Id
                },
                // Index 4
                new Module {
                    Name = "Testning",
                    StartDateTime = new DateTime(2016, 12, 12, 8, 0, 0),
                    EndDateTime = new DateTime(2016, 12, 16, 16, 0, 0),
                    Description = "Allmänt om test. Möjlighet till ISTQB certifiering",
                    CourseId = courses[courseNetHt16].Id
                },
                // Index 5
                new Module {
                    Name = "MVC fördjupning",
                    StartDateTime = new DateTime(2017, 1, 9, 8, 0, 0),
                    EndDateTime = new DateTime(2017, 2, 3, 17, 0, 0),
                    Description = "Projekt enligt Scrum",
                    CourseId = courses[courseNetHt16].Id
                },
            // -------- till kursen 'It-tekniker'
                // Index 6
                new Module {
                    Name = "Projektledning",
                    StartDateTime = new DateTime(2016, 08, 29, 10, 0, 0),
                    EndDateTime = new DateTime(2016, 09, 16, 15, 0, 0),
                    Description = "Metodik och projektstyrning.",
                    CourseId = courses[1].Id
                },
                // Index 7
                new Module {
                    Name = "Office365",
                    StartDateTime = new DateTime(2016, 09, 18, 09, 0, 0),
                    EndDateTime = new DateTime(2016, 09, 30, 15, 0, 0),
                    Description = "Administera offfice365.",
                    CourseId = courses[1].Id
                },
                // Index 8
                new Module {
                    Name = "Sharepoint",
                    StartDateTime = new DateTime(2016, 10, 03, 09, 0, 0),
                    EndDateTime = new DateTime(2016, 10, 31, 15, 0, 0),
                    Description = "Introduktion till Sharepoint Server.",
                    CourseId = courses[1].Id
                },
            // -------- till kursen 'Java4W'
                // Index 9
                new Module {
                    Name = "Java",
                    StartDateTime = new DateTime(2016, 08, 29, 10, 0, 0),
                    EndDateTime = new DateTime(2016, 09, 16, 15, 0, 0),
                    Description = "Programmering i Java.",
                    CourseId = courses[2].Id
                },
                // Index 10
                new Module {
                    Name = "JavaBeans",
                    StartDateTime = new DateTime(2016, 09, 19, 09, 0, 0),
                    EndDateTime = new DateTime(2016, 09, 24, 15, 0, 0),
                    Description = "Utveckling med JavaBeans.",
                    CourseId = courses[2].Id
                },
                // Index 11
                new Module {
                    Name = "JUnit",
                    StartDateTime = new DateTime(2016, 10, 10, 09, 0, 0),
                    EndDateTime = new DateTime(2016, 10, 14, 15, 0, 0),
                    Description = "Test med JUnit.",
                    CourseId = courses[2].Id
                }

            };

            context.Modules.AddOrUpdate(m => m.Name, modules);
            context.SaveChanges();

            // --------------- Activity Types -------------------------------------

            var activityTypes = new ActivityType[] {
            // Index 0
                new ActivityType {
                    Name = "Föreläsning"
                },
            // Index 1
                new ActivityType {
                    Name = "E-learning"
                },
            // Index 2
                new ActivityType {
                    Name = "Övning"
                },
            // Index 3
                new ActivityType {
                    Name = "Tentamen"
                },
            // Index 4
                new ActivityType {
                    Name = "Projektarbete"
                },
            // Index 5
                new ActivityType {
                    Name = "Sprintredovisning"
                }
            };
            context.ActivityTypes.AddOrUpdate(t => t.Name, activityTypes);
            context.SaveChanges();

            // --------------- Activities -------------------------------------
            // ----------- till Module 'C#' (kurs .NET)
            // Index 0
            var activities = new Activity[] {
                new Activity {
                    Name = "C# Intro",
                    ActivityTypeId = activityTypes[0].Id, // Föreläsning
                    StartDateTime = new DateTime(2016, 10, 30, 8, 30, 0),
                    EndDateTime = new DateTime(2016, 10, 30, 17, 0, 0),
                    Description = "C# Intro - med Adrian",
                    ModuleId = modules[0].Id
                },
                // Index 1
                new Activity {
                    Name = "Intro + C# Fundamentals with Visual Studio 2015, Kap 1-2",
                    ActivityTypeId = activityTypes[1].Id, // E-learning
                    StartDateTime = new DateTime(2016, 10, 29, 13, 0, 0),
                    EndDateTime = new DateTime(2016, 10, 29, 17, 0, 0),
                    Description = "Scott Allens föreläsning: https://app.pluralsight.com/library/courses/c-sharp-fundamentals-with-visual-studio-2015/table-of-contents",
                    ModuleId = modules[0].Id
                },
                // Index 2
                new Activity {
                    Name = "C# Fundamentals with Visual Studio 2015, Kap 4-5",
                    ActivityTypeId = activityTypes[1].Id, // E-learning
                    StartDateTime = new DateTime(2016, 10, 31, 13, 0, 0),
                    EndDateTime = new DateTime(2016, 10, 31, 17, 0, 0),
                    Description = "Scott Allens föreläsning: https://app.pluralsight.com/library/courses/c-sharp-fundamentals-with-visual-studio-2015/table-of-contents",
                    ModuleId = modules[0].Id
                },
                // Index 3
                new Activity {
                    Name = "C# Fundamentals with Visual Studio 2015, Kap 3",
                    ActivityTypeId = activityTypes[1].Id, // E-learning
                    StartDateTime = new DateTime(2016, 10, 31, 8, 30, 0),
                    EndDateTime = new DateTime(2016, 10, 31, 12, 0, 0),
                    Description = "Scott Allens föreläsning: https://app.pluralsight.com/library/courses/c-sharp-fundamentals-with-visual-studio-2015/table-of-contents",
                    ModuleId = modules[0].Id
                },
                // Index 4
                new Activity {
                    Name = "C# Grunderna",
                    ActivityTypeId = activityTypes[0].Id, // Föreläsning
                    StartDateTime = new DateTime(2016, 11, 1, 8, 30, 0),
                    EndDateTime = new DateTime(2016, 11, 1, 17, 0, 0),
                    Description = "C# Grunderna - med Adrian",
                    ModuleId = modules[0].Id
                },
                // Index 5
                new Activity {
                    Name = "C# Fundamentals with Visual Studio 2015, Kap 6-7",
                    ActivityTypeId = activityTypes[1].Id, // E-learning
                    StartDateTime = new DateTime(2016, 11, 2, 8, 30, 0),
                    EndDateTime = new DateTime(2016, 11, 2, 12, 0, 0),
                    Description = "Scott Allens föreläsning: https://app.pluralsight.com/library/courses/c-sharp-fundamentals-with-visual-studio-2015/table-of-contents",
                    ModuleId = modules[0].Id
                },
                // Index 6
                new Activity {
                    Name = "C# Fundamentals with Visual Studio 2015, Kap 8",
                    ActivityTypeId = activityTypes[1].Id, // E-learning
                    StartDateTime = new DateTime(2016, 11, 2, 13, 0, 0),
                    EndDateTime = new DateTime(2016, 11, 4, 17, 0, 0),
                    Description = "Scott Allens föreläsning: https://app.pluralsight.com/library/courses/c-sharp-fundamentals-with-visual-studio-2015/table-of-contents",
                    ModuleId = modules[0].Id
                },
                // Index 7
                new Activity {
                    Name = "C# certification exam. Supervised by Tony Montana.",
                    ActivityTypeId = activityTypes[3].Id, // Tentamen
                    StartDateTime = new DateTime(2016, 11, 16, 09, 0, 0),
                    EndDateTime = new DateTime(2016, 11, 16, 17, 0, 0),
                    Description = "The exam is 8 hours maximum. Lunch is not allowed. You are permitted to bring smaller foodstuffs such as chocolate, water etc.",
                    ModuleId = modules[0].Id
                },
            // ----------- till Module 'MVC' (kurs .NET)
                new Activity {
                    Name = "Exploring great patterns with Johan Sari.",
                    ActivityTypeId = activityTypes[1].Id, // E-learning
                    StartDateTime = new DateTime(2016, 11, 19, 09, 0, 0),
                    EndDateTime = new DateTime(2016, 11, 19, 17, 0, 0),
                    Description = "En mästerful e-learning av mästaren.",
                    ModuleId = modules[1].Id
                },
                new Activity {
                    Name = "Front End and You with Dimitris.",
                    ActivityTypeId = activityTypes[0].Id, // Föreläsning
                    StartDateTime = new DateTime(2016, 11, 22, 09, 0, 0),
                    EndDateTime = new DateTime(2016, 11, 28, 17, 0, 0),
                    Description = "Hur man designar knappar och pilar för HTML.",
                    ModuleId = modules[1].Id
                 },
                new Activity {
                    Name = "Iffies rules the world, JavaScript with Tony Granato.",
                    ActivityTypeId = activityTypes[1].Id, // E-learning
                    StartDateTime = new DateTime(2016, 11, 29, 09, 0, 0),
                    EndDateTime = new DateTime(2016, 11, 30, 17, 0, 0),
                    Description = "The anonymous Javascript function and it's uses in a bigger context.",
                    ModuleId = modules[1].Id
                },
            // ----------- till Module 'Bootstrap' (kurs .NET)
                new Activity {
                    Name = "Bootstrap foundations with Dr Drowzee.",
                    ActivityTypeId = activityTypes[1].Id, // E-learning
                    StartDateTime = new DateTime(2016, 12, 1, 8, 0, 0),
                    EndDateTime = new DateTime(2016, 12, 1, 17, 0, 0),
                    Description = "Introduction to the Bootstrap concept.",
                    ModuleId = modules[2].Id
                },
                new Activity {
                    Name = "Badges and animations for the handy coder.",
                    ActivityTypeId = activityTypes[0].Id, // Föreläsning
                    StartDateTime = new DateTime(2016, 12, 2, 8, 0, 0),
                    EndDateTime = new DateTime(2016, 12, 2, 17, 0, 0),
                    Description = "Get a job by designing buttons and stuff. And it's fun, too!!.",
                    ModuleId = modules[2].Id
                },

                  //  ----------- till Module 'App-utveckling' (kurs .NET)
                new Activity {
                    Name = "XXX E-L 9XXX",
                    ActivityTypeId = activityTypes[1].Id, // E-learning
                    StartDateTime = new DateTime(2016, 12, 5, 8, 0, 0),
                    EndDateTime = new DateTime(2016, 12, 5, 12, 0, 0),
                    Description = "",
                    ModuleId = modules[3].Id
                },
                new Activity {
                    Name = "UX-övning",
                    ActivityTypeId = activityTypes[1].Id, // Övning
                    StartDateTime = new DateTime(2016, 12, 5, 13, 0, 0),
                    EndDateTime = new DateTime(2016, 12, 5, 17, 0, 0),
                    Description = "",
                    StudentUpload = true,
                    ModuleId = modules[3].Id
                },
                new Activity {
                    Name = "UX",
                    ActivityTypeId = activityTypes[0].Id, // Föreläsning
                    StartDateTime = new DateTime(2016, 12, 6, 8, 0, 0),
                    EndDateTime = new DateTime(2016, 12, 6, 12, 0, 0),
                    Description = "Föreläsning med Adrian",
                    ModuleId = modules[3].Id
                },
                new Activity {
                    Name = "Övning 16",
                    ActivityTypeId = activityTypes[1].Id, // Övning
                    StartDateTime = new DateTime(2016, 12, 6, 13, 0, 0),
                    EndDateTime = new DateTime(2016, 12, 6, 17, 0, 0),
                    Description = "Adrian närvarande",
                    StudentUpload = true,
                    ModuleId = modules[3].Id
                },
                new Activity {
                    Name = "Angular JS",
                    ActivityTypeId = activityTypes[1].Id, // E-learning
                    StartDateTime = new DateTime(2016, 12, 7, 8, 0, 0),
                    EndDateTime = new DateTime(2016, 12, 9, 12, 0, 0),
                    Description = "XXX E-learning XXX. Adrian på plats.",
                    ModuleId = modules[3].Id
                },
                new Activity {
                    Name = "Client vs Server",
                    ActivityTypeId = activityTypes[0].Id, // Föreläsning
                    StartDateTime = new DateTime(2016, 12, 9, 13, 0, 0),
                    EndDateTime = new DateTime(2016, 12, 9, 17, 0, 0),
                    Description = "Adrian föreläser",
                    ModuleId = modules[3].Id
                },
                  //  ----------- till Module 'Testning' (kurs .NET)
                new Activity {
                    Name = "ISTQB",
                    ActivityTypeId = activityTypes[1].Id, // E-learning
                    StartDateTime = new DateTime(2016, 12, 12, 8, 0, 0),
                    EndDateTime = new DateTime(2016, 12, 12, 17, 0, 0),
                    Description = "",
                    ModuleId = modules[4].Id
                },
                new Activity {
                    Name = "TDD",
                    ActivityTypeId = activityTypes[1].Id, // E-learning
                    StartDateTime = new DateTime(2016, 12, 13, 8, 0, 0),
                    EndDateTime = new DateTime(2016, 12, 13, 17, 0, 0),
                    Description = "Test Driven Development  XXX E-L XXX",
                    ModuleId = modules[4].Id
                },
                new Activity {
                    Name = "ISTQB",
                    ActivityTypeId = activityTypes[0].Id, // Föreläsning
                    StartDateTime = new DateTime(2016, 12, 14, 8, 0, 0),
                    EndDateTime = new DateTime(2016, 12, 16, 15, 0, 0),
                    Description = "Föreläsning inför certifiering.",
                    ModuleId = modules[4].Id
                },
                /*
                new Activity {
                    Name = "ISTQB Certifiering",
                    ActivityTypeId = activityTypes[3].Id, // Tentamen
                    StartDateTime = new DateTime(2016, 12, 16, 15, 30, 0),
                    EndDateTime = new DateTime(2016, 12, 16, 17, 0, 0),
                    Description = "Man får endast ha med sig dokumentet 'ISTQB Testtermer' på skrivningen.",
                    StudentUpload = true,
                    ModuleId = modules[4].Id
                },
    */
                  //  ----------- till Module 'MVC fördjupning' (kurs .NET)
                  
                new Activity {
                    Name = "SCRUM",
                    ActivityTypeId = activityTypes[0].Id, // Föreläsning
                    StartDateTime = new DateTime(2017, 1, 9, 8, 0, 0),
                    EndDateTime = new DateTime(2017, 1, 9, 17, 0, 0),
                    Description = "Föreläsning med Adrian",
                    ModuleId = modules[5].Id
                },
                new Activity {
                    Name = "Projektplanering",
                    ActivityTypeId = activityTypes[4].Id, // Projektarbete
                    StartDateTime = new DateTime(2017, 1, 10, 8, 0, 0),
                    EndDateTime = new DateTime(2017, 1, 12, 12, 0, 0),
                    Description = "Indelning i projektgrupper med 3-4 medlemmar.",
                    ModuleId = modules[5].Id
                },

                new Activity {
                    Name = "Planering sprint 1",
                    ActivityTypeId = activityTypes[4].Id, // Projektarbete
                    StartDateTime = new DateTime(2017, 1, 12, 13, 0, 0),
                    EndDateTime = new DateTime(2017, 1, 12, 17, 0, 0),
                    Description = "Start på första sprinten. En vecka lång sprint.",
                    ModuleId = modules[5].Id
                },
                new Activity {
                    Name = "Projekt sprint 1",
                    ActivityTypeId = activityTypes[4].Id, // Projektarbete
                    StartDateTime = new DateTime(2017, 1, 12, 8, 0, 0),
                    EndDateTime = new DateTime(2016, 1, 17, 17, 0, 0),
                    Description = "Arbete enl scrum, med scrummöten.",
                    ModuleId = modules[5].Id
                },
                new Activity {
                    Name = "Sprintredovisning 1",
                    ActivityTypeId = activityTypes[4].Id, // Projektarbete
                    StartDateTime = new DateTime(2017, 1, 18, 8, 0, 0),
                    EndDateTime = new DateTime(2017, 1, 18, 12, 0, 0),
                    Description = "Varje grupp redovisar de nya fungerande features som tillkommit under senaste sprinten",
                    ModuleId = modules[5].Id
                },
                new Activity {
                    Name = "Retrospektiv. Planering sprint 2",
                    ActivityTypeId = activityTypes[4].Id, // Projektarbete
                    StartDateTime = new DateTime(2017, 1, 18, 13, 0, 0),
                    EndDateTime = new DateTime(2017, 1, 18, 17, 0, 0),
                    Description = "Start på andra sprinten. En vecka lång sprint.",
                    ModuleId = modules[5].Id
                },
                new Activity {
                    Name = "Projekt sprint 2",
                    ActivityTypeId = activityTypes[4].Id, // Projektarbete
                    StartDateTime = new DateTime(2017, 1, 19, 8, 0, 0),
                    EndDateTime = new DateTime(2017, 1, 24, 17, 0, 0),
                    Description = "Arbete enl scrum, med scrummöten.",
                    ModuleId = modules[5].Id
                },
                new Activity {
                    Name = "Sprintredovisning 2",
                    ActivityTypeId = activityTypes[4].Id, // Projektarbete
                    StartDateTime = new DateTime(2017, 1, 25, 8, 0, 0),
                    EndDateTime = new DateTime(2017, 1, 25, 13, 0, 0),
                    Description = "Varje grupp redovisar de nya fungerande features som tillkommit under senaste sprinten",
                    ModuleId = modules[5].Id
                },
                new Activity {
                    Name = "Retrospektiv. Planering sprint 3",
                    ActivityTypeId = activityTypes[4].Id, // Projektarbete
                    StartDateTime = new DateTime(2017, 1, 25, 13, 0, 0),
                    EndDateTime = new DateTime(2017, 1, 25, 17, 0, 0),
                    Description = "Start på tredje sprinten. En vecka lång sprint.",
                    ModuleId = modules[5].Id
                },
                new Activity {
                    Name = "Projekt sprint 3",
                    ActivityTypeId = activityTypes[4].Id, // Projektarbete
                    StartDateTime = new DateTime(2017, 1, 26, 8, 0, 0),
                    EndDateTime = new DateTime(2017, 1, 31, 17, 0, 0),
                    Description = "Arbete enl scrum, med scrummöten.",
                    ModuleId = modules[5].Id
                },
                new Activity {
                    Name = "Sprintredovisning 3",
                    ActivityTypeId = activityTypes[4].Id, // Projektarbete
                    StartDateTime = new DateTime(2017, 2, 1, 8, 0, 0),
                    EndDateTime = new DateTime(2017, 2, 1, 13, 0, 0),
                    Description = "Varje grupp redovisar de nya fungerande features som tillkommit under senaste sprinten",
                    ModuleId = modules[5].Id
                },
                new Activity {
                    Name = "Förberedelse inför avslutande presentation",
                    ActivityTypeId = activityTypes[4].Id, // Projektarbete
                    StartDateTime = new DateTime(2017, 2, 1, 13, 0, 0),
                    EndDateTime = new DateTime(2017, 2, 2, 17, 0, 0),
                    Description = "Varje grupp redovisar de nya fungerande features som tillkommit under senaste sprinten",
                    ModuleId = modules[5].Id
                },
                new Activity {
                    Name = "Presentation av projektarbete",
                    ActivityTypeId = activityTypes[4].Id, // Projektarbete
                    StartDateTime = new DateTime(2017, 2, 3, 8, 0, 0),
                    EndDateTime = new DateTime(2017, 2, 3, 17, 0, 0),
                    Description = "Varje grupp presenterar sin applikation, hur den fungerar och bakomliggande kod.",
                    ModuleId = modules[5].Id
                },
    
            // ----------- till Module 'Office365' (kurs 'It-tekniker')
                new Activity {
                    Name = "Office365 for dummies.",
                    ActivityTypeId = activityTypes[1].Id, // E-learning
                    StartDateTime = new DateTime(2016, 09, 18, 09, 0, 0),
                    EndDateTime = new DateTime(2016, 09, 18, 17, 0, 0),
                    Description = "Introduction to Office365 as it simpliest.",
                    ModuleId = modules[7].Id
                },
            // ----------- till Module 'Sharepoint' (kurs 'It-tekniker')
                new Activity {
                    Name = "Advanced Sharepoint and Web Parts.",
                    ActivityTypeId = activityTypes[1].Id, // E-learning
                    StartDateTime = new DateTime(2016, 10, 01, 09, 0, 0),
                    EndDateTime = new DateTime(2016, 10, 14, 17, 0, 0),
                    Description = "Programming web parts.",
                    ModuleId = modules[8].Id
                },
            // ----------- till Module 'Projektledning' (kurs 'It-tekniker')
                new Activity {
                    Name = "Projektledning.",
                    ActivityTypeId = activityTypes[0].Id, // Föreläsning
                    StartDateTime = new DateTime(2016, 08, 29, 09, 0, 0),
                    EndDateTime = new DateTime(2016, 09, 14, 17, 0, 0),
                    Description = "Planera ditt projekt.",
                    ModuleId = modules[6].Id
                },
                new Activity {
                    Name = "Förvaltningsmodeller.",
                    ActivityTypeId = activityTypes[3].Id, // Tentamen
                    StartDateTime = new DateTime(2016, 09, 15, 09, 0, 0),
                    EndDateTime = new DateTime(2016, 09, 15, 17, 0, 0),
                    Description = "Grundläggande kunkaper för att skapa en förvaltningsorganinsation.",
                    ModuleId = modules[6].Id
                },
            // ----------- till Module 'Java' (kursen 'Java4W')
                new Activity {
                 Name = "Java programming with Tony Granato.",
                    ActivityTypeId = activityTypes[1].Id, // E-learning
                    StartDateTime = new DateTime(2016, 08, 29, 09, 0, 0),
                    EndDateTime = new DateTime(2016, 09, 07, 17, 0, 0),
                    Description = "The anonymous Javascript function and it's uses in a bigger context.",
                    ModuleId = modules[9].Id
                },
            // ----------- till Module 'JavaBeans' (kursen 'Java4W')
                new Activity {
                    Name = "JavaBeans foundations with Dr Drowzee.",
                    ActivityTypeId = activityTypes[1].Id, // E-learning
                    StartDateTime = new DateTime(2016, 09, 19, 09, 0, 0),
                    EndDateTime = new DateTime(2016, 09, 23, 17, 0, 0),
                    Description = "Introduction to the JavaBeans concept.",
                    ModuleId = modules[10].Id
                },
            // ----------- till Module 'JUnit' (kursen 'Java4W')
                new Activity {
                    Name = "Introduction to JUnit.",
                    ActivityTypeId = activityTypes[0].Id, // Föreläsning
                    StartDateTime = new DateTime(2016, 10, 10, 09, 0, 0),
                    EndDateTime = new DateTime(2016, 10, 13, 17, 0, 0),
                    Description = "Write your first test with JUnit.",
                    ModuleId = modules[11].Id
                },
                new Activity {
                    Name = "Continuous integration with jenkins.",
                    ActivityTypeId = activityTypes[3].Id, // Tentamen
                    StartDateTime = new DateTime(2016, 10, 14, 09, 0, 0),
                    EndDateTime = new DateTime(2016, 10, 14, 17, 0, 0),
                    Description = "Apply continuous integration to your project.",
                    ModuleId = modules[11].Id
                }
            };
            foreach (var v in activities)
            {
                context.Activities.AddOrUpdate(a => new { a.Name, a.ActivityTypeId },v);
            }
            context.SaveChanges();

            // --------------- Files ----------------------------------------
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
