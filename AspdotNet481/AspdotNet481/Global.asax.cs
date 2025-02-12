using System;
using System.Web.Mvc;
using System.Web.Routing;
using System.Diagnostics;
using System.Web;

namespace AspdotNet481
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static readonly ActivitySource activitySource = new ActivitySource("12TelemetryService"); //Keep it same as your service name

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            TelemetryService.Initialize("12TelemetryService", "666af2fef6b93a24518cf726", true);                       

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