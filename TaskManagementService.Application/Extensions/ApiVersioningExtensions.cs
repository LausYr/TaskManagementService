using Asp.Versioning;
using Asp.Versioning.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace TaskManagementService.Application.Extensions;

/// <summary>
/// Методы расширения для настройки версионирования API.
/// </summary>
public static class ApiVersioningExtensions
{
    /// <summary>
    /// Добавляет и настраивает сервисы версионирования API.
    /// </summary>
    public static IServiceCollection AddApiVersioningConfiguration(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        services.Configure<RouteOptions>(options =>
        {
            options.ConstraintMap.Add("apiVersion", typeof(ApiVersionRouteConstraint));
        });

        return services;
    }
}