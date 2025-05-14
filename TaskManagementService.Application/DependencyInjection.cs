using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Net.Http;
using TaskManagementService.Application.Filters;
using TaskManagementService.Application.Services;
using TaskManagementService.Application.Validators;

namespace TaskManagementService.Application
{
    /// <summary>
    /// Класс для настройки зависимостей приложения
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Добавляет общие зависимости, используемые во всех компонентах
        /// </summary>
        public static IServiceCollection AddCommonDependencies(this IServiceCollection services)
        {
            services.AddScoped<ValidationFilter>();
            return services;
        }

        /// <summary>
        /// Добавляет основные компоненты уровня приложения
        /// </summary>
        public static IServiceCollection AddApplicationCore(this IServiceCollection services)
        {
            services.AddCommonDependencies();
            services.AddScoped<IEventNotificationService, EventNotificationService>();
            services.AddHttpClient<IEventNotificationService, EventNotificationService>()
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                .AddPolicyHandler(GetRetryPolicy());
            services.AddValidatorsFromAssemblyContaining<CreateTaskDtoValidator>();
            return services;
        }

        /// <summary>
        /// Добавляет компоненты управления задачами
        /// </summary>
        public static IServiceCollection AddTaskManagement(this IServiceCollection services)
        {
            services.AddCommonDependencies();
            services.AddScoped<ITaskService, TaskService>();
            services.AddScoped<NotFoundExceptionFilter>();
            return services;
        }

        /// <summary>
        /// Добавляет компоненты для обработки событий (Listener)
        /// </summary>
        public static IServiceCollection AddListenerCore(this IServiceCollection services)
        {
            services.AddCommonDependencies();
            services.AddValidatorsFromAssemblyContaining<TaskDeletedEventValidator>();
            return services;
        }

        /// <summary>
        /// Создает политику повторных попыток для HTTP-запросов
        /// </summary>
        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }
    }
}