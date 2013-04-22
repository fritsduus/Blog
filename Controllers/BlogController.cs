using Blog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Blog.Controllers
{
    public class BlogController : Controller
    {
        private IBlogPostRepository blogPostRepository;

        public BlogController(IBlogPostRepository blogPostRepository)
        {
            this.blogPostRepository = blogPostRepository;
        }

        public ActionResult Index()
        {
            ViewBag.Message = "Welcome to my new blog.";
            ViewBag.Tags = blogPostRepository.GetAllTags();
            return View(blogPostRepository.GetAll().OrderByDescending(b => b.Created));
        }

        public ActionResult Tag(string tagName)
        {
            ViewBag.Message = "Welcome to my new blog.";
            ViewBag.Tags = blogPostRepository.GetAllTags();
            return View("Index", blogPostRepository.GetByTag(tagName).OrderByDescending(b => b.Created));
        }

        //[Authorize(Roles="Editor")]
        public ActionResult Edit(string id)
        {
            BlogPost blogPost = blogPostRepository.Get("BlogPost/" + id);
            return View(blogPost);
        }

        [HttpPost]
        //[Authorize(Roles = "Editor")]
        public ActionResult Edit(BlogPost blogPost)
        {
            if (ModelState.IsValid)
            {
                blogPostRepository.Update(blogPost);
            }
            return View(blogPost);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Who am I ?";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [ChildActionOnly]
        public ActionResult TagsList()
        {
            return PartialView("_TagsListPartial", blogPostRepository.GetAllTags());
        }
    }
}
