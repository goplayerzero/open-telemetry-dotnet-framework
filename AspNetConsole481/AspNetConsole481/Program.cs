using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Logs;
using OpenTelemetry.Exporter;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Serilog.Sinks.OpenTelemetry;

namespace AspNetConsole481
{
    class Program
    {
        private static ActivitySource activitySource;
        private static TracerProvider tracerProvider;
        private static MeterProvider meterProvider;
        private static ILoggerFactory loggerFactory;

        private static ILogger<Program> logger;

        static void Main(string[] args)
        {
            var serviceName = "My Dataset";
            var serviceVersion = "1.0.0";

            var endPoint = "https://sdk.playerzero.app/otlp";
            var headers = "Authorization=Bearer <api_token>,x-pzprod=true";

            var resourceBuilder = ResourceBuilder.CreateDefault()
               .AddService(serviceName: serviceName, serviceVersion: serviceVersion);

            tracerProvider = Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(resourceBuilder)
                .AddSource(serviceName)
                .AddSqlClientInstrumentation(options =>
                {
                    options.SetDbStatementForStoredProcedure = true;
                    options.SetDbStatementForText = true;
                })
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(endPoint + "/v1/traces");
                    options.Headers = headers;
                    options.Protocol = OtlpExportProtocol.HttpProtobuf;
                })
                .AddConsoleExporter()
                .Build();

            meterProvider = Sdk.CreateMeterProviderBuilder()
                .SetResourceBuilder(resourceBuilder)
                .AddMeter(serviceName)
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(endPoint + "/v1/metrics");
                    options.Headers = headers;
                    options.Protocol = OtlpExportProtocol.HttpProtobuf;
                })
                .AddConsoleExporter()
                .Build();

            // Configure logging
            loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddOpenTelemetry(logging =>
                    {
                        logging.IncludeFormattedMessage = true;
                        logging.IncludeScopes = true;
                        logging.ParseStateValues = true;
                        logging.SetResourceBuilder(resourceBuilder);
                        logging.AddConsoleExporter();
                        logging.AddOtlpExporter(options =>
                        {
                            options.Endpoint = new Uri(endPoint + "/v1/logs");
                            options.Headers = headers;
                            options.Protocol = OtlpExportProtocol.HttpProtobuf;
                        });
                    });
            });

            logger = loggerFactory.CreateLogger<Program>();


            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.OpenTelemetry(options =>
                {
                    options.Endpoint = new Uri(endPoint + "/v1/logs").ToString();
                    options.Headers = new Dictionary<string, string> {
                        ["Authorization"] = "Bearer <api_token>",
                        ["x-pzprod"] = "true"
                    };
                    options.Protocol = OtlpProtocol.HttpProtobuf;
                    options.ResourceAttributes = new Dictionary<string, object>
                    {
                        ["service.name"] = serviceName
                    };
                })
                .CreateLogger();

            Log.Warning("Serilog Application starting...");
            Log.Error("SERILOG - Serilog Error...");
            Log.Fatal("SERILOG - Serilog Fatal");
            Log.Warning("SERILOG - WARNING");

            activitySource = new ActivitySource(serviceName, serviceVersion);

            using (var activity = Activity.Current ?? activitySource.StartActivity("Main"))
            {
                if (activity != null)
                {
                    // Log a message
                    logger.LogInformation("MSLOGGGER - Open Telemetry integration with Playerzero.");
                    logger.LogError("MSLOGGGER - Completely new error");
                    

                    // Simulate user service
                    var userService = new UserService(loggerFactory.CreateLogger<UserService>(), Log.Logger);

                    // Simulate user login
                    SimulateUserLogin(userService);

                }
            }

            Console.WriteLine("Hello, World!");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            DisposeProviders();
        }

        private static void DisposeProviders()
        {
            tracerProvider?.Dispose();
            meterProvider?.Dispose();
            loggerFactory?.Dispose();
        }

        static void SimulateUserLogin(UserService userService)
        {
            string username = "foo";
            string password = "bar";

            if (userService.Authenticate(username, password))
            {
                Console.WriteLine("User logged in successfully!");
                Log.Warning("SERILOG - User logged in successfully! " + username + password);
            }
            else
            {
                Console.WriteLine("Login failed. Please check your credentials. " + username + password);
                Log.Error("SERILOG - Login failed. Please check your credentials. " + username + password);
                Log.Fatal("SERILOG - FATAL TYPE _ Login failed. Please check your credentials. " + username);
            }
        }
    }
}
