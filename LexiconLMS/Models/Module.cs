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

        [Required]
        public string Name { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd hh\\:mm}")]
        [Display(Name = "Start time")]
        public DateTime StartDateTime { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd hh\\:mm}")]
        [Display(Name = "End time")]
        public DateTime EndDateTime { get; set; }

        public string Description { get; set; }

        public virtual Course Course { get; }
    }
}