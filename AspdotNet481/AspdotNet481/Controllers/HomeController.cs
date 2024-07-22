using System.Diagnostics;
using System.Web.Mvc;
using Microsoft.Extensions.Logging;

namespace AspdotNet481.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILoggerFactory _loggerFactory;

        public HomeController()
        {
            _loggerFactory = LoggerFactory.Create(builder =>
            {
                
            });
        }

        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password)
        {
            var _logger = _loggerFactory?.CreateLogger<HomeController>();
            var currentActivity = Activity.Current;

            if (username == "foo" && password == "bar")
            {
                _logger.LogInformation("User logged in");

                if (currentActivity != null)
                {
                    currentActivity.TraceStateString = "userid=1234";
                    _logger.LogInformation("Trace state string set to 'userid=1234'");
                }
                else
                {
                    _logger.LogWarning("CurrentActivity is null, trace state string not set.");
                }

                return RedirectToAction("Index", "Home");
            }
            else
            {
                _logger.LogError("Login Failed!");
                _logger.LogError("Login Failed for user: {username}", username);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
