using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TaskManagementService.Common;
using TaskManagementService.Common.Configuration;
using TaskManagementService.Infrastructure.Messaging.Events;

namespace TaskManagementService.Infrastructure.Messaging
{
    /// <summary>
    /// Потребитель сообщений RabbitMQ, обрабатывающий события задач
    /// </summary>
    public class RabbitMqConsumer : IHostedService, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<RabbitMqConsumer> _logger;
        private readonly RabbitMQConfiguration _config;
        private bool _disposed;
        private readonly int _maxRetries = 2;

        /// <summary>
        /// Инициализирует новый экземпляр потребителя RabbitMQ
        /// </summary>
        /// <param name="connectionFactory">Фабрика подключений к RabbitMQ</param>
        /// <param name="logger">Логгер</param>
        /// <param name="config">Конфигурация RabbitMQ</param>
        public RabbitMqConsumer(
            IConnectionFactory connectionFactory,
            ILogger<RabbitMqConsumer> logger,
            RabbitMQConfiguration config)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));

            try
            {
                _connection = connectionFactory.CreateConnection();
                _channel = _connection.CreateModel();
                _logger.LogInformation("Сonsumer RabbitMQ инициализирован. Очередь: {Queue}", _config.QueueName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не удалось инициализировать сonsumer RabbitMQ");
                throw;
            }
        }

        /// <summary>
        /// Запускает потребителя сообщений
        /// </summary>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var retryCount = GetRetryCount(ea.BasicProperties);
                var messageId = ea.BasicProperties.MessageId ?? "неизвестно";
                var correlationId = ea.BasicProperties.CorrelationId ?? "неизвестно";

                using var logScope = _logger.BeginScope(new Dictionary<string, object>
                {
                    ["MessageId"] = messageId,
                    ["CorrelationId"] = correlationId,
                    ["RetryCount"] = retryCount
                });

                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var routingKey = ea.RoutingKey;

                    _logger.LogInformation("Получено сообщение. Ключ маршрутизации: {RoutingKey}", routingKey);

                    switch (routingKey)
                    {
                        case MessageConstants.TaskCreatedRoutingKey:
                            var createdEvent = JsonSerializer.Deserialize<TaskCreatedEvent>(message)
                                ?? throw new JsonException("Не удалось десериализовать TaskCreatedEvent");
                            _logger.LogInformation(
                                "Событие создания задачи: EventId={EventId}, TaskId={TaskId}, Название={Title}, Статус={Status}",
                                createdEvent.EventId, createdEvent.TaskId, createdEvent.Title, createdEvent.Status);
                            break;

                        case MessageConstants.TaskUpdatedRoutingKey:
                            var updatedEvent = JsonSerializer.Deserialize<TaskUpdatedEvent>(message)
                                ?? throw new JsonException("Не удалось десериализовать TaskUpdatedEvent");
                            _logger.LogInformation(
                                "Событие обновления задачи: EventId={EventId}, TaskId={TaskId}, Название={Title}, Статус={Status}",
                                updatedEvent.EventId, updatedEvent.TaskId, updatedEvent.Title, updatedEvent.Status);
                            break;

                        case MessageConstants.TaskDeletedRoutingKey:
                            var deletedEvent = JsonSerializer.Deserialize<TaskDeletedEvent>(message)
                                ?? throw new JsonException("Не удалось десериализовать TaskDeletedEvent");
                            _logger.LogInformation(
                                "Событие удаления задачи: EventId={EventId}, TaskId={TaskId}",
                                deletedEvent.EventId, deletedEvent.TaskId);
                            break;

                        default:
                            _logger.LogWarning("Неизвестный ключ маршрутизации: {RoutingKey}. Подтверждаем сообщение.", routingKey);
                            break;
                    }

                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    _logger.LogInformation("Сообщение успешно обработано");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Не удалось обработать сообщение. Ключ маршрутизации: {RoutingKey}", ea.RoutingKey);

                    if (retryCount >= _maxRetries)
                    {
                        _logger.LogError("Достигнуто максимальное количество повторов для сообщения. Отправка в DLQ. Ключ маршрутизации: {RoutingKey}", ea.RoutingKey);
                        _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                    }
                    else
                    {
                        var properties = ea.BasicProperties;
                        if (properties.Headers == null)
                            properties.Headers = new Dictionary<string, object>();
                        properties.Headers["x-retry-count"] = retryCount + 1;

                        try
                        {
                            _channel.BasicPublish(
                                exchange: _config.ExchangeName,
                                routingKey: ea.RoutingKey,
                                basicProperties: properties,
                                body: ea.Body);

                            _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                            _logger.LogInformation("Сообщение поставлено в очередь для повторной обработки. Количество повторов: {RetryCount}", retryCount + 1);
                        }
                        catch (Exception publishEx)
                        {
                            _logger.LogError(publishEx, "Не удалось поставить сообщение в очередь повторно. Отправка в DLQ. Ключ маршрутизации: {RoutingKey}", ea.RoutingKey);
                            _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                        }
                    }
                }
            };

            try
            {
                _channel.BasicConsume(
                    queue: _config.QueueName,
                    autoAck: false,
                    consumer: consumer);

                _logger.LogInformation("Сonsumer запущен. Ожидание сообщений из очереди {QueueName}", _config.QueueName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не удалось запустить сonsumer RabbitMQ для очереди {QueueName}", _config.QueueName);
                throw;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Получает количество повторных попыток обработки сообщения
        /// </summary>
        private int GetRetryCount(IBasicProperties properties)
        {
            if (properties?.Headers != null && properties.Headers.TryGetValue("x-retry-count", out var retryCountObj))
            {
                return Convert.ToInt32(retryCountObj);
            }
            return 0;
        }

        /// <summary>
        /// Останавливает потребителя сообщений
        /// </summary>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Остановка сonsumer RabbitMQ");
            Dispose();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Освобождает ресурсы, используемые потребителем
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            try
            {
                _channel?.Close();
                _channel?.Dispose();
                _connection?.Close();
                _connection?.Dispose();
                _logger.LogInformation("Соединение сonsumer RabbitMQ закрыто");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при освобождении ресурсов сonsumer RabbitMQ");
            }

            _disposed = true;
        }
    }
}