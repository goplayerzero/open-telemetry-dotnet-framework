# Demonstration of Open Telemetry Instrumentation with .NET Framework 4.6.2

## ASP.NET Initialization
Initialization for ASP.NET is a little different than for ASP.NET Core.

First, install the following NuGet packages:

- [OpenTelemetry.Instrumentation.AspNet](https://www.nuget.org/packages/OpenTelemetry.Instrumentation.AspNet)
- [OpenTelemetry.Exporter.Console](https://www.nuget.org/packages/OpenTelemetry.Exporter.Console)
- [OpenTelemetry.Exporter.OpenTelemetryProtocol](https://www.nuget.org/packages/OpenTelemetry.Exporter.OpenTelemetryProtocol/)
- [OpenTelemetry.Extensions.Hosting](https://www.nuget.org/packages/OpenTelemetry.Extensions.Hosting)

Next, modify your ```Web.Config``` file to add a required HttpModule:

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
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Logs;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Exporter;

public class WebApiApplication : HttpApplication
{
    private TracerProvider _tracerProvider;
    private MeterProvider _meterProvider;
    private ILoggerFactory _loggerFactory;

    protected void Application_Start()
    {
        AreaRegistration.RegisterAllAreas();
        GlobalConfiguration.Configure(WebApiConfig.Register);
        FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
        RouteConfig.RegisterRoutes(RouteTable.Routes);
        BundleConfig.RegisterBundles(BundleTable.Bundles);

        var serviceName = "My Dataset Name";
        var endPoint = "https://sdk.playerzero.app/otlp";
        var headers = "Authorization=Bearer <api token>,x-pzprod=false";

        _tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddAspNetInstrumentation()
            .AddConsoleExporter()
            .AddSource(serviceName)
            .SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                    .AddService(serviceName: serviceName, serviceVersion: "1.0.0"))
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(endPoint + "/v1/traces");
                options.Headers = headers;
                options.Protocol = OtlpExportProtocol.HttpProtobuf;
            })
            .Build();

        _meterProvider = Sdk.CreateMeterProviderBuilder()
            .AddMeter(serviceName)
            .AddConsoleExporter()
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
                logging.AddConsoleExporter();
                logging.SetResourceBuilder(
                    ResourceBuilder.CreateDefault()
                        .AddService(serviceName: serviceName, serviceVersion: "1.0.0"));
                logging.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(endPoint + "/v1/logs");
                    options.Headers = headers;
                    options.Protocol = OtlpExportProtocol.HttpProtobuf;
                });
            });
        });

        var logger = _loggerFactory.CreateLogger<WebApiApplication>();
        logger.LogInformation("Application started and OpenTelemetry configured.");

        Application["LoggerFactory"] = _loggerFactory;
    }

    protected void Application_End()
    {
        _tracerProvider?.Dispose();
        _meterProvider?.Dispose();
        _loggerFactory?.Dispose();
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