using Microsoft.Extensions.DependencyInjection;
using Vestfold.Extensions.Metrics.Services;

namespace Vestfold.Extensions.Metrics;

/// <summary>
/// Extension methods for adding Vestfold metrics services to the service collection
/// </summary>
public static class MetricsExtension
{
    /// <summary>
    /// Extension method to add Vestfold metrics services to the service collection
    /// </summary>
    /// <param name="services">The IServiceCollection to add IMetricsService to</param>
    /// <returns></returns>
    public static IServiceCollection AddVestfoldMetrics(this IServiceCollection services) =>
        services.AddSingleton<IMetricsService, MetricsService>();
}