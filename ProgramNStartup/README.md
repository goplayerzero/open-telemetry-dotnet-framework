# Open Telemetry with dot net
Demonstration of OpenTelemetry instrumentation in a .NET project with ```Program.cs``` and ```Startup.cs``` separated.

# ASP.NET Core instrumentation configuration

Install the instrumentation NuGet packages from OpenTelemetry that will generate the telemetry, and set them up.
1 Add the packages
```
dotnet add package OpenTelemetry
dotnet add package OpenTelemetry.Instrumentation.Http
dotnet add package OpenTelemetry.Instrumentation.SqlClient
dotnet add package OpenTelemetry.Instrumentation.AspNetCore
dotnet add package OpenTelemetry.Exporter.OpenTelemetryProtocol
dotnet add package OpenTelemetry.Exporter.Console
dotnet add package OpenTelemetry.Extensions.Hosting
```

2 Setup the OpenTelemetry code

In Startup.cs, add the following lines:
```
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ProgramNStartup
{
    public class Startup
    {
        const string serviceName = "My Dataset";
        const string otelEndpoint = "https://sdk.playerzero.app/otlp";
        const string otelHeaders = "Authorization=Bearer <api_token>,x-pzprod=false";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddOpenTelemetry(options =>
                {
                    options.IncludeFormattedMessage = true;
                    options.ParseStateValues = true;
                    options.SetResourceBuilder(
                        ResourceBuilder.CreateDefault()
                            .AddService(serviceName));
                    options.AddOtlpExporter(o =>
                    {
                        o.Endpoint = new Uri(otelEndpoint + "/v1/logs");
                        o.Headers = otelHeaders;
                        o.Protocol = OtlpExportProtocol.HttpProtobuf;
                    });
                });
            });

            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService(serviceName))
                .WithTracing(tracing => tracing
                    .AddSource(serviceName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSqlClientInstrumentation(options =>
                    {
                        options.SetDbStatementForText = true;
                        options.SetDbStatementForStoredProcedure = true;
                    })
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otelEndpoint + "/v1/traces");
                        options.Headers = otelHeaders;
                        options.Protocol = OtlpExportProtocol.HttpProtobuf;
                    }))
                .WithMetrics(metrics => metrics
                    .AddMeter(serviceName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otelEndpoint + "/v1/metrics");
                        options.Headers = otelHeaders;
                        options.Protocol = OtlpExportProtocol.HttpProtobuf;
                    }));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

```