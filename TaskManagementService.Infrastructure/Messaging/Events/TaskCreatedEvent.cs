using System;
using System.Text.Json.Serialization;
using TaskManagementService.Domain.Enums;

namespace TaskManagementService.Infrastructure.Messaging.Events
{
    /// <summary>
    /// Событие создания задачи
    /// </summary>
    public record TaskCreatedEvent
    {
        public Guid EventId { get; init; }
        public Guid TaskId { get; init; }
        public string Title { get; init; } = null!;
        public string Description { get; init; } = null!;
        public TaskState Status { get; init; }
        public DateTime Timestamp { get; init; }

        public TaskCreatedEvent(Guid taskId, string title, string description, TaskState status)
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