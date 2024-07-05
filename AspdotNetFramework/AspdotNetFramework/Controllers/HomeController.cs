using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;

namespace AspdotNetFramework.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";


            var currentActivity = Activity.Current;
            
            if (currentActivity != null)
            {
                // Add TraceStateString if user is valid
                currentActivity.TraceStateString = $"userid=1234";
            }

            return View();
        }
    }
}
