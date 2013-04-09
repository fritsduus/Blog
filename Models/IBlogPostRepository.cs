using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Blog.Models
{
    public interface IBlogPostRepository
    {
        BlogPost Get(string id);
        IEnumerable<BlogPost> GetAll();

        IEnumerable<TagCount> GetAllTags();
        IEnumerable<BlogPost> GetByTag(string tag);

        void Add(BlogPost post);
        void Update(BlogPost post);
    }
}