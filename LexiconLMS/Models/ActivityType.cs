using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LexiconLMS.Models
{
    public class ActivityType
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(40)]
        [Index(IsUnique = true)]
        public string Name { get; set; }

 //       public virtual ICollection<Activity> activities { get; }  // XXXannika Har vi behov av set här?

    }
}