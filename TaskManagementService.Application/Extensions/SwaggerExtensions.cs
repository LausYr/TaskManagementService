using Asp.Versioning.ApiExplorer;
using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.OpenApi.Any;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace TaskManagementService.Application.Extensions
{
    /// <summary>
    /// Методы расширения для настройки Swagger.
    /// </summary>
    public static class SwaggerExtensions
    {
        /// <summary>
        /// Добавляет и настраивает сервисы Swagger для документирования API.
        /// </summary>
        /// <param name="serviceName">Имя сервиса для отображения в документации.</param>
        /// <param name="contactEmail">Контактный email для документации.</param>
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services, string serviceName, string contactEmail, ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            var logger = loggerFactory.CreateLogger("SwaggerExtensions");

            services.AddSwaggerGen(options =>
            {
                var provider = services.BuildServiceProvider().GetService<IApiVersionDescriptionProvider>();
                ConfigureSwaggerDocs(options, provider, serviceName, contactEmail);
                IncludeXmlComments(options, configuration, logger);
                options.OperationFilter<SwaggerDefaultValues>();
            });

            return services;
        }

        /// <summary>
        /// Настраивает документы Swagger для различных версий API.
        /// </summary>
        /// <param name="options">Параметры генерации Swagger.</param>
        /// <param name="provider">Провайдер описаний версий API.</param>
        /// <param name="serviceName">Имя сервиса.</param>
        /// <param name="contactEmail">Контактный email.</param>
        private static void ConfigureSwaggerDocs(SwaggerGenOptions options, IApiVersionDescriptionProvider? provider, string serviceName, string contactEmail)
        {
            if (provider != null && provider.ApiVersionDescriptions.Any())
            {
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerDoc(description.GroupName, CreateOpenApiInfo(serviceName, description.ApiVersion, contactEmail));
                }
            }
            else
            {
                options.SwaggerDoc("v1", CreateOpenApiInfo(serviceName, new ApiVersion(1, 0), contactEmail));
            }
        }

        /// <summary>
        /// Создает информационный объект OpenAPI для документации Swagger.
        /// </summary>
        /// <param name="serviceName">Имя сервиса.</param>
        /// <param name="version">Версия API.</param>
        /// <param name="contactEmail">Контактный email.</param>
        /// <returns>Объект информации OpenAPI.</returns>
        private static OpenApiInfo CreateOpenApiInfo(string serviceName, ApiVersion version, string contactEmail)
        {
            return new OpenApiInfo
            {
                Title = $"{serviceName} API v{version}",
                Version = version.ToString(),
                Description = $"API для {serviceName}",
                Contact = new OpenApiContact { Name = $"{serviceName} Team", Email = contactEmail }
            };
        }

        /// <summary>
        /// Включает XML-комментарии в документацию Swagger.
        /// </summary>
        /// <param name="options">Параметры генерации Swagger.</param>
        /// <param name="configuration">Конфигурация приложения.</param>
        /// <param name="logger">Логгер для записи информации о процессе.</param>
        private static void IncludeXmlComments(SwaggerGenOptions options, IConfiguration configuration, ILogger logger)
        {
            var xmlPaths = new List<string>();
            try
            {
                var entryAssembly = Assembly.GetEntryAssembly();
                if (entryAssembly != null)
                {
                    var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{entryAssembly.GetName().Name}.xml");
                    if (File.Exists(xmlPath)) xmlPaths.Add(xmlPath);
                    else logger.LogWarning("Файл комментариев XML не найден: {XmlPath}", xmlPath);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Не удалось загрузить комментарии XML");
            }

            var additionalPaths = configuration.GetSection("Swagger:XmlPaths").Get<string[]>();
            if (additionalPaths != null)
            {
                foreach (var path in additionalPaths)
                {
                    var fullPath = Path.IsPathRooted(path) ? path : Path.Combine(AppContext.BaseDirectory, path);
                    if (File.Exists(fullPath) && !xmlPaths.Contains(fullPath)) xmlPaths.Add(fullPath);
                }
            }

            foreach (var path in xmlPaths)
            {
                try
                {
                    options.IncludeXmlComments(path);
                    logger.LogInformation("Включены комментарии XML из {XmlPath}", path);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Не удалось включить комментарии XML из {XmlPath}", path);
                }
            }
        }

        /// <summary>
        /// Настраивает использование Swagger UI.
        /// </summary>
        /// <param name="serviceName">Имя сервиса для отображения в Swagger UI.</param>
        public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app, string serviceName)
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                var provider = app.ApplicationServices.GetService<IApiVersionDescriptionProvider>();
                if (provider != null && provider.ApiVersionDescriptions.Any())
                {
                    foreach (var description in provider.ApiVersionDescriptions)
                    {
                        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", $"{serviceName} API {description.ApiVersion}");
                    }
                }
                else
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", $"{serviceName} API v1");
                }
                options.RoutePrefix = "swagger";
            });
            return app;
        }
    }

    /// <summary>
    /// Фильтр операций Swagger для установки значений по умолчанию.
    /// </summary>
    public class SwaggerDefaultValues : IOperationFilter
    {
        /// <summary>
        /// Применяет фильтр к операции Swagger.
        /// </summary>
        /// <param name="operation">Операция OpenAPI для модификации.</param>
        /// <param name="context">Контекст фильтра операции.</param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var apiDescription = context.ApiDescription;
            operation.Deprecated = apiDescription.IsDeprecated();

            if (operation.Responses != null)
            {
                foreach (var responseType in context.ApiDescription.SupportedResponseTypes)
                {
                    var responseKey = responseType.IsDefaultResponse ? "default" : responseType.StatusCode.ToString();
                    if (operation.Responses.TryGetValue(responseKey, out var response))
                    {
                        var contentTypesToRemove = response.Content
                            .Where(content => !responseType.ApiResponseFormats.Any(x => x.MediaType == content.Key))
                            .Select(content => content.Key)
                            .ToList();

                        foreach (var contentType in contentTypesToRemove)
                        {
                            response.Content.Remove(contentType);
                        }
                    }
                }
            }

            if (operation.Parameters == null)
                return;

            foreach (var parameter in operation.Parameters)
            {
                var description = apiDescription.ParameterDescriptions.FirstOrDefault(p => p.Name == parameter.Name);
                if (description == null)
                    continue;

                if (parameter.Description == null)
                {
                    parameter.Description = description.ModelMetadata?.Description;
                }

                if (parameter.Schema.Default == null && description.DefaultValue != null &&
                    description.DefaultValue is not DBNull)
                {
                    switch (description.DefaultValue)
                    {
                        case bool boolValue:
                            parameter.Schema.Default = new OpenApiBoolean(boolValue);
                            break;
                        case int intValue:
                            parameter.Schema.Default = new OpenApiInteger(intValue);
                            break;
                        case long longValue:
                            parameter.Schema.Default = new OpenApiLong(longValue);
                            break;
                        case float floatValue:
                            parameter.Schema.Default = new OpenApiFloat(floatValue);
                            break;
                        case double doubleValue:
                            parameter.Schema.Default = new OpenApiDouble(doubleValue);
                            break;
                        case string stringValue:
                            parameter.Schema.Default = new OpenApiString(stringValue);
                            break;
                    }
                }

                parameter.Required |= description.IsRequired;
            }
        }
    }
}