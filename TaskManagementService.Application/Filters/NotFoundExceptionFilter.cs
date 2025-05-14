using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TaskManagementService.Application.Exceptions;

namespace TaskManagementService.Application.Filters
{
    /// <summary>
    /// Фильтр для обработки исключений типа NotFoundException.
    /// Преобразует исключения отсутствия ресурса в HTTP-ответ 404 Not Found.
    /// </summary>
    public class NotFoundExceptionFilter : IAsyncExceptionFilter
    {
        private readonly ILogger<NotFoundExceptionFilter> _logger;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="NotFoundExceptionFilter"/>.
        /// </summary>
        /// <param name="logger">Сервис логирования.</param>
        /// <exception cref="ArgumentNullException">Возникает, если logger равен null.</exception>
        public NotFoundExceptionFilter(ILogger<NotFoundExceptionFilter> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Обрабатывает исключения в контексте выполнения.
        /// Если исключение является NotFoundException, преобразует его в HTTP-ответ 404 Not Found.
        /// </summary>
        /// <param name="context">Контекст исключения.</param>
        public Task OnExceptionAsync(ExceptionContext context)
        {
            if (context.Exception is NotFoundException notFoundException)
            {
                _logger.LogWarning("Обработка NotFoundException: {Message}", notFoundException.Message);
                context.Result = new NotFoundObjectResult(new ProblemDetails
                {
                    Title = "Resource Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = notFoundException.Message
                });
                context.ExceptionHandled = true;
            }
            return Task.CompletedTask;
        }
    }
}