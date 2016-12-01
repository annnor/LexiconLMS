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

       [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        // XXX Nedanstående funkar ej... Då visas inte existerande värdet i Edit-fönstret, utan 'dd/mm/yyyy       
        //[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }
        public string Description { get; set; }

        public virtual ICollection<ApplicationUser> Students { get; set; }
    }
}