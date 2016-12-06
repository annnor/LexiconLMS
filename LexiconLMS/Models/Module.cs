using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LexiconLMS.Models
{
    public class Module
    {
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }
        public virtual Course Course { get; }

        [Required]
        [Display(Name = "Module Name")]
        public string Name { get; set; }

        [Required]
        [DisplayFormat(ApplyFormatInEditMode = true,
            DataFormatString = "{0:yyyy-MM-dd HH\\:mm}")]
        [Display(Name = "Start time")]
        public DateTime StartDateTime { get; set; }

        [Required]
        [DisplayFormat(ApplyFormatInEditMode = true,
            DataFormatString = "{0:yyyy-MM-dd HH\\:mm}")]
        [Display(Name = "End time")]
        public DateTime EndDateTime { get; set; }

        public string Description { get; set; }

        public virtual ICollection<Activity> Activities { get; set; }
    }
}