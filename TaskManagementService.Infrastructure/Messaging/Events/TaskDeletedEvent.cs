using System;
using System.Text.Json.Serialization;

namespace TaskManagementService.Infrastructure.Messaging.Events
{
    /// <summary>
    /// Событие удаления задачи
    /// </summary>
    public record TaskDeletedEvent
    {
        public Guid EventId { get; init; }
        public Guid TaskId { get; init; }
        public DateTime Timestamp { get; init; }

        /// <summary>
        /// Создает событие удаления задачи с текущей временной меткой
        /// </summary>
        public TaskDeletedEvent(Guid taskId)
        {
            EventId = Guid.NewGuid();
            TaskId = taskId;
            Timestamp = DateTime.UtcNow;
        }
    }
}