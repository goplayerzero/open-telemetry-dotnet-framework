using System.Diagnostics;
using System.Web.Mvc;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Exporter;
using System;

namespace AspdotNet481.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILoggerFactory _loggerFactory;

        public HomeController()
        {
            var serviceName = "My Dataset Name";
            var serviceVersion = "1.0.0";

            var endPoint = "https://sdk.playerzero.app/otlp";
            var headers = "Authorization=Bearer <api_token>,x-pzprod=true";

            var resourceBuilder = ResourceBuilder.CreateDefault()
               .AddService(serviceName: serviceName, serviceVersion: serviceVersion);

            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddOpenTelemetry(logging =>
                {
                    logging.SetResourceBuilder(resourceBuilder);
                    logging.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(endPoint + "/v1/logs");
                        options.Headers = headers;
                        options.Protocol = OtlpExportProtocol.HttpProtobuf;
                    });
                });
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
                    _logger.LogError("CurrentActivity is null, trace state string not set.");
                }

                return RedirectToAction("Index", "About");
            }
            else
            {
                _logger.LogError("Login Failed!");
                _logger.LogError("Login Failed for user: ${username}", username);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
