# Demonstration of Open Telemetry Instrumentation with VB.NET Framework 4.8.1

First, install the following NuGet packages:

- [OpenTelemetry.Instrumentation.AspNet](https://www.nuget.org/packages/OpenTelemetry.Instrumentation.AspNet)
- [OpenTelemetry.Exporter.OpenTelemetryProtocol](https://www.nuget.org/packages/OpenTelemetry.Exporter.OpenTelemetryProtocol/)
- [OpenTelemetry.Extensions.Hosting](https://www.nuget.org/packages/OpenTelemetry.Extensions.Hosting)
- [OpenTelemetry.Instrumentation.Http](https://www.nuget.org/packages/OpenTelemetry.Instrumentation.Http)


Finally, initialize instrumentation in your ```Global.asax``` file along with other OpenTelemetry initialization:

```
Imports OpenTelemetry
Imports OpenTelemetry.Trace
Imports OpenTelemetry.Resources
Imports OpenTelemetry.Metrics
Imports OpenTelemetry.Logs
Imports OpenTelemetry.Exporter
Imports Microsoft.Extensions.DependencyInjection
Imports Microsoft.Extensions.Logging

Public Class Global_asax
    Inherits HttpApplication

    Private Shared _serviceProvider As IServiceProvider
    Private Shared tracerProvider As TracerProvider
    Private Shared meterProvider As MeterProvider

    Public Shared Logger As ILogger(Of Global_asax)

    Dim serviceName = "My Dataset Name"
    Dim serviceVersion = "1.0.0"
    Dim endPoint = "https://sdk.playerzero.app/otlp"
    Dim headers = "Authorization=Bearer <api_token>,x-pzprod=true"

    Sub Application_Start(sender As Object, e As EventArgs)
        System.Diagnostics.Debug.WriteLine("Application_Start: Starting OpenTelemetry configuration")

        Try
            ' Initialize Tracer Provider
            tracerProvider = Sdk.CreateTracerProviderBuilder() _
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName:=serviceName, serviceVersion:=serviceVersion)) _
                .AddHttpClientInstrumentation() _
                .AddSource(serviceName) _
                .SetSampler(New AlwaysOnSampler()) _
                .AddOtlpExporter(Function(options)
                                     options.Endpoint = New Uri(endPoint + "/v1/traces")
                                     options.Headers = headers
                                     options.Protocol = OtlpExportProtocol.HttpProtobuf
                                 End Function) _
                .Build()

            ' Initialize Meter Provider
            meterProvider = Sdk.CreateMeterProviderBuilder() _
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName:=serviceName, serviceVersion:=serviceVersion)) _
                .AddMeter(serviceName) _
                .AddOtlpExporter(Function(options)
                                     options.Endpoint = New Uri(endPoint + "/v1/metrics")
                                     options.Headers = headers
                                     options.Protocol = OtlpExportProtocol.HttpProtobuf
                                 End Function) _
                .Build()

            ' Set up Dependency Injection
            Dim serviceCollection As New ServiceCollection()
            ConfigureServices(serviceCollection)

            ' Build the ServiceProvider
            _serviceProvider = serviceCollection.BuildServiceProvider()

            ' Example logging
            Logger = _serviceProvider.GetRequiredService(Of ILogger(Of Global_asax))()
            Logger.LogInformation("Application Started")
            Logger.LogError("Logging failed!")
            Logger.LogCritical("Logging LogCritical!")

            System.Diagnostics.Debug.WriteLine("Application_Start: OpenTelemetry configured successfully")

        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine($"Application_Start: Error configuring OpenTelemetry - {ex.Message}")
        End Try
    End Sub

    Sub Application_End(sender As Object, e As EventArgs)
        System.Diagnostics.Debug.WriteLine("Application_End: Disposing OpenTelemetry")

        ' Dispose OpenTelemetry providers
        tracerProvider?.Dispose()
        meterProvider?.Dispose()
    End Sub

    Private Sub ConfigureServices(services As IServiceCollection)
        ' Add logging
        services.AddLogging(Function(builder) _
                                builder.AddOpenTelemetry(Function(logging)
                                                             logging.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName:=serviceName, serviceVersion:=serviceVersion))
                                                             logging.AddOtlpExporter(Function(options)
                                                                                         options.Endpoint = New Uri(endPoint + "/v1/logs")
                                                                                         options.Headers = headers
                                                                                         options.Protocol = OtlpExportProtocol.HttpProtobuf
                                                                                     End Function)
                                                         End Function) _
                                .SetMinimumLevel(LogLevel.Information))
    End Sub
End Class

```


## Configure Activity TraceState
```
Dim currentActivity = Activity.Current
If currentActivity IsNot Nothing Then
    ' Use the current activity
    currentActivity.TraceStateString = "userid=your_user_id"
End If
```