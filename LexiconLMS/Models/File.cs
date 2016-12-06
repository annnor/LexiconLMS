using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LexiconLMS.Models
{
    public class File
    {
        public int Id { get; set; }

        [StringLength(255)]
        public string FileName { get; set; }

        [StringLength(255)]
        public string Description { get; set; }

        public string Path { get; set; }

        bool PubliclyVisible { get; set; }

        //these 3 fields below are used to to determine where a document belongs. they are set when a file is uploaded. they are get when a file will be shown.
        public int? CourseId { get; set; }
        public int? ModuleId { get; set; }
        public int? ActivityId { get; set; }

        [StringLength(100)]
        public string ContentType { get; set; }

        public byte[] Content { get; set; }

        public FileType FileType { get; set; }

        //samma resonemang här, ska jag ta en virtual applicationuser?
        public virtual ApplicationUser User { get; set; }
    }
}