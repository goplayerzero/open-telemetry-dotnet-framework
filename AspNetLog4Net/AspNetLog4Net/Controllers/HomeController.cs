using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AspNetLog4Net.Controllers
{
    public class HomeController : Controller
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(HomeController));

        public ActionResult Index()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password)
        {

            if (username == "foo" && password == "bar")
            {
                logger.Info($"User logged in : {username}");
                return RedirectToAction("Index", "Player");
            }
            else
            {
                logger.Error($"{username} Failed to log in.");
            }

            return RedirectToAction("Index", "Home");
        }
    }
}