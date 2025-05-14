namespace TaskManagementService.Application.DTOs
{
    /// <summary>
    /// DTO для создания новой задачи
    /// </summary>
    public class CreateTaskDto
    {
        /// <summary>
        /// Название задачи
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Описание задачи
        /// </summary>
        public string Description { get; set; }
    }
}