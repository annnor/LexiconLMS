using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LexiconLMS.Models
{
    public class UserViewModels
    {
        public string Id { get; set; }

        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Display(Name = "Last name")]
        public string LastName { get; set; }
        public string Adress { get; set; }
        public string Email { get; set; }

        [Display(Name = "Course name")]
        public string CourseName { get; set; }
    }
}