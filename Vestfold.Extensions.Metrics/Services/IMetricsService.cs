using ITimer = Prometheus.ITimer;

namespace Vestfold.Extensions.Metrics.Services;

/// <summary>
/// Exposes methods for managing and recording metrics
/// </summary>
public interface IMetricsService
{
    /// <summary>
    /// Counters only increase in value and reset to zero when the process restarts
    /// </summary>
    /// <param name="name">Name of the counter metric</param>
    /// <param name="description">Description of the counter metric (defaults to string.Empty)</param>
    /// <param name="increment">Amount to increase the counter by (defaults to 1)</param>
    void Count(string name, string? description = null, int increment = 1);
    
    /// <summary>
    /// Counters only increase in value and reset to zero when the process restarts
    /// </summary>
    /// <param name="name">Name of the counter metric</param>
    /// <param name="description">Description of the counter metric (defaults to string.Empty)</param>
    /// <param name="increment">Amount to increase the counter by (defaults to 1)</param>
    /// <param name="labels">Labels to associate with the counter metric</param>
    void Count(string name, string? description = null, int increment = 1,
        params (string labelName, string labelValue)[] labels);
    
    /// <summary>
    /// Counters only increase in value and reset to zero when the process restarts
    /// </summary>
    /// <param name="name">Name of the counter metric</param>
    /// <param name="description">Description of the counter metric (defaults to string.Empty)</param>
    /// <param name="labels">Labels to associate with the counter metric</param>
    void Count(string name, string? description = null, params (string labelName, string labelValue)[] labels);

    /// <summary>
    /// Gauges can have any numeric value and change arbitrarily
    /// </summary>
    /// <param name="name">Name of the gauge metric</param>
    /// <param name="description">Description of the gauge metric</param>
    /// <param name="value">Gauge value at this moment</param>
    void Gauge(string name, string description, double value);
    
    /// <summary>
    /// Gauges can have any numeric value and change arbitrarily
    /// </summary>
    /// <param name="name">Name of the gauge metric</param>
    /// <param name="value">Gauge value at this moment</param>
    void Gauge(string name, double value);
    
    /// <summary>
    /// Gauges can have any numeric value and change arbitrarily
    /// </summary>
    /// <param name="name">Name of the gauge metric</param>
    /// <param name="description">Description of the gauge metric</param>
    /// <param name="value">Gauge value at this moment</param>
    /// <param name="labels">Labels to associate with the gauge metric</param>
    void Gauge(string name, string description, double value, params (string labelName, string labelValue)[] labels);
    
    /// <summary>
    /// Gauges can have any numeric value and change arbitrarily
    /// </summary>
    /// <param name="name">Name of the gauge metric</param>
    /// <param name="value">Gauge value at this moment</param>
    /// <param name="labels">Labels to associate with the gauge metric</param>
    void Gauge(string name, double value, params (string labelName, string labelValue)[] labels);

    /// <summary>
    /// A timer that can be used to observe a duration of elapsed time. The observation is made either when ObserveDuration is called or when the instance is disposed of
    /// </summary>
    /// <param name="name">Name of the histogram metric</param>
    /// <param name="description">Description of the histogram metric (defaults to string.Empty)</param>
    /// <returns>Prometheus.ITimer</returns>
    ITimer Histogram(string name, string? description = null);
    
    /// <summary>
    /// A timer that can be used to observe a duration of elapsed time. The observation is made either when ObserveDuration is called or when the instance is disposed of
    /// </summary>
    /// <param name="name">Name of the histogram metric</param>
    /// <param name="description">Description of the histogram metric (defaults to string.Empty)</param>
    /// <param name="labels">Labels to associate with the histogram metric</param>
    /// <returns>Prometheus.ITimer</returns>
    ITimer Histogram(string name, string? description = null, params (string labelName, string labelValue)[] labels);
    
    /// <summary>
    /// A timer that can be used to observe a duration of elapsed time. The observation is made either when ObserveDuration is called or when the instance is disposed of
    /// </summary>
    /// <param name="name">Name of the histogram metric</param>
    /// <param name="labels">Labels to associate with the histogram metric</param>
    /// <returns>Prometheus.ITimer</returns>
    ITimer Histogram(string name, params (string labelName, string labelValue)[] labels);
}