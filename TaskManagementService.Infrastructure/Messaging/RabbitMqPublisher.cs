using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using TaskManagementService.Common.Configuration;

namespace TaskManagementService.Infrastructure.Messaging
{
    /// <summary>
    /// Издатель сообщений RabbitMQ с использованием Polly для обработки повторных попыток
    /// </summary>
    public class RabbitMqPublisher : IMessagePublisher, IDisposable
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly ILogger<RabbitMqPublisher> _logger;
        private readonly RabbitMQConfiguration _config;
        private IConnection _connection;
        private readonly object _connectionLock = new object();
        private readonly AsyncRetryPolicy _retryPolicy;
        private bool _disposed;

        /// <summary>
        /// Инициализирует новый экземпляр издателя RabbitMQ
        /// </summary>
        /// <param name="connectionFactory">Фабрика подключений к RabbitMQ</param>
        /// <param name="logger">Логгер</param>
        /// <param name="config">Конфигурация RabbitMQ</param>
        public RabbitMqPublisher(
            IConnectionFactory connectionFactory,
            ILogger<RabbitMqPublisher> logger,
            RabbitMQConfiguration config)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));


            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt - 1)),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(exception,
                            "Ошибка при публикации сообщения. Повторная попытка {RetryCount} через {RetryInterval}. Ключ маршрутизации: {RoutingKey}",
                            retryCount, timeSpan, context["routingKey"]);
                        
                        EnsureConnection();
                    });

            try
            {
                _connection = _connectionFactory.CreateConnection();
                _logger.LogInformation("Publisher RabbitMQ инициализирован. Обменник: {Exchange}", _config.ExchangeName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не удалось инициализировать publisher RabbitMQ");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task PublishAsync<T>(T message, string routingKey) where T : class
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            if (string.IsNullOrWhiteSpace(routingKey))
                throw new ArgumentException("Ключ маршрутизации не может быть пустым", nameof(routingKey));

            EnsureConnection();

            var context = new Context
            {
                ["routingKey"] = routingKey,
                ["messageType"] = message.GetType().Name
            };

            await _retryPolicy.ExecuteAsync(async (ctx) =>
            {
                using var channel = _connection.CreateModel();
                var messageJson = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(messageJson);

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;
                properties.ContentType = "application/json";
                properties.MessageId = Guid.NewGuid().ToString();
                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                properties.CorrelationId = Guid.NewGuid().ToString();

                channel.BasicPublish(
                    exchange: _config.ExchangeName,
                    routingKey: routingKey,
                    basicProperties: properties,
                    body: body);

                _logger.LogInformation("Опубликовано сообщение. Тип: {MessageType}, Ключ маршрутизации: {RoutingKey}, MessageId: {MessageId}",
                    message.GetType().Name, routingKey, properties.MessageId);

                return Task.CompletedTask;
            }, context);
        }

        /// <summary>
        /// Проверяет и при необходимости восстанавливает соединение с RabbitMQ
        /// </summary>
        private void EnsureConnection()
        {
            if (_connection != null && _connection.IsOpen)
                return;

            lock (_connectionLock)
            {
                if (_connection != null && _connection.IsOpen)
                    return;

                try
                {
                    _logger.LogWarning("Соединение с RabbitMQ закрыто или не инициализировано. Переподключение...");

                    _connection?.Dispose();

                    _connection = _connectionFactory.CreateConnection();
                    _logger.LogInformation("Соединение с RabbitMQ успешно восстановлено");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Не удалось восстановить соединение с RabbitMQ");
                    throw;
                }
            }
        }

        /// <summary>
        /// Освобождает ресурсы, используемые издателем
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            try
            {
                _connection?.Close();
                _connection?.Dispose();
                _logger.LogInformation("Соединение publisher RabbitMQ закрыто");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при освобождении ресурсов publisher RabbitMQ");
            }

            _disposed = true;
        }
    }
}