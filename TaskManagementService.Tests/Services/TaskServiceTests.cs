using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManagementService.Application.DTOs;
using TaskManagementService.Application.Exceptions;
using TaskManagementService.Application.Services;
using TaskManagementService.Domain.Entities;
using TaskManagementService.Domain.Enums;
using TaskManagementService.Infrastructure.Data;
using Xunit;

namespace TaskManagementService.Tests.Application.Services
{
    public class TaskServiceTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<AppDbContext> _contextOptions;
        private readonly Mock<ILogger<TaskService>> _mockLogger;
        private readonly List<TaskItem> _taskItems;

        public TaskServiceTests()
        {
            // Создаем соединение SQLite в памяти
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            // Настраиваем опции контекста для использования SQLite
            _contextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

            // Создаем схему базы данных
            using var context = new AppDbContext(_contextOptions);
            context.Database.EnsureCreated();

            // Создаем тестовые данные
            _taskItems = new List<TaskItem>
            {
                new TaskItem("Task 1", "Description 1"),
                new TaskItem("Task 2", "Description 2"),
                new TaskItem("Task 3", "Description 3")
            };

            // Добавляем тестовые данные в базу
            context.TaskItems.AddRange(_taskItems);
            context.SaveChanges();

            // Настраиваем мок для Logger
            _mockLogger = new Mock<ILogger<TaskService>>();
        }

        public void Dispose()
        {
            _connection.Close();
        }

        private AppDbContext CreateContext()
        {
            return new AppDbContext(_contextOptions);
        }

        [Fact]
        public async Task GetAllTasksAsync_ShouldReturnPaginatedTasks()
        {
            // Arrange
            using var context = CreateContext();
            var taskService = new TaskService(context, _mockLogger.Object);
            var paginationParams = new PaginationParams { Page = 1, PageSize = 2 };

            // Act
            var result = await taskService.GetAllTasksAsync(paginationParams);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count());
            Assert.Equal(3, result.TotalCount);
            Assert.Equal(1, result.Page);
            Assert.Equal(2, result.PageSize);
            Assert.Equal(2, result.TotalPages);
        }

        [Fact]
        public async Task GetTaskByIdAsync_WithValidId_ShouldReturnTask()
        {
            // Arrange
            using var context = CreateContext();
            var taskService = new TaskService(context, _mockLogger.Object);
            var taskId = _taskItems[0].Id;

            // Act
            var result = await taskService.GetTaskByIdAsync(taskId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(taskId, result.Id);
            Assert.Equal(_taskItems[0].Title, result.Title);
            Assert.Equal(_taskItems[0].Description, result.Description);
        }

        [Fact]
        public async Task GetTaskByIdAsync_WithInvalidId_ShouldThrowNotFoundException()
        {
            // Arrange
            using var context = CreateContext();
            var taskService = new TaskService(context, _mockLogger.Object);
            var invalidTaskId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                taskService.GetTaskByIdAsync(invalidTaskId));
        }

        [Fact]
        public async Task CreateTaskAsync_ShouldCreateAndReturnTask()
        {
            // Arrange
            using var context = CreateContext();
            var taskService = new TaskService(context, _mockLogger.Object);
            var createTaskDto = new CreateTaskDto
            {
                Title = "New Task",
                Description = "New Description"
            };

            // Act
            var result = await taskService.CreateTaskAsync(createTaskDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createTaskDto.Title, result.Title);
            Assert.Equal(createTaskDto.Description, result.Description);
            Assert.Equal(TaskState.New, result.Status);

            // Проверяем, что задача добавлена в базу
            using var verifyContext = CreateContext();
            var taskInDb = await verifyContext.TaskItems.FirstOrDefaultAsync(t => t.Id == result.Id);
            Assert.NotNull(taskInDb);
            Assert.Equal(createTaskDto.Title, taskInDb.Title);
        }

        [Fact]
        public async Task UpdateTaskAsync_WithValidId_ShouldUpdateAndReturnTask()
        {
            // Arrange
            using var context = CreateContext();
            var taskService = new TaskService(context, _mockLogger.Object);
            var taskId = _taskItems[0].Id;
            var updateTaskDto = new UpdateTaskDto
            {
                Title = "Updated Title",
                Description = "Updated Description",
                Status = TaskState.InProgress
            };

            // Act
            var result = await taskService.UpdateTaskAsync(taskId, updateTaskDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(updateTaskDto.Title, result.Title);
            Assert.Equal(updateTaskDto.Description, result.Description);
            Assert.Equal(updateTaskDto.Status, result.Status);

            // Проверяем, что задача обновлена в базе
            using var verifyContext = CreateContext();
            var taskInDb = await verifyContext.TaskItems.FirstOrDefaultAsync(t => t.Id == taskId);
            Assert.NotNull(taskInDb);
            Assert.Equal(updateTaskDto.Title, taskInDb.Title);
            Assert.Equal(updateTaskDto.Description, taskInDb.Description);
            Assert.Equal(updateTaskDto.Status, taskInDb.Status);
        }

        [Fact]
        public async Task UpdateTaskAsync_WithInvalidId_ShouldThrowNotFoundException()
        {
            // Arrange
            using var context = CreateContext();
            var taskService = new TaskService(context, _mockLogger.Object);
            var invalidTaskId = Guid.NewGuid();
            var updateTaskDto = new UpdateTaskDto
            {
                Title = "Updated Title",
                Description = "Updated Description",
                Status = TaskState.InProgress
            };

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                taskService.UpdateTaskAsync(invalidTaskId, updateTaskDto));
        }

        [Fact]
        public async Task DeleteTaskAsync_WithValidId_ShouldDeleteTask()
        {
            // Arrange
            using var context = CreateContext();
            var taskService = new TaskService(context, _mockLogger.Object);
            var taskId = _taskItems[0].Id;

            // Act
            await taskService.DeleteTaskAsync(taskId);

            // Assert
            // Проверяем, что задача удалена из базы
            using var verifyContext = CreateContext();
            var taskInDb = await verifyContext.TaskItems.FirstOrDefaultAsync(t => t.Id == taskId);
            Assert.Null(taskInDb);
        }

        [Fact]
        public async Task DeleteTaskAsync_WithInvalidId_ShouldThrowNotFoundException()
        {
            // Arrange
            using var context = CreateContext();
            var taskService = new TaskService(context, _mockLogger.Object);
            var invalidTaskId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                taskService.DeleteTaskAsync(invalidTaskId));
        }
    }
}