![NuGet Version](https://img.shields.io/nuget/v/Vestfold.Extensions.Metrics.svg)
![NuGet Downloads](https://img.shields.io/nuget/dt/Vestfold.Extensions.Metrics.svg)

# Vestfold.Extensions.Metrics

Contains builder extensions to extend a dotnet core application with metrics functionality.

## Setting up for an Azure Function / Azure Web App

Add the following to your Program.cs file:
```csharp
var builder = FunctionsApplication.CreateBuilder(args);
builder.ConfigureFunctionsWebApplication();
builder.Services.AddVestfoldMetrics();

// Configure the service container to collect Prometheus metrics from all registered HttpClients
builder.Services.UseHttpClientMetrics();

builder.Build().Run();
```

Add a new HTTPTrigger function to your Azure Function project, and add the following code to it:
```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Vestfold.Extensions.Metrics.Services;

namespace ChangeThisNamespace;

public class MetricsEndpoint
{
    private readonly ILogger<MetricsEndpoint> _logger;
    private readonly IMetricsService _metricsService;

    public MetricsEndpoint(ILogger<MetricsEndpoint> logger, IMetricsService metricsService)
    {
        _logger = logger;
        _metricsService = metricsService;
    }

    [Function("Metrics")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "metrics")]
        HttpRequest req)
    {
        using var _ = _metricsService.Histogram("MetricsEndpointResponseTime",
            "Response time of the metrics endpoint in milliseconds");
        _logger.LogDebug("Serving Prometheus metrics");
        
        var responseStream = new MemoryStream();
        await Prometheus.Metrics.DefaultRegistry.CollectAndExportAsTextAsync(responseStream);
        responseStream.Position = 0;
        
        var reader = new StreamReader(responseStream);
        var content = await reader.ReadToEndAsync();
        
        return new ContentResult
        {
            Content = content,
            ContentType = "text/plain",
            StatusCode = StatusCodes.Status200OK
        };
    }
}
````

## Setting up for a HostBuilder (Console app, ClassLibrary, etc.)

```csharp
public static async Task Main(string[] args)
{
    await Host.CreateDefaultBuilder(args)
        .ConfigureServices(services => 
        {
            services.AddVestfoldMetrics();
            
            // Configure the service container to collect Prometheus metrics from all registered HttpClients
            services.UseHttpClientMetrics();
            
            // Starts a Kestrel metrics server.
            // Hostname defaults to "+" which means it will listen on all hostnames.
            // The default port is 0, so this should be set to a specific port.
            // The default URL is /metrics (which is a Prometheus convention).
            // Registry defaults to Metrics.DefaultRegistry, which is the default registry for metrics.
            services.AddMetricServer(options =>
            {
                options.Hostname = "localhost";
                options.Port = 8080;
                //options.Url = "/metrics";
                /*options.Registry = Metrics.DefaultRegistry;*/
            });
        })
        .Build()
        .RunAsync();

    await Serilog.Log.CloseAndFlushAsync();
}
```

## Setting up for a WebApplicationBuilder (WebAPI, Blazor, etc.)

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddVestfoldMetrics();

// Configure the service container to collect Prometheus metrics from all registered HttpClients
builder.Services.UseHttpClientMetrics();

var app = builder.Build();

// Starts a Prometheus metrics exporter. The default URL is /metrics (which is a Prometheus convention) and will be made available on the same port as your app is running on.
app.UseMetricServer();

// Configure the ASP.NET Core request pipeline to collect Prometheus metrics on processed HTTP requests. Call this after .UseRouting()
app.UseHttpMetrics();
```

## Using the metrics methods

To use the metrics methods, you inject the `IMetricsService` into your classes.

Then you call:
- `Count` to increment a counter metric.
- `Gauge` to set a gauge metric with an arbitrary value.
- `Histogram` to record duration and number of events, in a histogram metric.

```csharp
public class Something
{
    private readonly IMetricsService _metricsService;
    
    public Something(IMetricsService metricsService)
    {
        _metricsService = metricsService;
    }
    
    public void DoSomething()
    {
        // Increment a counter metric
        // optionally you can provide the increment value, default is 1
        _metricsService.Count("my_counter_metric", "Description of my counter metric");
        
        // Increment a counter metric
        // optionally you can omit the increment value, it will default to 1
        _metricsService.Count("my_counter_metric_2", "Description of my counter metric 2", 1, ("labelName", "labelValue"), ("labelName2", "labelValue2"));

        // Set a gauge metric
        // optionally you can omit the description, it will default to an empty string
        _metricsService.Gauge("my_gauge_metric", "Description of my gauge metric" 42.0);
        
        // Set a gauge metric
        // optionally you can omit the description, it will default to an empty string
        _metricsService.Gauge("my_gauge_metric_2", "Description of my gauge metric 2" 42.0, ("labelName3", "labelValue3"), ("labelName4", "labelValue4"));

        // Record duration and number of events, in a histogram metric. The duration is automatically calculated when the returned _ITimer_ is disposed, either by calling `histogram.Dispose()` or when the returned _ITimer_ is out of block scope.
        using var histogram = _metricsService.Histogram("my_histogram_metric", "Description of my histogram metric");
        histogram.Dispose();
        
        // Record duration and number of events, in a histogram metric. The duration is automatically calculated when the returned _ITimer_ is disposed, either by calling `histogram.Dispose()` or when the returned _ITimer_ is out of block scope.
        // optionally you can omit the description, it will default to an empty string
        using var histogram2 = _metricsService.Histogram("my_histogram_metric_2", "Description of my histogram metric 2", ("labelName5", "labelValue5"), ("labelName6", "labelValue6"));
    }
}
```