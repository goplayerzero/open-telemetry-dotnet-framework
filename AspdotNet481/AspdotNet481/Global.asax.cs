﻿using System;
using System.Web.Mvc;
using System.Web.Routing;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Logs;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Exporter;

namespace AspdotNet481
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private TracerProvider _tracerProvider;
        private MeterProvider _meterProvider;
        private ILoggerFactory _loggerFactory;

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            var serviceName = "My Dataset Name";
            var serviceVersion = "1.0.0";

            var endPoint = "https://sdk.playerzero.app/otlp";
            var headers = "Authorization=Bearer <api_token>,x-pzprod=false";

            var resourceBuilder = ResourceBuilder.CreateDefault()
               .AddService(serviceName: serviceName, serviceVersion: serviceVersion);

            _tracerProvider = Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(resourceBuilder)
                .AddAspNetInstrumentation()
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
    }
}