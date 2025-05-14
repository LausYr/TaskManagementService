using System;
using System.Threading.Tasks;
using TaskManagementService.Application.DTOs;

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
        /// <param name="taskDto">Созданная задача</param>
        Task NotifyTaskCreatedAsync(TaskDto taskDto);

        /// <summary>
        /// Отправляет уведомление об обновлении задачи
        /// </summary>
        /// <param name="taskDto">Обновленная задача</param>
        Task NotifyTaskUpdatedAsync(TaskDto taskDto);

        /// <summary>
        /// Отправляет уведомление об удалении задачи
        /// </summary>
        /// <param name="taskId">Идентификатор удаленной задачи</param>
        Task NotifyTaskDeletedAsync(Guid taskId);
    }
}