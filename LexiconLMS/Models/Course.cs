using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LexiconLMS.Models
{
    public class Course
    {
        public int? Id { get; set; }
        [Display(Name = "Course Name")]
        public string Name { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd hh\\:mm}")]
        [Display(Name = "Start time")]
        public DateTime StartDateTime { get; set; }
        public string Description { get; set; }

        public virtual ICollection<AppUser> Students { get; set; }
    }
}