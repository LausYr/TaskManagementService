using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TaskManagementService.Application.DTOs;
using TaskManagementService.Application.Exceptions;
using TaskManagementService.Application.Mapping;
using TaskManagementService.Domain.Entities;
using TaskManagementService.Domain.Repositories;

namespace TaskManagementService.Application.Services
{
    /// <summary>
    /// Сервис для управления задачами
    /// </summary>
    public class TaskService : ITaskService
    {
        private readonly ITaskItemRepository _taskRepository;
        private readonly IEventNotificationService _eventNotificationService;
        private readonly ILogger<TaskService> _logger;

        public TaskService(
            ITaskItemRepository taskRepository,
            IEventNotificationService eventNotificationService,
            ILogger<TaskService> logger)
        {
            _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
            _eventNotificationService = eventNotificationService ?? throw new ArgumentNullException(nameof(eventNotificationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<TaskListDto> GetAllTasksAsync(PaginationParams paginationParams, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Получение списка задач. Страница: {Page}, Размер страницы: {PageSize}",
                paginationParams.Page, paginationParams.PageSize);

            var skip = (paginationParams.Page - 1) * paginationParams.PageSize;
            var take = paginationParams.PageSize;

            var tasks = await _taskRepository.GetAllAsync(skip, take, cancellationToken);
            var totalCount = await _taskRepository.GetCountAsync(cancellationToken);

            return tasks.ToTaskListDto(totalCount, paginationParams.Page, paginationParams.PageSize);
        }

        /// <inheritdoc/>
        public async Task<TaskDto> GetTaskByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Получение задачи по ID: {TaskId}", id);

            var task = await _taskRepository.GetByIdAsync(id, cancellationToken);

            if (task == null)
            {
                _logger.LogWarning("Задача с ID: {TaskId} не найдена", id);
                throw new NotFoundException("Task", id);
            }

            return task.ToDto();
        }

        /// <inheritdoc/>
        public async Task<TaskDto> CreateTaskAsync(CreateTaskDto createTaskDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Создание новой задачи с названием: {Title}", createTaskDto.Title);

            var task = new TaskItem(createTaskDto.Title, createTaskDto.Description);

            await _taskRepository.AddAsync(task, cancellationToken);

            _ = Task.Run(async () =>
            {
                _logger.LogInformation("Начало фоновой отправки уведомления о создании задачи с ID: {TaskId}", task.Id);
                try
                {
                    await _eventNotificationService.NotifyTaskCreatedAsync(task);
                    _logger.LogInformation("Завершена фоновая отправка уведомления о создании задачи с ID: {TaskId}", task.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при фоновой отправке уведомления о создании задачи с ID: {TaskId}", task.Id);
                }
            }, cancellationToken);

            return task.ToDto();
        }
        /// <inheritdoc/>
        public async Task<TaskDto> UpdateTaskAsync(Guid id, UpdateTaskDto updateTaskDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Обновление задачи с ID: {TaskId}", id);

            var task = await _taskRepository.GetByIdAsync(id, cancellationToken);

            if (task == null)
            {
                _logger.LogWarning("Задача с ID: {TaskId} не найдена для обновления", id);
                throw new NotFoundException("Task", id);
            }

            task.Update(updateTaskDto.Title, updateTaskDto.Description, updateTaskDto.Status);

            await _taskRepository.UpdateAsync(task, cancellationToken);

            _ = Task.Run(async () =>
            {
                _logger.LogInformation("Начало фоновой отправки уведомления об обновлении задачи с ID: {TaskId}", id);
                try
                {
                    await _eventNotificationService.NotifyTaskUpdatedAsync(task);
                    _logger.LogInformation("Завершена фоновая отправка уведомления об обновлении задачи с ID: {TaskId}", id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при фоновой отправке уведомления об обновлении задачи с ID: {TaskId}", id);
                }
            }, cancellationToken);

            return task.ToDto();
        }

        /// <inheritdoc/>
        public async Task DeleteTaskAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Удаление задачи с ID: {TaskId}", id);

            var task = await _taskRepository.GetByIdAsync(id, cancellationToken);

            if (task == null)
            {
                _logger.LogWarning("Задача с ID: {TaskId} не найдена для удаления", id);
                throw new NotFoundException("Task", id);
            }

            await _taskRepository.DeleteAsync(id, cancellationToken);

            _ = Task.Run(async () =>
            {
                _logger.LogInformation("Начало фоновой отправки уведомления об удалении задачи с ID: {TaskId}", id);
                try
                {
                    await _eventNotificationService.NotifyTaskDeletedAsync(id);
                    _logger.LogInformation("Завершена фоновая отправка уведомления об удалении задачи с ID: {TaskId}", id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при фоновой отправке уведомления об удалении задачи с ID: {TaskId}", id);
                }
            }, cancellationToken);
        }
    }
}