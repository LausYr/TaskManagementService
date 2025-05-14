using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TaskManagementService.Common;
using TaskManagementService.Domain.Entities;
using TaskManagementService.Infrastructure.Messaging;
using TaskManagementService.Infrastructure.Messaging.Events;

namespace TaskManagementService.Application.Services
{
    /// <summary>
    ///   Сервис для отправки уведомлений о событиях задач через различные каналы связи 
    public class EventNotificationService : IEventNotificationService
    {
        private readonly IMessagePublisher _messagePublisher; private readonly HttpClient _httpClient; private readonly ILogger _logger;

        public EventNotificationService(
        IMessagePublisher messagePublisher,
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<EventNotificationService> logger)
        {
            _messagePublisher = messagePublisher ?? throw new ArgumentNullException(nameof(messagePublisher));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var listenerUrl = configuration["ListenerUrl"];
            if (string.IsNullOrEmpty(listenerUrl))
            {
                throw new InvalidOperationException("ListenerUrl не настроен в конфигурации");
            }
            _httpClient.BaseAddress = new Uri(listenerUrl);
        }

        /// <inheritdoc/>
        public async Task NotifyTaskCreatedAsync(TaskItem taskItem)
        {
            try
            {
                var @event = new TaskCreatedEvent(taskItem.Id, taskItem.Title, taskItem.Description, taskItem.Status);

                await _messagePublisher.PublishAsync(@event, MessageConstants.TaskCreatedRoutingKey);
                _logger.LogInformation("Отправлено уведомление о создании задачи в RabbitMQ: TaskId={TaskId}, EventId={EventId}", taskItem.Id, @event.EventId);

                var response = await _httpClient.PostAsJsonAsync("/api/v1/events/created", @event);
                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Отправлено уведомление о создании задачи в Listener: TaskId={TaskId}, EventId={EventId}", taskItem.Id, @event.EventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отправке уведомления о создании задачи с ID: {TaskId}", taskItem.Id);
            }
        }

        /// <inheritdoc/>
        public async Task NotifyTaskUpdatedAsync(TaskItem taskItem)
        {
            try
            {
                var @event = new TaskUpdatedEvent(taskItem.Id, taskItem.Title, taskItem.Description, taskItem.Status);

                await _messagePublisher.PublishAsync(@event, MessageConstants.TaskUpdatedRoutingKey);
                _logger.LogInformation("Отправлено уведомление об обновлении задачи в RabbitMQ: TaskId={TaskId}, EventId={EventId}", taskItem.Id, @event.EventId);

                var response = await _httpClient.PostAsJsonAsync("/api/v1/events/updated", @event);
                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Отправлено уведомление об обновлении задачи в Listener: TaskId={TaskId}, EventId={EventId}", taskItem.Id, @event.EventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отправке уведомления об обновлении задачи с ID: {TaskId}", taskItem.Id);
            }
        }

        /// <inheritdoc/>
        public async Task NotifyTaskDeletedAsync(Guid taskId)
        {
            try
            {
                var @event = new TaskDeletedEvent(taskId);

                await _messagePublisher.PublishAsync(@event, MessageConstants.TaskDeletedRoutingKey);
                _logger.LogInformation("Отправлено уведомление об удалении задачи в RabbitMQ: TaskId={TaskId}, EventId={EventId}", taskId, @event.EventId);

                var response = await _httpClient.PostAsJsonAsync("/api/v1/events/deleted", @event);
                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Отправлено уведомление об удалении задачи в Listener: TaskId={TaskId}, EventId={EventId}", taskId, @event.EventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отправке уведомления об удалении задачи с ID: {TaskId}", taskId);
            }
        }
    }
}