using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManagementService.Application.DTOs;
using TaskManagementService.Application.Exceptions;
using TaskManagementService.Application.Services;
using TaskManagementService.Domain.Entities;
using TaskManagementService.Domain.Enums;
using TaskManagementService.Domain.Repositories;

namespace TaskManagementService.Tests.Services
{
    public class TaskServiceTests
    {
        private readonly Mock<ITaskItemRepository> _taskRepositoryMock;
        private readonly Mock<IEventNotificationService> _notificationServiceMock;
        private readonly Mock<ILogger<TaskService>> _loggerMock;
        private readonly TaskService _taskService;

        public TaskServiceTests()
        {
            _taskRepositoryMock = new Mock<ITaskItemRepository>();
            _notificationServiceMock = new Mock<IEventNotificationService>();
            _loggerMock = new Mock<ILogger<TaskService>>();
            _taskService = new TaskService(_taskRepositoryMock.Object, _notificationServiceMock.Object, _loggerMock.Object);
        }

        #region GetAllTasksAsync

        [Fact]
        public async Task GetAllTasksAsync_ReturnsTaskListDto_WithCorrectPagination()
        {
            // Arrange
            var paginationParams = new PaginationParams { Page = 2, PageSize = 2 };
            var tasks = CreateTaskItems(2);
            SetupRepositoryGetAll(2, 2, tasks, 5);

            // Act
            var result = await _taskService.GetAllTasksAsync(paginationParams);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.TotalCount);
            Assert.Equal(2, result.Page);
            Assert.Equal(2, result.PageSize);
            Assert.Equal(3, result.TotalPages);
            Assert.Equal(2, result.Items.Count());
            Assert.Equal("Task 0", result.Items.First().Title);
            VerifyRepositoryGetAll(2, 2, Times.Once());
            VerifyLoggerCalled(LogLevel.Information, Times.Once());
        }

        [Fact]
        public async Task GetAllTasksAsync_WithEmptyTasks_ReturnsEmptyTaskListDto()
        {
            // Arrange
            var paginationParams = new PaginationParams { Page = 1, PageSize = 10 };
            SetupRepositoryGetAll(0, 10, new List<TaskItem>(), 0);

            // Act
            var result = await _taskService.GetAllTasksAsync(paginationParams);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
            Assert.Equal(1, result.Page);
            Assert.Equal(10, result.PageSize);
            Assert.Equal(0, result.TotalPages);
            VerifyRepositoryGetAll(0, 10, Times.Once());
        }

        [Fact]
        public async Task GetAllTasksAsync_WithInvalidPageSize_UsesDefaultValues()
        {
            // Arrange
            var paginationParams = new PaginationParams { Page = 0, PageSize = 0 };
            var tasks = CreateTaskItems(1);
            SetupRepositoryGetAll(0, 1, tasks, 1);

            // Act
            var result = await _taskService.GetAllTasksAsync(paginationParams);

            // Assert
            Assert.Equal(1, result.Page);
            Assert.Equal(1, result.PageSize);
            Assert.Equal(1, result.Items.Count());
            VerifyRepositoryGetAll(0, 1, Times.Once());
        }

        [Fact]
        public async Task GetAllTasksAsync_WithPageSizeAboveMax_UsesMaxPageSize()
        {
            // Arrange
            var paginationParams = new PaginationParams { Page = 1, PageSize = 100 };
            var tasks = CreateTaskItems(50);
            SetupRepositoryGetAll(0, 50, tasks, 50);

            // Act
            var result = await _taskService.GetAllTasksAsync(paginationParams);

            // Assert
            Assert.Equal(50, result.PageSize);
            Assert.Equal(50, result.Items.Count());
            VerifyRepositoryGetAll(0, 50, Times.Once());
        }

        [Fact]
        public async Task GetAllTasksAsync_ThrowsOperationCanceledException_WhenCancelled()
        {
            // Arrange
            var paginationParams = new PaginationParams { Page = 1, PageSize = 10 };
            var cts = new CancellationTokenSource();
            cts.Cancel();
            _taskRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), cts.Token))
                .ThrowsAsync(new OperationCanceledException());

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                _taskService.GetAllTasksAsync(paginationParams, cts.Token));
        }

        #endregion

        #region GetTaskByIdAsync

        [Fact]
        public async Task GetTaskByIdAsync_ReturnsTaskDto_WhenTaskExists()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var task = CreateTaskItem("Test Task", "Test Desc", taskId);
            _taskRepositoryMock.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(task);

            // Act
            var result = await _taskService.GetTaskByIdAsync(taskId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(taskId, result.Id);
            Assert.Equal("Test Task", result.Title);
            Assert.Equal(TaskState.New, result.Status);
            _taskRepositoryMock.Verify(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>()), Times.Once());
            VerifyLoggerCalled(LogLevel.Information, Times.Once());
        }

        [Fact]
        public async Task GetTaskByIdAsync_ThrowsNotFoundException_WhenTaskDoesNotExist()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            _taskRepositoryMock.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((TaskItem)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _taskService.GetTaskByIdAsync(taskId));
            VerifyLoggerCalled(LogLevel.Warning, Times.Once());
            _taskRepositoryMock.Verify(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>()), Times.Once());
        }

        #endregion

        #region CreateTaskAsync

        [Fact]
        public async Task CreateTaskAsync_CreatesTaskAndTriggersNotification()
        {
            // Arrange
            var createTaskDto = new CreateTaskDto { Title = "New Task", Description = "New Desc" };
            _taskRepositoryMock.Setup(r => r.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _notificationServiceMock.Setup(n => n.NotifyTaskCreatedAsync(It.IsAny<TaskItem>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _taskService.CreateTaskAsync(createTaskDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Task", result.Title);
            Assert.Equal("New Desc", result.Description);
            Assert.Equal(TaskState.New, result.Status);
            _taskRepositoryMock.Verify(r => r.AddAsync(It.Is<TaskItem>(t => t.Title == "New Task"), It.IsAny<CancellationToken>()), Times.Once());
            _notificationServiceMock.Verify(n => n.NotifyTaskCreatedAsync(It.Is<TaskItem>(t => t.Title == "New Task")), Times.Once());
            VerifyLoggerCalled(LogLevel.Information, Times.AtLeast(1));
        }

        [Fact]
        public async Task CreateTaskAsync_LogsError_WhenNotificationFails()
        {
            // Arrange
            var createTaskDto = new CreateTaskDto { Title = "New Task", Description = "New Desc" };
            _taskRepositoryMock.Setup(r => r.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _notificationServiceMock.Setup(n => n.NotifyTaskCreatedAsync(It.IsAny<TaskItem>()))
                .ThrowsAsync(new Exception("Notification failed"));

            // Act
            var result = await _taskService.CreateTaskAsync(createTaskDto);

            // Assert
            Assert.NotNull(result);
            VerifyLoggerCalled(LogLevel.Error, Times.Once());
        }

        [Fact]
        public async Task CreateTaskAsync_ThrowsDbUpdateException_WhenRepositoryFails()
        {
            // Arrange
            var createTaskDto = new CreateTaskDto { Title = "New Task", Description = "New Desc" };
            _taskRepositoryMock.Setup(r => r.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new DbUpdateException("Database error", new Exception()));

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(() => _taskService.CreateTaskAsync(createTaskDto));
            _notificationServiceMock.Verify(n => n.NotifyTaskCreatedAsync(It.IsAny<TaskItem>()), Times.Never());
        }

        #endregion

        #region UpdateTaskAsync

        [Fact]
        public async Task UpdateTaskAsync_UpdatesTaskAndTriggersNotification()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var task = CreateTaskItem("Old Task", "Old Desc", taskId);
            var updateTaskDto = new UpdateTaskDto { Title = "Updated Task", Description = "Updated Desc", Status = TaskState.InProgress };
            _taskRepositoryMock.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(task);
            _taskRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _notificationServiceMock.Setup(n => n.NotifyTaskUpdatedAsync(It.IsAny<TaskItem>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _taskService.UpdateTaskAsync(taskId, updateTaskDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Task", result.Title);
            Assert.Equal(TaskState.InProgress, result.Status);
            _taskRepositoryMock.Verify(r => r.UpdateAsync(It.Is<TaskItem>(t => t.Title == "Updated Task"), It.IsAny<CancellationToken>()), Times.Once());
            _notificationServiceMock.Verify(n => n.NotifyTaskUpdatedAsync(It.Is<TaskItem>(t => t.Title == "Updated Task")), Times.Once());
            VerifyLoggerCalled(LogLevel.Information, Times.AtLeast(1));
        }

        [Fact]
        public async Task UpdateTaskAsync_ThrowsNotFoundException_WhenTaskDoesNotExist()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var updateTaskDto = new UpdateTaskDto { Title = "Updated Task", Description = "Updated Desc", Status = TaskState.InProgress };
            _taskRepositoryMock.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((TaskItem)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _taskService.UpdateTaskAsync(taskId, updateTaskDto));
            VerifyLoggerCalled(LogLevel.Warning, Times.Once());
            _taskRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task UpdateTaskAsync_ThrowsDbUpdateException_WhenRepositoryFails()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var task = CreateTaskItem("Old Task", "Old Desc", taskId);
            var updateTaskDto = new UpdateTaskDto { Title = "Updated Task", Description = "Updated Desc", Status = TaskState.InProgress };
            _taskRepositoryMock.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(task);
            _taskRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new DbUpdateException("Database error", new Exception()));

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(() => _taskService.UpdateTaskAsync(taskId, updateTaskDto));
            _notificationServiceMock.Verify(n => n.NotifyTaskUpdatedAsync(It.IsAny<TaskItem>()), Times.Never());
        }

        #endregion

        #region DeleteTaskAsync

        [Fact]
        public async Task DeleteTaskAsync_DeletesTaskAndTriggersNotification()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var task = CreateTaskItem("Task", "Desc", taskId);
            _taskRepositoryMock.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(task);
            _taskRepositoryMock.Setup(r => r.DeleteAsync(taskId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _notificationServiceMock.Setup(n => n.NotifyTaskDeletedAsync(taskId))
                .Returns(Task.CompletedTask);

            // Act
            await _taskService.DeleteTaskAsync(taskId);

            // Assert
            _taskRepositoryMock.Verify(r => r.DeleteAsync(taskId, It.IsAny<CancellationToken>()), Times.Once());
            _notificationServiceMock.Verify(n => n.NotifyTaskDeletedAsync(taskId), Times.Once());
            VerifyLoggerCalled(LogLevel.Information, Times.AtLeast(1));
        }

        [Fact]
        public async Task DeleteTaskAsync_ThrowsNotFoundException_WhenTaskDoesNotExist()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            _taskRepositoryMock.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((TaskItem)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _taskService.DeleteTaskAsync(taskId));
            VerifyLoggerCalled(LogLevel.Warning, Times.Once());
            _taskRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task DeleteTaskAsync_ThrowsDbUpdateException_WhenRepositoryFails()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var task = CreateTaskItem("Task", "Desc", taskId);
            _taskRepositoryMock.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(task);
            _taskRepositoryMock.Setup(r => r.DeleteAsync(taskId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new DbUpdateException("Database error", new Exception()));

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(() => _taskService.DeleteTaskAsync(taskId));
            _notificationServiceMock.Verify(n => n.NotifyTaskDeletedAsync(It.IsAny<Guid>()), Times.Never());
        }

        #endregion

        #region Helper Methods

        private List<TaskItem> CreateTaskItems(int count)
        {
            var tasks = new List<TaskItem>();
            for (int i = 0; i < count; i++)
            {
                tasks.Add(CreateTaskItem($"Task {i}", $"Desc {i}", Guid.NewGuid()));
            }
            return tasks;
        }

        private TaskItem CreateTaskItem(string title, string description, Guid id)
        {
            var task = new TaskItem(title, description);
            typeof(TaskItem).GetProperty("Id").SetValue(task, id);
            return task;
        }

        private void SetupRepositoryGetAll(int skip, int take, IEnumerable<TaskItem> tasks, int totalCount)
        {
            _taskRepositoryMock.Setup(r => r.GetAllAsync(skip, take, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tasks);
            _taskRepositoryMock.Setup(r => r.GetCountAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(totalCount);
        }

        private void VerifyRepositoryGetAll(int skip, int take, Times times)
        {
            _taskRepositoryMock.Verify(r => r.GetAllAsync(skip, take, It.IsAny<CancellationToken>()), times);
            _taskRepositoryMock.Verify(r => r.GetCountAsync(It.IsAny<CancellationToken>()), times);
        }

        private void VerifyLoggerCalled(LogLevel level, Times times)
        {
            _loggerMock.Verify(
                l => l.Log(
                    level,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                times);
        }

        #endregion
    }
}