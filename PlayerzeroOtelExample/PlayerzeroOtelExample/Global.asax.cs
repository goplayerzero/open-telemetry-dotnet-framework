using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Http;

namespace PlayerzeroOtelExample
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static readonly ActivitySource activitySource = new ActivitySource("<SERVICE_NAME>");
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            
            TelemetryService.Initialize("<SERVICE_NAME>", "<API_TOKEN>", true); // Service Name, Token, PzProd
        }
        
        protected void Application_End()
        {
            TelemetryService.Dispose();
        }

        protected void Application_BeginRequest()
        {
            HttpCookie traceIdCookie = HttpContext.Current.Request.Cookies["pz-traceid"];
            if (traceIdCookie != null)
            {
                string traceId = traceIdCookie.Value;

                // Convert the string traceId to ActivityTraceId
                ActivityTraceId pzTraceId = ActivityTraceId.CreateFromString(traceId.AsSpan());

                var spanId = ActivitySpanId.CreateRandom();

                var activityContext = new ActivityContext(pzTraceId, spanId, ActivityTraceFlags.Recorded);

                Activity.Current.Stop();

                // Create and start the activity
                var activity = activitySource.StartActivity("CustomActivity", ActivityKind.Server, activityContext);

                Activity.Current = activity;

                Debug.WriteLine($"Activity started with TraceId: {Activity.Current?.TraceId}");
            }
        }
    }
}
