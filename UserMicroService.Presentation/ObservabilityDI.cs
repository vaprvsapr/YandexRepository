using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace UserMicroService.Presentation;

public static partial class DependencyInjectionExtensions
{
    /// <summary>
    /// Добавляет сервисы для логирования, мониторинга и трейсинга.
    /// </summary>
    public static IServiceCollection AddObservability(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenTelemetry()
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.Filter = httpContext =>
                    {
                        var path = httpContext.Request.Path;
                        return !path.StartsWithSegments("/health") &&
                               !path.StartsWithSegments("/metrics");
                    };
                })
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                .AddOtlpExporter(o => o.Endpoint = new Uri(configuration["Otlp:Endpoint"]!)))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation()
                .AddPrometheusExporter())
            .ConfigureResource(r => r.AddService(serviceName: "user-service"));

        return services;
    }
}