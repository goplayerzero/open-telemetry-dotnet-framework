# Demonstration of Open Telemetry Instrumentation with .NET Framework 4.8.1


## .NET Framework instrumentation configuration

First, install the following NuGet packages:

- [OpenTelemetry](https://www.nuget.org/packages/OpenTelemetry)
- [OpenTelemetry.Extensions.Hosting](https://www.nuget.org/packages/OpenTelemetry.Extensions.Hosting)
- [OpenTelemetry.Instrumentation.AspNet](https://www.nuget.org/packages/OpenTelemetry.Instrumentation.AspNet)
- [OpenTelemetry.Instrumentation.Http](https://www.nuget.org/packages/OpenTelemetry.Instrumentation.Http)
- [OpenTelemetry.Exporter.OpenTelemetryProtocol](https://www.nuget.org/packages/OpenTelemetry.Exporter.OpenTelemetryProtocol/)
- [OpenTelemetry.Instrumentation.SqlClient](https://www.nuget.org/packages/OpenTelemetry.Instrumentation.sqlclient)



Next, make sure ```Web.Config``` file gets updated with below changes, if not modify your ```Web.Config``` file to add a required HttpModule: 

```
<system.webServer>
    <modules>
        <add
            name="TelemetryHttpModule"
            type="OpenTelemetry.Instrumentation.AspNet.TelemetryHttpModule,
                OpenTelemetry.Instrumentation.AspNet.TelemetryHttpModule"
            preCondition="integratedMode,managedHandler" />
    </modules>
</system.webServer>
```

Finally, initialize ASP.NET instrumentation in your ```Global.asax.cs``` file along with other OpenTelemetry initialization:

```
using System;
using System.Web.Mvc;
using System.Web.Routing;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Logs;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Exporter;
using System.Diagnostics;
using System.Web;

namespace AspdotNet481
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private TracerProvider _tracerProvider;
        private MeterProvider _meterProvider;
        private ILoggerFactory _loggerFactory;

        private static readonly ActivitySource activitySource = new ActivitySource("My Dataset Name"); //Keep it same as your service name

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            var serviceName = "My Dataset Name";
            var serviceVersion = "1.0.0";

            var endPoint = "https://sdk.playerzero.app/otlp";
            var headers = "Authorization=Bearer <api_token>,x-pzprod=true";

            var resourceBuilder = ResourceBuilder.CreateDefault()
               .AddService(serviceName: serviceName, serviceVersion: serviceVersion);

            _tracerProvider = Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(resourceBuilder)
                .AddAspNetInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSqlClientInstrumentation(
                        options => options.SetDbStatement = true)
                .AddSource(serviceName)
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(endPoint + "/v1/traces");
                    options.Headers = headers;
                    options.Protocol = OtlpExportProtocol.HttpProtobuf;
                })
                .Build();

            _meterProvider = Sdk.CreateMeterProviderBuilder()
                .SetResourceBuilder(resourceBuilder)
                .AddMeter(serviceName)
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(endPoint + "/v1/metrics");
                    options.Headers = headers;
                    options.Protocol = OtlpExportProtocol.HttpProtobuf;
                })
                .Build();

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

        protected void Application_End()
        {
            _tracerProvider?.Dispose();
            _meterProvider?.Dispose();
            _loggerFactory?.Dispose();
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
```


## Configure Activity TraceState
```
var currentActivity = Activity.Current;            
if (currentActivity != null)
{
    currentActivity.TraceStateString = $"userid=your_user_id";
}
```

## Passing Trace Id from UI in Server Sider rending Application
After you integrated Playerzero into your application by using WebSDK, below change needs to be done to pass Trace Id from UI to backend.
Generally we need do this in ```Layout.cshtml``` or ```Master.master``` page.

## Embed the PlayerZero Javascript snippet
```
<script type="text/javascript"
            src="https://go.playerzero.app/record/<websdk_token>"
            async crossorigin></script>
<script>
    const userId = "<USER_ID>";
    const metadata = {
        name: "<USER_NAME>",
        email: "<USER_EMAIL>",
        group: "<GROUP>"
    };
    const setCookie = (t) => document.cookie = `pz-traceid=${t}; Path=/;`;

    if (window.playerzero) {
        // PlayerZero has loaded
        window.playerzero.identify(userId, metadata);
        window.playerzero.onTraceChange(setCookie);
    } else {
        // PlayerZero has not loaded, so we'll wait for the ready event
        window.addEventListener(
            "playerzero_ready",
            () => {
                window.playerzero.identify(userId, metadata);
                window.playerzero.onTraceChange(setCookie);
            },
            { once: true }
        );
    }
</script>
```

## Read trace id from cookie and SetParentId of Current Activity.
In ```Global.asax``` file, we need to read the cookie value and set in current activity.

```
protected void Application_BeginRequest()
{
    HttpCookie traceIdCookie = HttpContext.Current.Request.Cookies["pz-traceid"];
    if (traceIdCookie != null)
    {
        string traceId = traceIdCookie.Value;

        // Convert the string traceId to ActivityTraceId
        ActivityTraceId pzTraceId = ActivityTraceId.CreateFromString(traceId.AsSpan());

        // Get the current activity
        var currentActivity = Activity.Current;
        if (currentActivity != null)
        {
            // Set the parent id using the converted ActivityTraceId
            currentActivity.SetParentId(pzTraceId, currentActivity.SpanId, currentActivity.ActivityTraceFlags);
        }               
    }
    else
    {
        Debug.WriteLine("Trace ID cookie not found.");
    }
}
```


## Instrumenting SQL Client
Include below code snippet in your trace provider configurations.

For latest versions (1.9.0-beta.1 or greater)
```
.AddSqlClientInstrumentation(options =>
{
    options.SetDbStatementForStoredProcedure = true;
    options.SetDbStatementForText = true;
})
```

For Older versions
```
.AddSqlClientInstrumentation(
    options => options.SetDbStatement = true)
```
