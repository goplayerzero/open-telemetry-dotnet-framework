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