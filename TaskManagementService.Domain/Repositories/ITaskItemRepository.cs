using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaskManagementService.Domain.Entities;

namespace TaskManagementService.Domain.Repositories
{
    /// <summary>
    /// Интерфейс репозитория для работы с задачами
    /// </summary>
    public interface ITaskItemRepository
    {
        /// <summary>
        /// Получает все задачи с пагинацией
        /// </summary>
        /// <param name="skip">Количество пропускаемых элементов</param>
        /// <param name="take">Количество получаемых элементов</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Список задач</returns>
        Task<IEnumerable<TaskItem>> GetAllAsync(int skip, int take, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получает общее количество задач
        /// </summary>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Количество задач</returns>
        Task<int> GetCountAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Получает задачу по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор задачи</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Задача или null, если задача не найдена</returns>
        Task<TaskItem> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Добавляет новую задачу
        /// </summary>
        /// <param name="taskItem">Задача для добавления</param>
        /// <param name="cancellationToken">Токен отмены</param>
        Task AddAsync(TaskItem taskItem, CancellationToken cancellationToken = default);

        /// <summary>
        /// Обновляет существующую задачу
        /// </summary>
        /// <param name="taskItem">Обновленная задача</param>
        /// <param name="cancellationToken">Токен отмены</param>
        Task UpdateAsync(TaskItem taskItem, CancellationToken cancellationToken = default);

        /// <summary>
        /// Удаляет задачу по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор задачи</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>true, если задача была удалена; иначе false</returns>
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}