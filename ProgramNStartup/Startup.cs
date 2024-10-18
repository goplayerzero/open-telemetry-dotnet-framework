using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ProgramNStartup
{
    public class Startup
    {
        const string serviceName = "startup-program-cs";
        const string otelEndpoint = "https://sdk.playerzero.app/otlp";
        const string otelHeaders = "Authorization=Bearer 666af2fef6b93a24518cf726,x-pzprod=true";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
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
                    options.AddConsoleExporter();
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
                    })
                    .AddConsoleExporter())
                .WithMetrics(metrics => metrics
                    .AddMeter(serviceName)
                    .AddMeter("System.Net.NameResolution")
                    .AddMeter("System.Net.Http")
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otelEndpoint + "/v1/metrics");
                        options.Headers = otelHeaders;
                        options.Protocol = OtlpExportProtocol.HttpProtobuf;
                    })
                    .AddConsoleExporter());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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
