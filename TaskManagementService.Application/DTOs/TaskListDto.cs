using System.Collections.Generic;

namespace TaskManagementService.Application.DTOs
{
    /// <summary>
    /// DTO для списка задач с пагинацией
    /// </summary>
    public class TaskListDto
    {
        /// <summary>
        /// Список задач на текущей странице
        /// </summary>
        public IEnumerable<TaskDto> Items { get; set; }

        /// <summary>
        /// Общее количество задач
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Текущая страница
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Размер страницы
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Общее количество страниц
        /// </summary>
        public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
    }
}