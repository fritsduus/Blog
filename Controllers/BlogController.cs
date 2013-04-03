using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Blog.Controllers
{
    public class BlogController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Welcome to my new blog.";

            return View();
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
    }
}
