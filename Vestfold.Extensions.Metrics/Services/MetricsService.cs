using System.Collections.Concurrent;
using System.Linq;
using Prometheus;
using ITimer = Prometheus.ITimer;

namespace Vestfold.Extensions.Metrics.Services;

/// <summary>
/// Exposes methods for managing and recording metrics
/// </summary>
public class MetricsService : IMetricsService
{
    private readonly ConcurrentDictionary<string, Counter> _counters = [];
    private readonly ConcurrentDictionary<string, Gauge> _gauges = [];
    private readonly ConcurrentDictionary<string, Histogram> _histograms = [];
    
    /// <summary>
    /// Counters only increase in value and reset to zero when the process restarts
    /// </summary>
    /// <param name="name">Name of the counter metric</param>
    /// <param name="description">Description of the counter metric (defaults to string.Empty)</param>
    /// <param name="increment">Amount to increase the counter by (defaults to 1)</param>
    public void Count(string name, string? description = null, int increment = 1)
    {
        if (!_counters.TryGetValue(name, out var counter))
        {
            counter = Prometheus.Metrics.CreateCounter(name, description ?? string.Empty);
            _counters.AddOrUpdate(name, counter, (_, _) => counter);
        }
        
        counter.Inc(increment);
    }

    /// <summary>
    /// Counters only increase in value and reset to zero when the process restarts<br /><br />
    /// <b>All labels to be used in the same run must be registered when the metric is created the first time.<br />New labelsNames can't be added after the metric is created.<br />LabelValues however will be added to the initial labelNames</b>
    /// </summary>
    /// <param name="name">Name of the counter metric</param>
    /// <param name="description">Description of the counter metric (defaults to string.Empty)</param>
    /// <param name="increment">Amount to increase the counter by (defaults to 1)</param>
    /// <param name="labels">Labels to associate with the counter metric</param>
    public void Count(string name, string? description = null, int increment = 1,
        params (string labelName, string labelValue)[] labels)
    {
        if (!_counters.TryGetValue(name, out var counter))
        {
            counter = Prometheus.Metrics.CreateCounter(name, description ?? string.Empty,
                labels.Select(l => l.labelName).ToArray());
            _counters.AddOrUpdate(name, counter, (_, _) => counter);
        }
        
        var labelValues = labels.Select(l => l.labelValue).ToArray();
        counter.WithLabels(labelValues).Inc(increment);
    }

    /// <summary>
    /// Counters only increase in value and reset to zero when the process restarts<br /><br />
    /// <b>All labels to be used in the same run must be registered when the metric is created the first time.<br />New labelsNames can't be added after the metric is created.<br />LabelValues however will be added to the initial labelNames</b>
    /// </summary>
    /// <param name="name">Name of the counter metric</param>
    /// <param name="description">Description of the counter metric (defaults to string.Empty)</param>
    /// <param name="labels">Labels to associate with the counter metric</param>
    public void Count(string name, string? description = null, params (string labelName, string labelValue)[] labels) =>
        Count(name, description, 1, labels);
    
    /// <summary>
    /// Gauges can have any numeric value and change arbitrarily
    /// </summary>
    /// <param name="name">Name of the gauge metric</param>
    /// <param name="description">Description of the gauge metric</param>
    /// <param name="value">Gauge value at this moment</param>
    public void Gauge(string name, string description, double value)
    {
        if (!_gauges.TryGetValue(name, out var gauge))
        {
            gauge = Prometheus.Metrics.CreateGauge(name, description);
            _gauges.AddOrUpdate(name, gauge, (_, _) => gauge);
        }
        
        gauge.Set(value);
    }
    
    /// <summary>
    /// Gauges can have any numeric value and change arbitrarily
    /// </summary>
    /// <param name="name">Name of the gauge metric</param>
    /// <param name="value">Gauge value at this moment</param>
    public void Gauge(string name, double value) => Gauge(name, string.Empty, value);
    
    /// <summary>
    /// Gauges can have any numeric value and change arbitrarily<br /><br />
    /// <b>All labels to be used in the same run must be registered when the metric is created the first time.<br />New labelsNames can't be added after the metric is created.<br />LabelValues however will be added to the initial labelNames</b>
    /// </summary>
    /// <param name="name">Name of the gauge metric</param>
    /// <param name="description">Description of the gauge metric</param>
    /// <param name="value">Gauge value at this moment</param>
    /// <param name="labels">Labels to associate with the gauge metric</param>
    public void Gauge(string name, string description, double value, params (string labelName, string labelValue)[] labels)
    {
        if (!_gauges.TryGetValue(name, out var gauge))
        {
            gauge = Prometheus.Metrics.CreateGauge(name, description,
                labels.Select(l => l.labelName).ToArray());
            _gauges.AddOrUpdate(name, gauge, (_, _) => gauge);
        }
        
        var labelValues = labels.Select(l => l.labelValue).ToArray();
        gauge.WithLabels(labelValues).Set(value);
    }
    
    /// <summary>
    /// Gauges can have any numeric value and change arbitrarily<br /><br />
    /// <b>All labels to be used in the same run must be registered when the metric is created the first time.<br />New labelsNames can't be added after the metric is created.<br />LabelValues however will be added to the initial labelNames</b>
    /// </summary>
    /// <param name="name">Name of the gauge metric</param>
    /// <param name="value">Gauge value at this moment</param>
    /// <param name="labels">Labels to associate with the gauge metric</param>
    public void Gauge(string name, double value, params (string labelName, string labelValue)[] labels) =>
        Gauge(name, string.Empty, value, labels);
    
    /// <summary>
    /// A timer that can be used to observe a duration of elapsed time. The observation is made either when ObserveDuration is called or when the instance is disposed of
    /// </summary>
    /// <param name="name">Name of the histogram metric</param>
    /// <param name="description">Description of the histogram metric (defaults to string.Empty)</param>
    /// <returns>Prometheus.ITimer</returns>
    public ITimer Histogram(string name, string? description = null)
    {
        if (!_histograms.TryGetValue(name, out var histogram))
        {
            histogram = Prometheus.Metrics.CreateHistogram(name, description ?? string.Empty);
            _histograms.AddOrUpdate(name, histogram, (_, _) => histogram);
        }
        
        return histogram.NewTimer();
    }
    
    /// <summary>
    /// A timer that can be used to observe a duration of elapsed time. The observation is made either when ObserveDuration is called or when the instance is disposed of<br /><br />
    /// <b>All labels to be used in the same run must be registered when the metric is created the first time.<br />New labelsNames can't be added after the metric is created.<br />LabelValues however will be added to the initial labelNames</b>
    /// </summary>
    /// <param name="name">Name of the histogram metric</param>
    /// <param name="description">Description of the histogram metric (defaults to string.Empty)</param>
    /// <param name="labels">Labels to associate with the histogram metric</param>
    /// <returns>Prometheus.ITimer</returns>
    public ITimer Histogram(string name, string? description = null, params (string labelName, string labelValue)[] labels)
    {
        if (!_histograms.TryGetValue(name, out var histogram))
        {
            histogram = Prometheus.Metrics.CreateHistogram(name, description ?? string.Empty,
                labels.Select(l => l.labelName).ToArray());
            _histograms.AddOrUpdate(name, histogram, (_, _) => histogram);
        }
        
        var labelValues = labels.Select(l => l.labelValue).ToArray();
        return histogram.WithLabels(labelValues).NewTimer();
    }
    
    /// <summary>
    /// A timer that can be used to observe a duration of elapsed time. The observation is made either when ObserveDuration is called or when the instance is disposed of<br /><br />
    /// <b>All labels to be used in the same run must be registered when the metric is created the first time.<br />New labelsNames can't be added after the metric is created.<br />LabelValues however will be added to the initial labelNames</b>
    /// </summary>
    /// <param name="name">Name of the histogram metric</param>
    /// <param name="labels">Labels to associate with the histogram metric</param>
    /// <returns>Prometheus.ITimer</returns>
    public ITimer Histogram(string name, params (string labelName, string labelValue)[] labels) =>
        Histogram(name, string.Empty, labels);
}