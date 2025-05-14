using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace TaskManagementService.Application.Extensions;

/// <summary>
/// Методы расширения для настройки OpenTelemetry.
/// </summary>
public static class OpenTelemetryExtensions
{
    /// <summary>
    /// Добавляет и настраивает OpenTelemetry.
    /// </summary>
    /// <param name="serviceName">Имя сервиса для идентификации в телеметрии.</param>
    public static IServiceCollection AddOpenTelemetry(this IServiceCollection services, IConfiguration configuration, string serviceName)
    {
        services.AddOpenTelemetry()
            .WithTracing(builder =>
            {
                builder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSource("TaskManagementService");

                builder.AddConsoleExporter();
            });

        return services;
    }
}