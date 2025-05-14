using System.Threading.Tasks;

namespace TaskManagementService.Infrastructure.Messaging
{
    /// <summary>
    /// Интерфейс для публикации сообщений
    /// </summary>
    public interface IMessagePublisher
    {
        /// <summary>
        /// Публикует сообщение в очередь
        /// </summary>
        /// <typeparam name="T">Тип сообщения</typeparam>
        /// <param name="message">Сообщение для публикации</param>
        /// <param name="routingKey">Ключ маршрутизации</param>
        Task PublishAsync<T>(T message, string routingKey) where T : class;
    }
}