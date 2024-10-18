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
                    .AddMeter("System.Net.NameResolution")
                    .AddMeter("System.Net.Http")
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
