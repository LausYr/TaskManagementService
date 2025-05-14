using System;
using TaskManagementService.Domain.Enums;

namespace TaskManagementService.Domain.Entities
{
    /// <summary>
    /// Сущность задачи
    /// </summary>
    public class TaskItem
    {
        /// <summary>
        /// Уникальный идентификатор задачи
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Название задачи
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Описание задачи
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Статус задачи
        /// </summary>
        public TaskState Status { get; private set; }

        /// <summary>
        /// Дата создания задачи
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Дата последнего обновления задачи
        /// </summary>
        public DateTime UpdatedAt { get; private set; }

        // Приватный конструктор для EF Core
        private TaskItem() { }

        /// <summary>
        /// Создает новую задачу
        /// </summary>
        /// <param name="title">Название задачи</param>
        /// <param name="description">Описание задачи</param>
        public TaskItem(string title, string description)
        {
            Id = Guid.NewGuid();
            Title = title;
            Description = description ?? string.Empty;
            Status = TaskState.New;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = CreatedAt;
        }

        /// <summary>
        /// Обновляет информацию о задаче
        /// </summary>
        /// <param name="title">Новое название задачи</param>
        /// <param name="description">Новое описание задачи</param>
        /// <param name="status">Новый статус задачи</param>
        public void Update(string title, string description, TaskState status)
        {
            Title = title;
            Description = description ?? string.Empty;
            Status = status;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Изменяет статус задачи
        /// </summary>
        /// <param name="status">Новый статус задачи</param>
        public void ChangeStatus(TaskState status)
        {
            Status = status;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}