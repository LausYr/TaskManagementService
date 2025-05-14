using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
namespace TaskManagementService.Application.Filters
{
    /// <summary>
    /// Фильтр для валидации входных данных запроса с использованием FluentValidation.
    /// Проверяет аргументы действий контроллера и возвращает ошибку 400 Bad Request при обнаружении проблем валидации.
    /// </summary>
    public class ValidationFilter : IAsyncActionFilter
    {
        private readonly ILogger<ValidationFilter> _logger;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ValidationFilter"/>.
        /// </summary>
        /// <param name="logger">Сервис логирования.</param>
        /// <exception cref="ArgumentNullException">Возникает, если logger равен null.</exception>
        public ValidationFilter(ILogger<ValidationFilter> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Выполняет валидацию аргументов действия перед его выполнением.
        /// Проверяет наличие null-аргументов и применяет соответствующие валидаторы FluentValidation.
        /// </summary>
        /// <param name="context">Контекст выполнения действия.</param>
        /// <param name="next">Делегат для выполнения следующего фильтра или действия.</param>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            foreach (var arg in context.ActionArguments.Values)
            {
                if (arg == null)
                {
                    _logger.LogWarning("Получен null-аргумент в запросе");
                    context.Result = new BadRequestObjectResult(new ProblemDetails
                    {
                        Title = "Validation failed",
                        Status = StatusCodes.Status400BadRequest,
                        Detail = "One or more arguments are null."
                    });
                    return;
                }

                var validatorType = typeof(IValidator<>).MakeGenericType(arg.GetType());
                var validator = context.HttpContext.RequestServices.GetService(validatorType) as IValidator;

                if (validator != null)
                {
                    var validationResult = await validator.ValidateAsync(new ValidationContext<object>(arg));
                    if (!validationResult.IsValid)
                    {
                        var errors = string.Join("; ", validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
                        _logger.LogWarning("Ошибки валидации для {Type}: {Errors}", arg.GetType().Name, errors);
                        context.Result = new BadRequestObjectResult(new ProblemDetails
                        {
                            Title = "Validation failed",
                            Status = StatusCodes.Status400BadRequest,
                            Detail = "One or more validation errors occurred.",
                            Extensions = new Dictionary<string, object>
                            {
                                ["errors"] = validationResult.Errors
                                    .GroupBy(e => e.PropertyName)
                                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
                            }
                        });
                        return;
                    }
                }
            }

            await next();
        }
    }
}