using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.OpenTelemetry;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace PZConfigurations
{
    public class PZConfigurations
    {
        private static PZConfigurations _otelInstance;
        private static object _locker = new object();
        private TracerProvider tracerProvider;
        private MeterProvider meterProvider;
        private ResourceBuilder resourceBuilder;
        
        private string endPoint;
        private string headers;
        private string serviceName;
        private string serviceVersion;

        public static PZConfigurations GetInstance
        {
            get
            {
                if (_otelInstance == null)
                {
                    lock (_locker)
                    {
                        _otelInstance = new PZConfigurations();
                    }
                }
                return _otelInstance;
            }
        }

        private void BuildResource(string serviceName, string serviceVersion)
        {
            resourceBuilder = ResourceBuilder.CreateDefault()
               .AddService(serviceName: serviceName, serviceVersion: serviceVersion);
        }

        private void CreateTraceProvider(string serviceName)
        {
            tracerProvider = Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(resourceBuilder)
                .AddSource(serviceName)
                .AddHttpClientInstrumentation()
                .AddSqlClientInstrumentation(
                        options =>
                        {
                            options.SetDbStatementForText = true;
                            options.SetDbStatementForStoredProcedure = true;
                        })
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(endPoint + "/v1/traces");
                    options.Headers = headers;
                    options.Protocol = OtlpExportProtocol.HttpProtobuf;
                })
                .Build();
        }

        private void CreateMeterProvider(string serviceName)
        {
            meterProvider = Sdk.CreateMeterProviderBuilder()
                .SetResourceBuilder(resourceBuilder)
                .AddMeter(serviceName)
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(endPoint + "/v1/metrics");
                    options.Headers = headers;
                    options.Protocol = OtlpExportProtocol.HttpProtobuf;
                })
                .Build();
        }

        public void InitializePZOtel()
        {
            var pzOtelSection = ConfigurationManager.GetSection("pzOtelSection") as PZOtelSettings;
            if (pzOtelSection != null)
            {
                if (pzOtelSection.pzOtelSettings.Count != 0)
                {
                    endPoint = pzOtelSection.pzOtelSettings["endPoint"].Value;
                    headers = pzOtelSection.pzOtelSettings["authToken"].Value;
                    serviceName = pzOtelSection.pzOtelSettings["serviceName"].Value;
                    serviceVersion = pzOtelSection.pzOtelSettings["serviceVersion"].Value;
                }
            }

            BuildResource(serviceName, serviceVersion);

            CreateTraceProvider(serviceName);

            CreateMeterProvider(serviceName);
        }

        public LoggerConfiguration ConfigureSerilogLoggerFactory(LoggerConfiguration loggerConfig)
        {
            Dictionary<string, string> header = headers.Split(',')
                                                        .Select(part => part.Split('='))
                                                        .ToDictionary(split => split[0], split => split[1]);

            return loggerConfig.WriteTo.OpenTelemetry(options =>
            {
                options.Endpoint = new Uri(endPoint + "/v1/logs").ToString();
                options.Headers = header;
                options.Protocol = OtlpProtocol.HttpProtobuf;
                options.ResourceAttributes = new Dictionary<string, object>
                {
                    ["service.name"] = serviceName
                };
            });
        }
                
        public void Dispose()
        {
            tracerProvider?.Dispose();
            meterProvider?.Dispose();
        }
    }
}
