using System;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Exporter;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AspdotNet481
{
    public static class TelemetryService
    {
        private static TracerProvider _tracerProvider;
        private static MeterProvider _meterProvider;
        private static ILoggerFactory _loggerFactory;

        public static void Initialize(string serviceName, string apiToken, bool pzProd)
        {
            string endpoint = "https://sdk.playerzero.app/otlp";

            var resourceBuilder = ResourceBuilder.CreateDefault()
                .AddService(serviceName: serviceName, serviceVersion: "1.0.0");

            _tracerProvider = Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(resourceBuilder)
                .AddAspNetInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSqlClientInstrumentation(
                        options => options.SetDbStatement = true)
                .AddSource(serviceName)
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri($"{endpoint}/v1/traces");
                    options.Headers = $"Authorization=Bearer {apiToken},x-pzprod={pzProd}";
                    options.Protocol = OtlpExportProtocol.HttpProtobuf;
                })
                .Build();

            _meterProvider = Sdk.CreateMeterProviderBuilder()
                .SetResourceBuilder(resourceBuilder)
                .AddMeter(serviceName)
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri($"{endpoint}/v1/metrics");
                    options.Headers = $"Authorization=Bearer {apiToken},x-pzprod={pzProd}";
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
                        options.Endpoint = new Uri($"{endpoint}/v1/logs");
                        options.Headers = $"Authorization=Bearer {apiToken},x-pzprod={pzProd}";
                        options.Protocol = OtlpExportProtocol.HttpProtobuf;
                    });
                });
            });

            Debug.WriteLine("OpenTelemetry initialized successfully.");
        }

        public static ILogger CreateLogger(string category) => _loggerFactory.CreateLogger(category);

        public static void Dispose()
        {
            _tracerProvider?.Dispose();
            _meterProvider?.Dispose();
            _loggerFactory?.Dispose();
        }
    }
}