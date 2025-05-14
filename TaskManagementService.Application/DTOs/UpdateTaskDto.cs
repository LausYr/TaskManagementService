using System.Text.Json.Serialization;
using TaskManagementService.Domain.Enums;

namespace TaskManagementService.Application.DTOs
{
    /// <summary>
    /// DTO для обновления существующей задачи
    /// </summary>
    public class UpdateTaskDto
    {
        /// <summary>
        /// Название задачи
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Описание задачи
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Статус задачи
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TaskState Status { get; set; }
    }
}