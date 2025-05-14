using System;
using System.Text.Json.Serialization;
using TaskManagementService.Domain.Enums;

namespace TaskManagementService.Infrastructure.Messaging.Events
{
    /// <summary>
    /// Событие обновления задачи
    /// </summary>
    public record TaskUpdatedEvent
    {
        public Guid EventId { get; init; }
        public Guid TaskId { get; init; }
        public string Title { get; init; } = null!;
        public string Description { get; init; } = null!;
        public TaskState Status { get; init; }
        public DateTime Timestamp { get; init; }


        /// <summary>
        /// Создает событие обновления задачи с текущей временной меткой
        /// </summary>
        public TaskUpdatedEvent(Guid taskId, string title, string description, TaskState status)
        {
            EventId = Guid.NewGuid();
            TaskId = taskId;
            Title = title;
            Description = description;
            Status = status;
            Timestamp = DateTime.UtcNow;
        }
    }
}