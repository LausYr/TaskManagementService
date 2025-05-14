using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskManagementService.Application.DTOs;
using TaskManagementService.Application.Exceptions;
using TaskManagementService.Application.Mapping;
using TaskManagementService.Domain.Entities;
using TaskManagementService.Domain.Enums;
using TaskManagementService.Infrastructure.Data;

namespace TaskManagementService.Application.Services
{
    /// <summary>
    /// Сервис для управления задачами
    /// </summary>
    public class TaskService : ITaskService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<TaskService> _logger;

        public TaskService(
            AppDbContext dbContext,
            ILogger<TaskService> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<TaskListDto> GetAllTasksAsync(PaginationParams paginationParams, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Получение списка задач. Страница: {Page}, Размер страницы: {PageSize}",
                paginationParams.Page, paginationParams.PageSize);

            var skip = (paginationParams.Page - 1) * paginationParams.PageSize;
            var take = paginationParams.PageSize;

            var tasks = await _dbContext.TaskItems
                .OrderByDescending(t => t.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);

            var totalCount = await _dbContext.TaskItems.CountAsync(cancellationToken);

            return tasks.ToTaskListDto(totalCount, paginationParams.Page, paginationParams.PageSize);
        }

        /// <inheritdoc/>
        public async Task<TaskDto> GetTaskByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Получение задачи по ID: {TaskId}", id);

            var task = await _dbContext.TaskItems
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

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

            await _dbContext.TaskItems.AddAsync(task, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return task.ToDto();
        }

        /// <inheritdoc/>
        public async Task<TaskDto> UpdateTaskAsync(Guid id, UpdateTaskDto updateTaskDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Обновление задачи с ID: {TaskId}", id);

            var task = await _dbContext.TaskItems
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

            if (task == null)
            {
                _logger.LogWarning("Задача с ID: {TaskId} не найдена для обновления", id);
                throw new NotFoundException("Task", id);
            }

            task.Update(updateTaskDto.Title, updateTaskDto.Description, updateTaskDto.Status);

            _dbContext.TaskItems.Update(task);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return task.ToDto();
        }

        /// <inheritdoc/>
        public async Task DeleteTaskAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Удаление задачи с ID: {TaskId}", id);

            var task = await _dbContext.TaskItems
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

            if (task == null)
            {
                _logger.LogWarning("Задача с ID: {TaskId} не найдена для удаления", id);
                throw new NotFoundException("Task", id);
            }

            _dbContext.TaskItems.Remove(task);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}