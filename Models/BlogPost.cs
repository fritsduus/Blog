using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Blog.Models
{
    public class BlogPost
    {
        public string Id { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime Created { get; set; }

        [Required]
        public string Title { get; set; }
        
        public string[] Tags { get; set; }

        [Required]
        public string Text { get; set; }
    }
}