using System;
using System.Threading;
using System.Threading.Tasks;
using TaskManagementService.Application.DTOs;

namespace TaskManagementService.Application.Services
{
    /// <summary>
    /// Интерфейс сервиса для управления задачами
    /// </summary>
    public interface ITaskService
    {
        /// <summary>
        /// Получает список всех задач с пагинацией
        /// </summary>
        /// <param name="paginationParams">Параметры пагинации</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Список задач с информацией о пагинации</returns>
        Task<TaskListDto> GetAllTasksAsync(PaginationParams paginationParams, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получает задачу по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор задачи</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>DTO задачи</returns>
        Task<TaskDto> GetTaskByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Создает новую задачу
        /// </summary>
        /// <param name="createTaskDto">DTO для создания задачи</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>DTO созданной задачи</returns>
        Task<TaskDto> CreateTaskAsync(CreateTaskDto createTaskDto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Обновляет существующую задачу
        /// </summary>
        /// <param name="id">Идентификатор задачи</param>
        /// <param name="updateTaskDto">DTO для обновления задачи</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>DTO обновленной задачи</returns>
        Task<TaskDto> UpdateTaskAsync(Guid id, UpdateTaskDto updateTaskDto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Удаляет задачу по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор задачи</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        Task DeleteTaskAsync(Guid id, CancellationToken cancellationToken = default);
    }
}