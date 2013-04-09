using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Blog.Models
{
    public class TagCount
    {
        public string Tag { get; set; }
        public int Count { get; set; }

        public string DisplayText { get { return string.Format("{0}({1})", Tag, Count); } }
    }
}