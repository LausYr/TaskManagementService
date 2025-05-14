using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace TaskManagementService.Application.Extensions
{
    /// <summary>
    /// Предоставляет методы расширения для настройки проверок работоспособности.
    /// </summary>
    public static class HealthChecksExtensions
    {
        /// <summary>
        /// Добавляет базовые проверки работоспособности.
        /// </summary>
        public static IServiceCollection AddBaseHealthChecks(this IServiceCollection services)
        {
            services.AddHealthChecks();
            return services;
        }

        /// <summary>
        /// Добавляет проверки работоспособности для базы данных PostgreSQL и RabbitMQ.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Выбрасывается, если строка подключения к базе данных не настроена или отсутствует конфигурация RabbitMQ.
        /// </exception>
        public static IServiceCollection AddHealthChecksWithDbAndRabbitMQ(this IServiceCollection services, IConfiguration configuration)
        {
            var healthChecksBuilder = services.AddHealthChecks();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("DefaultConnection is not configured.");
            healthChecksBuilder.AddNpgSql(connectionString, name: "database", tags: new[] { "db", "postgres" });

            var rabbitMqConfig = configuration.GetSection("RabbitMQ");
            if (!rabbitMqConfig.Exists())
                throw new InvalidOperationException("RabbitMQ configuration is missing.");
            var host = rabbitMqConfig["Host"] ?? "localhost";
            var port = rabbitMqConfig["Port"] != null ? $":{rabbitMqConfig["Port"]}" : ":5672";
            var userName = rabbitMqConfig["UserName"] ?? "guest";
            var password = rabbitMqConfig["Password"] ?? "guest";
            var virtualHost = rabbitMqConfig["VirtualHost"] ?? "/";
            var rabbitConnectionString = $"amqp://{Uri.EscapeDataString(userName)}:{Uri.EscapeDataString(password)}@{host}{port}/{Uri.EscapeDataString(virtualHost)}";
            healthChecksBuilder.AddRabbitMQ(rabbitConnectionString, name: "rabbitmq", tags: new[] { "messaging", "rabbitmq" });

            return services;
        }

        /// <summary>
        /// Настраивает конечные точки проверки работоспособности.
        /// </summary>
        public static IApplicationBuilder UseHealthChecks(this IApplicationBuilder app)
        {
            app.UseHealthChecks("/health", new HealthCheckOptions { ResponseWriter = WriteHealthCheckResponse });
            app.UseHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready"),
                ResponseWriter = WriteHealthCheckResponse
            });
            app.UseHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = WriteHealthCheckResponse
            });
            return app;
        }

        /// <summary>
        /// Форматирует и записывает ответ проверки работоспособности в формате JSON.
        /// </summary>
        /// <param name="context">Контекст HTTP-запроса.</param>
        /// <param name="report">Отчет о проверке работоспособности.</param>
        /// <returns>Задача, представляющая асинхронную операцию записи ответа.</returns>
        private static Task WriteHealthCheckResponse(HttpContext context, HealthReport report)
        {
            context.Response.ContentType = "application/json";
            var response = new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.TotalMilliseconds
                }),
                totalDuration = report.TotalDuration.TotalMilliseconds
            };
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            return context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }
    }
}