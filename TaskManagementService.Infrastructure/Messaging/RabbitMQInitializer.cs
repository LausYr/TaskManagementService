using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using TaskManagementService.Common;
using TaskManagementService.Common.Configuration;

namespace TaskManagementService.Infrastructure.Messaging
{
    /// <summary>
    /// Инициализатор инфраструктуры RabbitMQ
    /// </summary>
    public class RabbitMQInitializer
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly ILogger<RabbitMQInitializer> _logger;
        private readonly RabbitMQConfiguration _config;

        /// <summary>
        /// Инициализирует новый экземпляр инициализатора RabbitMQ
        /// </summary>
        /// <param name="connectionFactory">Фабрика подключений к RabbitMQ</param>
        /// <param name="logger">Логгер</param>
        /// <param name="config">Конфигурация RabbitMQ</param>
        public RabbitMQInitializer(
            IConnectionFactory connectionFactory,
            ILogger<RabbitMQInitializer> logger,
            RabbitMQConfiguration config)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Инициализирует очереди и обменники RabbitMQ
        /// </summary>
        public void Initialize()
        {
            if (_config.RoutingKeys == null || !_config.RoutingKeys.Any())
                throw new ArgumentException("Необходимо указать ключи маршрутизации", nameof(_config.RoutingKeys));

            var policy = Policy
                .Handle<Exception>()
                .WaitAndRetry(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (ex, time, attempt, context) =>
                    {
                        _logger.LogWarning(ex, "Повторная попытка {Attempt} инициализации соединения с RabbitMQ", attempt);
                    });

            policy.Execute(() =>
            {
                try
                {
                    using var connection = _connectionFactory.CreateConnection();
                    using var channel = connection.CreateModel();


                    channel.ExchangeDeclare(
                        exchange: _config.ExchangeName,
                        type: ExchangeType.Topic,
                        durable: true,
                        autoDelete: false);

                    channel.ExchangeDeclare(
                        exchange: _config.DLXExchangeName,
                        type: ExchangeType.Topic,
                        durable: true,
                        autoDelete: false);

                    var queueArgs = new Dictionary<string, object>
                    {
                        { "x-dead-letter-exchange", _config.DLXExchangeName },
                        { "x-dead-letter-routing-key", MessageConstants.DLQRoutingKey }
                    };

                    channel.QueueDeclare(
                        queue: _config.QueueName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: queueArgs);

                    channel.QueueDeclare(
                        queue: _config.DLQQueueName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);

                    foreach (var routingKey in _config.RoutingKeys)
                    {
                        channel.QueueBind(
                            queue: _config.QueueName,
                            exchange: _config.ExchangeName,
                            routingKey: routingKey);
                        _logger.LogInformation("Очередь {QueueName} привязана к ключу маршрутизации {RoutingKey}", _config.QueueName, routingKey);
                    }

                    channel.QueueBind(
                        queue: _config.DLQQueueName,
                        exchange: _config.DLXExchangeName,
                        routingKey: MessageConstants.DLQRoutingKey);
                    _logger.LogInformation("Очередь DLQ {DLQQueueName} привязана к обменнику DLX {DLXExchangeName} с ключом маршрутизации {DLQRoutingKey}",
                        _config.DLQQueueName, _config.DLXExchangeName, MessageConstants.DLQRoutingKey);

                    _logger.LogInformation("Очереди и обменники RabbitMQ успешно инициализированы");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Не удалось инициализировать очереди и обменники RabbitMQ. Внутреннее исключение: {InnerException}",
                        ex.InnerException?.Message ?? "Нет");
                    throw;
                }
            });
        }
    }
}