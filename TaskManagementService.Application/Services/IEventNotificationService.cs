using System;
using System.Threading.Tasks;
using TaskManagementService.Domain.Entities;

namespace TaskManagementService.Application.Services
{
    /// <summary>
    /// Интерфейс для сервиса отправки уведомлений о событиях задач
    /// </summary>
    public interface IEventNotificationService
    {
        /// <summary>
        /// Отправляет уведомление о создании задачи
        /// </summary>
        /// <param name="taskItem">Созданная задача</param>
        Task NotifyTaskCreatedAsync(TaskItem taskItem);

        /// <summary>
        /// Отправляет уведомление об обновлении задачи
        /// </summary>
        /// <param name="taskItem">Обновленная задача</param>
        Task NotifyTaskUpdatedAsync(TaskItem taskItem);

        /// <summary>
        /// Отправляет уведомление об удалении задачи
        /// </summary>
        /// <param name="taskId">Идентификатор удаленной задачи</param>
        Task NotifyTaskDeletedAsync(Guid taskId);
    }
}