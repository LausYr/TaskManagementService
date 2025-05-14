using System.Collections.Generic;
using System.Linq;
using TaskManagementService.Application.DTOs;
using TaskManagementService.Domain.Entities;

namespace TaskManagementService.Application.Mapping
{
    /// <summary>
    /// Класс для маппинга между доменными объектами и DTO
    /// </summary>
    public static class TaskMapper
    {
        /// <summary>
        /// Преобразует доменный объект TaskItem в DTO TaskDto
        /// </summary>
        /// <param name="task">Доменный объект задачи</param>
        /// <returns>DTO задачи</returns>
        public static TaskDto ToDto(this TaskItem task)
        {
            if (task == null) return null;

            return new TaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt
            };
        }

        /// <summary>
        /// Преобразует коллекцию доменных объектов TaskItem в коллекцию DTO TaskDto
        /// </summary>
        /// <param name="tasks">Коллекция доменных объектов задач</param>
        /// <returns>Коллекция DTO задач</returns>
        public static IEnumerable<TaskDto> ToDto(this IEnumerable<TaskItem> tasks)
        {
            return tasks?.Select(t => t.ToDto());
        }

        /// <summary>
        /// Создает объект TaskListDto на основе коллекции доменных объектов и параметров пагинации
        /// </summary>
        /// <param name="tasks">Коллекция доменных объектов задач</param>
        /// <param name="totalCount">Общее количество задач</param>
        /// <param name="page">Текущая страница</param>
        /// <param name="pageSize">Размер страницы</param>
        /// <returns>DTO списка задач с пагинацией</returns>
        public static TaskListDto ToTaskListDto(this IEnumerable<TaskItem> tasks, int totalCount, int page, int pageSize)
        {
            return new TaskListDto
            {
                Items = tasks.ToDto(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}