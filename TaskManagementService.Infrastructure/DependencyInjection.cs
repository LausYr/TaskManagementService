using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Threading.Tasks;
using System.Threading;
using System;
using TaskManagementService.Common.Configuration;
using TaskManagementService.Domain.Repositories;
using TaskManagementService.Infrastructure.Data;
using TaskManagementService.Infrastructure.Messaging;
using TaskManagementService.Infrastructure.Repositories;
using Polly;

namespace TaskManagementService.Infrastructure
{
    /// <summary>
    /// Содержит методы расширения для регистрации инфраструктурных сервисов
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Добавляет инфраструктурные сервисы в контейнер зависимостей
        /// </summary>
        /// <param name="services">Коллекция сервисов</param>
        /// <param name="configuration">Конфигурация приложения</param>
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("Строка подключения 'DefaultConnection' не найдена");
                options.UseNpgsql(
                    connectionString,
                    b => b.MigrationsAssembly("TaskManagementService.Infrastructure"));
            });

            // Репозиторий
            services.AddScoped<ITaskItemRepository, TaskItemRepository>();

            return services;
        }

        /// <summary>
        /// Добавляет сервисы RabbitMQ в контейнер зависимостей
        /// </summary>
        /// <param name="services">Коллекция сервисов</param>
        /// <param name="configuration">Конфигурация приложения</param>
        public static IServiceCollection AddRabbitMQ(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RabbitMQConfiguration>(configuration.GetSection("RabbitMQ"));

            services.AddSingleton(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<RabbitMQConfiguration>>();
                var config = sp.GetRequiredService<IOptions<RabbitMQConfiguration>>().Value;
                logger.LogInformation(
                    "Загружена конфигурация RabbitMQ: Хост={Host}, Порт={Port}, ВиртуальныйХост={VirtualHost}, Обменник={Exchange}, Очередь={Queue}, DLX={DLX}, DLQ={DLQ}",
                    config.HostName, config.Port, config.VirtualHost, config.ExchangeName, config.QueueName, config.DLXExchangeName, config.DLQQueueName);
                return config;
            });

            services.AddSingleton<IConnectionFactory>(sp =>
            {
                var config = sp.GetRequiredService<RabbitMQConfiguration>();
                var logger = sp.GetRequiredService<ILogger<ConnectionFactory>>();
                try
                {
                    var factory = new ConnectionFactory
                    {
                        HostName = config.HostName,
                        Port = config.Port,
                        UserName = config.UserName,
                        Password = config.Password,
                        VirtualHost = config.VirtualHost,
                        DispatchConsumersAsync = true,
                        AutomaticRecoveryEnabled = true,
                        RequestedConnectionTimeout = TimeSpan.FromSeconds(30),
                        NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
                    };
                    logger.LogInformation("Настроена фабрика подключений RabbitMQ: Хост={Host}, Порт={Port}, ВиртуальныйХост={VirtualHost}",
                        factory.HostName, factory.Port, factory.VirtualHost);
                    return factory;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Не удалось настроить фабрику подключений RabbitMQ");
                    throw;
                }
            });

            services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();

            services.AddSingleton<RabbitMQInitializer>();
            services.AddHostedService<RabbitMQInitializerHostedService>();

            return services;
        }

        /// <summary>
        /// Сервис, инициализирующий RabbitMQ при запуске приложения
        /// </summary>
        public class RabbitMQInitializerHostedService : IHostedService
        {
            private readonly RabbitMQInitializer _initializer;
            private readonly ILogger<RabbitMQInitializerHostedService> _logger;

            /// <summary>
            /// Инициализирует новый экземпляр сервиса инициализации RabbitMQ
            /// </summary>
            /// <param name="initializer">Инициализатор RabbitMQ</param>
            /// <param name="logger">Логгер</param>
            public RabbitMQInitializerHostedService(
                RabbitMQInitializer initializer,
                ILogger<RabbitMQInitializerHostedService> logger)
            {
                _initializer = initializer;
                _logger = logger;
            }

            /// <summary>
            /// Запускает инициализацию RabbitMQ
            /// </summary>
            public async Task StartAsync(CancellationToken cancellationToken)
            {
                _logger.LogInformation("Ожидание готовности RabbitMQ...");

                const int maxAttempts = 5;
                for (int attempt = 1; attempt <= maxAttempts; attempt++)
                {
                    try
                    {
                        _initializer.Initialize();
                        _logger.LogInformation("RabbitMQ успешно инициализирован");
                        return;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Попытка {Attempt}/{MaxAttempts} инициализации RabbitMQ не удалась", attempt, maxAttempts);
                        if (attempt == maxAttempts)
                        {
                            _logger.LogError(ex, "Не удалось инициализировать RabbitMQ после {MaxAttempts} попыток", maxAttempts);
                            throw;
                        }
                        await Task.Delay(1000, cancellationToken);
                    }
                }
            }

            /// <summary>
            /// Останавливает сервис инициализации
            /// </summary>
            public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        }
    }
}