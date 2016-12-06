using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace LexiconLMS.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required]
        [Index(IsUnique = true)]
        [MaxLength(40)]
        [Display(Name = "Course Name")]
        public string Name { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        public virtual ICollection<ApplicationUser> Students { get; set; }
        public virtual ICollection<Module> Modules { get; set; }
    }
}