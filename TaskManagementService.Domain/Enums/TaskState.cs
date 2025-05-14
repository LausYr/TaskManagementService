namespace TaskManagementService.Domain.Enums
{
    /// <summary>
    /// Статус задачи
    /// </summary>
    public enum TaskState
    {
        /// <summary>
        /// Новая задача
        /// </summary>
        New = 0,

        /// <summary>
        /// Задача в процессе выполнения
        /// </summary>
        InProgress = 1,

        /// <summary>
        /// Задача выполнена
        /// </summary>
        Completed = 2
    }
}