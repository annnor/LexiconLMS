using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace LexiconLMS.Models
{
    public class Activity
    {
        public int Id { get; set; }

        [Required]
        public int ModuleId { get; set; }
        public virtual Module Module { get; set; }

        [Required]
        public int ActivityTypeId { get; set; }
        public virtual ActivityType Type { get; set; }

        [Required]
        [Display(Name = "Activity Name")]
        public string Name { get; set; }

        [Required]
        [DisplayFormat(ApplyFormatInEditMode = true,
            DataFormatString = "{0:yyyy-MM-dd HH\\:mm}")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Start Time")]
        public DateTime StartDateTime { get; set; }

        [Required]
        [DisplayFormat(ApplyFormatInEditMode = true, 
            DataFormatString = "{0:yyyy-MM-dd HH\\:mm}")]
        [DataType(DataType.DateTime)]
        [Display(Name = "End Time")]
        public DateTime EndDateTime { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        public virtual ICollection<File> Files { get; set; }

    }
}