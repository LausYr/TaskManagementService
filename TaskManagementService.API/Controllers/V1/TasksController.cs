using Microsoft.AspNetCore.Mvc;
using TaskManagementService.Application.DTOs;
using TaskManagementService.Application.Services;
using Asp.Versioning;
using TaskManagementService.Application.Filters;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TaskManagementService.API.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Produces("application/json")]
    [TypeFilter(typeof(NotFoundExceptionFilter))]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly IEventNotificationService _eventNotificationService;
        private readonly ILogger<TasksController> _logger;

        public TasksController(
            ITaskService taskService,
            IEventNotificationService eventNotificationService,
            ILogger<TasksController> logger)
        {
            _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
            _eventNotificationService = eventNotificationService ?? throw new ArgumentNullException(nameof(eventNotificationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [ProducesResponseType(typeof(TaskListDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TaskListDto>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Получение списка задач: страница {Page}, размер страницы {PageSize}", page, pageSize);
            var paginationParams = new PaginationParams { Page = page, PageSize = pageSize };
            var result = await _taskService.GetAllTasksAsync(paginationParams, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TaskDto>> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Получение задачи по ID: {TaskId}", id);
            var task = await _taskService.GetTaskByIdAsync(id, cancellationToken);
            return Ok(task);
        }

        [HttpPost]
        [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TaskDto>> Create(
            [FromBody] CreateTaskDto createTaskDto,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Создание новой задачи: {TaskTitle}", createTaskDto.Title);

            // Создаем задачу через сервис
            var createdTask = await _taskService.CreateTaskAsync(createTaskDto, cancellationToken);

            // Отправляем уведомление напрямую из контроллера, используя TaskDto
            try
            {
                await _eventNotificationService.NotifyTaskCreatedAsync(createdTask);
                _logger.LogInformation("Отправлено уведомление о создании задачи: {TaskId}", createdTask.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отправке уведомления о создании задачи: {TaskId}", createdTask.Id);
            }

            return CreatedAtAction(nameof(GetById), new { id = createdTask.Id, version = HttpContext.GetRequestedApiVersion()?.ToString() }, createdTask);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TaskDto>> Update(
            Guid id,
            [FromBody] UpdateTaskDto updateTaskDto,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Обновление задачи: {TaskId}", id);

            // Обновляем задачу через сервис
            var updatedTask = await _taskService.UpdateTaskAsync(id, updateTaskDto, cancellationToken);

            // Отправляем уведомление напрямую из контроллера, используя TaskDto
            try
            {
                await _eventNotificationService.NotifyTaskUpdatedAsync(updatedTask);
                _logger.LogInformation("Отправлено уведомление об обновлении задачи: {TaskId}", updatedTask.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отправке уведомления об обновлении задачи: {TaskId}", updatedTask.Id);
            }

            return Ok(updatedTask);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Удаление задачи: {TaskId}", id);

            // Удаляем задачу через сервис
            await _taskService.DeleteTaskAsync(id, cancellationToken);

            // Отправляем уведомление напрямую из контроллера
            try
            {
                await _eventNotificationService.NotifyTaskDeletedAsync(id);
                _logger.LogInformation("Отправлено уведомление об удалении задачи: {TaskId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отправке уведомления об удалении задачи: {TaskId}", id);
            }

            return NoContent();
        }
    }
}