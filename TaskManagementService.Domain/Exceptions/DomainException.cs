using System;

namespace TaskManagementService.Domain.Exceptions
{
    /// <summary>
    /// Исключение, возникающее при нарушении доменных правил
    /// </summary>
    public class DomainException : Exception
    {
        /// <summary>
        /// Создает новый экземпляр исключения с указанным сообщением
        /// </summary>
        /// <param name="message">Сообщение об ошибке</param>
        public DomainException(string message) : base(message)
        {
        }

        /// <summary>
        /// Создает новый экземпляр исключения с указанным сообщением и внутренним исключением
        /// </summary>
        /// <param name="message">Сообщение об ошибке</param>
        /// <param name="innerException">Внутреннее исключение</param>
        public DomainException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}