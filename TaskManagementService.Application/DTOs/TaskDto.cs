using System;
using TaskManagementService.Domain.Enums;

namespace TaskManagementService.Application.DTOs
{
    /// <summary>
    /// DTO для представления задачи
    /// </summary>
    public class TaskDto
    {
        /// <summary>
        /// Уникальный идентификатор задачи
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Название задачи
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Описание задачи
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Текущий статус задачи
        /// </summary>
        public TaskState Status { get; set; }

        /// <summary>
        /// Дата создания задачи
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Дата последнего обновления задачи
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}