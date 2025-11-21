using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vestfold.Extensions.Metrics.Services;

namespace Vestfold.Extensions.Metrics.Test;

public class MetricsServiceTest
{
    private readonly MetricsService _metricsService;
    
    public MetricsServiceTest()
    {
        _metricsService = new MetricsService();
    }
    
    [Fact]
    public async Task Count_with_name_and_description_and_increment_should_register_and_unregister_metric()
    {
        const string metricName = "Vestfold_Count_Metric_1";
        const string metricDescription = "A test count metric";
        const int metricIncrementValue = 2;
        
        _metricsService.Count(metricName, metricDescription, metricIncrementValue);

        var registeredMetrics = await GetRegisteredMetrics();

        Assert.Single(registeredMetrics.Where(m => m == $"{metricName} {metricIncrementValue}"));
    }
    
    [Fact]
    public async Task Count_with_name_and_description_and_increment_and_labels_should_register_and_unregister_metric()
    {
        const string metricName = "Vestfold_Count_Metric_2";
        const string metricDescription = "A test count metric";
        const int metricIncrementValue1 = 2;
        const int metricIncrementValue2 = 4;
        var metricLabels = ("label", "value");
        
        // add metric with labels
        _metricsService.Count(metricName, metricDescription, metricIncrementValue1, metricLabels);

        var registeredMetrics = await GetRegisteredMetrics();
        
        Assert.Single(registeredMetrics.Where(m => m == $"{metricName}{{{metricLabels.Item1}=\"{metricLabels.Item2}\"}} {metricIncrementValue1}"));
        
        // update metric with labels
        _metricsService.Count(metricName, metricDescription, metricIncrementValue2, metricLabels);

        registeredMetrics = await GetRegisteredMetrics();
        
        Assert.Single(registeredMetrics.Where(m => m == $"{metricName}{{{metricLabels.Item1}=\"{metricLabels.Item2}\"}} {metricIncrementValue1 + metricIncrementValue2}"));
    }
    
    [Fact]
    public async Task Count_with_name_and_description_and_labels_should_register_and_unregister_metric()
    {
        const string metricName = "Vestfold_Count_Metric_3";
        const string metricDescription = "A test count metric";
        const int metricIncrementValue = 1;
        var metricLabels = ("label", "value");
        
        // add metric with labels
        _metricsService.Count(metricName, metricDescription, metricLabels);

        var registeredMetrics = await GetRegisteredMetrics();
        
        Assert.Single(registeredMetrics.Where(m => m == $"{metricName}{{{metricLabels.Item1}=\"{metricLabels.Item2}\"}} {metricIncrementValue}"));
        
        // update metric with labels
        _metricsService.Count(metricName, metricDescription, metricLabels);

        registeredMetrics = await GetRegisteredMetrics();
        
        Assert.Single(registeredMetrics.Where(m => m == $"{metricName}{{{metricLabels.Item1}=\"{metricLabels.Item2}\"}} {metricIncrementValue * 2}"));
    }
    
    [Fact]
    public async Task Gauge_with_name_and_description_and_value_should_register_and_unregister_metric()
    {
        const string metricName = "Vestfold_Gauge_Metric_1";
        const string metricDescription = "A test gauge metric";
        const double metricValue = 2;
        
        _metricsService.Gauge(metricName, metricDescription, metricValue);

        var registeredMetrics = await GetRegisteredMetrics();
        
        Assert.Single(registeredMetrics.Where(m => m == $"{metricName} {metricValue}"));
    }
    
    [Fact]
    public async Task Gauge_with_name_and_value_should_register_and_unregister_metric()
    {
        const string metricName = "Vestfold_Gauge_Metric_2";
        const double metricValue = 2;
        
        _metricsService.Gauge(metricName, metricValue);

        var registeredMetrics = await GetRegisteredMetrics();
        
        Assert.Single(registeredMetrics.Where(m => m == $"{metricName} {metricValue}"));
    }
    
    [Fact]
    public async Task Gauge_with_name_and_description_and_value_and_labels_should_register_and_unregister_metric()
    {
        const string metricName = "Vestfold_Gauge_Metric_3";
        const string metricDescription = "A test gauge metric";
        const double metricValue1 = 2;
        const double metricValue2 = 4;
        var metricLabels = ("label", "value");
        
        // add metric with labels
        _metricsService.Gauge(metricName, metricDescription, metricValue1, metricLabels);

        var registeredMetrics = await GetRegisteredMetrics();
        
        Assert.Single(registeredMetrics.Where(m => m == $"{metricName}{{{metricLabels.Item1}=\"{metricLabels.Item2}\"}} {metricValue1}"));
        
        // update metric with labels
        _metricsService.Gauge(metricName, metricDescription, metricValue2, metricLabels);

        registeredMetrics = await GetRegisteredMetrics();
        
        Assert.Single(registeredMetrics.Where(m => m == $"{metricName}{{{metricLabels.Item1}=\"{metricLabels.Item2}\"}} {metricValue2}"));
    }
    
    [Fact]
    public async Task Gauge_with_name_and_value_and_labels_should_register_and_unregister_metric()
    {
        const string metricName = "Vestfold_Gauge_Metric_4";
        const double metricValue1 = 2;
        const double metricValue2 = 4;
        var metricLabels = ("label", "value");
        
        // add first metric with labels
        _metricsService.Gauge(metricName, metricValue1, metricLabels);

        var registeredMetrics = await GetRegisteredMetrics();
        
        Assert.Single(registeredMetrics.Where(m => m == $"{metricName}{{{metricLabels.Item1}=\"{metricLabels.Item2}\"}} {metricValue1}"));
        
        // update first metric with labels
        _metricsService.Gauge(metricName, metricValue2, metricLabels);

        registeredMetrics = await GetRegisteredMetrics();
        
        Assert.Single(registeredMetrics.Where(m => m == $"{metricName}{{{metricLabels.Item1}=\"{metricLabels.Item2}\"}} {metricValue2}"));
    }
    
    [Fact]
    public async Task Histogram_with_name_and_description_should_register_and_unregister_metric()
    {
        const string metricName = "Vestfold_Histogram_Metric_1";
        const string metricDescription = "A test histogram metric";
        const int metricCount = 17;
        
        var histogram = _metricsService.Histogram(metricName, metricDescription);
        histogram.Dispose();

        var registeredMetrics = await GetRegisteredMetrics();

        // Histogram metrics have multiple bucket lines + sum and count
        Assert.Equal(metricCount, registeredMetrics.Count(m => m.Contains($"{metricName}_")));
        Assert.Single(registeredMetrics.Where(m => m.Contains($"{metricName}_sum")));
        Assert.Single(registeredMetrics.Where(m => m == $"{metricName}_count 1"));
        Assert.Equal(metricCount - 2, registeredMetrics.Count(m => m.Contains($"{metricName}_bucket")));
    }
    
    [Fact]
    public async Task Histogram_with_name_and_description_and_labels_should_register_and_unregister_metric()
    {
        const string metricName = "Vestfold_Histogram_Metric_2";
        const string metricDescription = "A test histogram metric";
        const int metricCount = 17;
        var metricLabels = ("label", "value");
        
        // add metric with labels
        var histogram = _metricsService.Histogram(metricName, metricDescription, metricLabels);
        histogram.Dispose();

        var registeredMetrics = await GetRegisteredMetrics();
        
        // Histogram metrics have multiple bucket lines + sum and count
        Assert.Equal(metricCount, registeredMetrics.Count(m => m.Contains($"{metricName}_")));
        Assert.Single(registeredMetrics.Where(m => m.Contains($"{metricName}_sum{{{metricLabels.Item1}=\"{metricLabels.Item2}\"}}")));
        Assert.Single(registeredMetrics.Where(m => m == $"{metricName}_count{{{metricLabels.Item1}=\"{metricLabels.Item2}\"}} 1"));
        Assert.Equal(metricCount - 2, registeredMetrics.Count(m => m.Contains($"{metricName}_bucket{{{metricLabels.Item1}=\"{metricLabels.Item2}\"")));
    }
    
    [Fact]
    public async Task Histogram_with_name_and_labels_should_register_and_unregister_metric()
    {
        const string metricName = "Vestfold_Histogram_Metric_3";
        const int metricCount = 17;
        var metricLabels = ("label", "value");
        
        // add first metric with labels
        var histogram = _metricsService.Histogram(metricName, metricLabels);
        histogram.Dispose();

        var registeredMetrics = await GetRegisteredMetrics();
        
        // Histogram metrics have multiple bucket lines + sum and count
        Assert.Equal(metricCount, registeredMetrics.Count(m => m.Contains($"{metricName}_")));
        Assert.Single(registeredMetrics.Where(m => m.Contains($"{metricName}_sum{{{metricLabels.Item1}=\"{metricLabels.Item2}\"}}")));
        Assert.Single(registeredMetrics.Where(m => m == $"{metricName}_count{{{metricLabels.Item1}=\"{metricLabels.Item2}\"}} 1"));
        Assert.Equal(metricCount - 2, registeredMetrics.Count(m => m.Contains($"{metricName}_bucket{{{metricLabels.Item1}=\"{metricLabels.Item2}\"")));
    }

    private static async Task<List<string>> GetRegisteredMetrics()
    {
        await using Stream stream = new MemoryStream();
        await Prometheus.Metrics.DefaultRegistry.CollectAndExportAsTextAsync(stream, CancellationToken.None);
     
        stream.Position = 0;
        using var reader = new StreamReader(stream);
        List<string> lines = [];
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (line != null && line.StartsWith("Vestfold"))
            {
                lines.Add(line);
            }
        }

        return lines;
    }
}