using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManagementService.Domain.Entities;
using TaskManagementService.Domain.Enums;
using TaskManagementService.Infrastructure.Data;
using TaskManagementService.Infrastructure.Repositories;
using Xunit;

namespace TaskManagementService.Tests.Infrastructure.Repositories
{
    public class TaskItemRepositoryTests : IDisposable
    {
        private readonly AppDbContext _dbContext; private readonly TaskItemRepository _repository;
        public TaskItemRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new AppDbContext(options);
            _repository = new TaskItemRepository(_dbContext);
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [Fact]
        public async Task GetAllAsync_ReturnsPaginatedTasks_OrderedByCreatedAtDescending()
        {
            // Arrange
            var tasks = new List<TaskItem>
        {
            new TaskItem("Task 1", "Desc 1"),
            new TaskItem("Task 2", "Desc 2"),
            new TaskItem("Task 3", "Desc 3")
        };

            // Добавляем задачи с небольшой задержкой, чтобы CreatedAt различались
            foreach (var task in tasks)
            {
                await _dbContext.TaskItems.AddAsync(task);
                await _dbContext.SaveChangesAsync();
                await Task.Delay(1); // Минимальная задержка для разных CreatedAt
            }

            // Act
            var result = await _repository.GetAllAsync(skip: 1, take: 2);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Equal("Task 2", result.First().Title); // Вторая по времени создания
            Assert.Equal("Task 1", result.Last().Title);  // Первая по времени создания
        }

        [Fact]
        public async Task GetAllAsync_WithEmptyDatabase_ReturnsEmptyList()
        {
            // Act
            var result = await _repository.GetAllAsync(skip: 0, take: 10);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCountAsync_ReturnsCorrectCount()
        {
            // Arrange
            await _dbContext.TaskItems.AddRangeAsync(
                new TaskItem("Task 1", "Desc 1"),
                new TaskItem("Task 2", "Desc 2")
            );
            await _dbContext.SaveChangesAsync();

            // Act
            var count = await _repository.GetCountAsync();

            // Assert
            Assert.Equal(2, count);
        }

        [Fact]
        public async Task GetCountAsync_WithEmptyDatabase_ReturnsZero()
        {
            // Act
            var count = await _repository.GetCountAsync();

            // Assert
            Assert.Equal(0, count);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsTask_WhenTaskExists()
        {
            // Arrange
            var task = new TaskItem("Task 1", "Desc 1");
            await _dbContext.TaskItems.AddAsync(task);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(task.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(task.Id, result.Id);
            Assert.Equal("Task 1", result.Title);
            Assert.Equal("Desc 1", result.Description);
            Assert.Equal(TaskState.New, result.Status);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenTaskDoesNotExist()
        {
            // Act
            var result = await _repository.GetByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddAsync_AddsTaskToDatabase()
        {
            // Arrange
            var task = new TaskItem("Task 1", "Desc 1");
            var before = DateTime.UtcNow.AddSeconds(-1);

            // Act
            await _repository.AddAsync(task);
            var savedTask = await _dbContext.TaskItems.FindAsync(task.Id);

            // Assert
            Assert.NotNull(savedTask);
            Assert.Equal("Task 1", savedTask.Title);
            Assert.Equal("Desc 1", savedTask.Description);
            Assert.Equal(TaskState.New, savedTask.Status);
            Assert.True(savedTask.CreatedAt >= before);
            Assert.True(savedTask.CreatedAt <= DateTime.UtcNow);
            Assert.Equal(savedTask.CreatedAt, savedTask.UpdatedAt);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesTaskUsingUpdateMethod()
        {
            // Arrange
            var task = new TaskItem("Task 1", "Desc 1");
            await _dbContext.TaskItems.AddAsync(task);
            await _dbContext.SaveChangesAsync();

            // Act
            task.Update("Updated Title", "Updated Desc", TaskState.InProgress);
            await _repository.UpdateAsync(task);
            var updatedTask = await _dbContext.TaskItems.FindAsync(task.Id);

            // Assert
            Assert.NotNull(updatedTask);
            Assert.Equal("Updated Title", updatedTask.Title);
            Assert.Equal("Updated Desc", updatedTask.Description);
            Assert.Equal(TaskState.InProgress, updatedTask.Status);
            Assert.True(updatedTask.UpdatedAt > updatedTask.CreatedAt);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesTaskUsingChangeStatusMethod()
        {
            // Arrange
            var task = new TaskItem("Task 1", "Desc 1");
            await _dbContext.TaskItems.AddAsync(task);
            await _dbContext.SaveChangesAsync();

            // Act
            task.ChangeStatus(TaskState.Completed);
            await _repository.UpdateAsync(task);
            var updatedTask = await _dbContext.TaskItems.FindAsync(task.Id);

            // Assert
            Assert.NotNull(updatedTask);
            Assert.Equal("Task 1", updatedTask.Title); // Title не изменился
            Assert.Equal("Desc 1", updatedTask.Description); // Description не изменился
            Assert.Equal(TaskState.Completed, updatedTask.Status);
            Assert.True(updatedTask.UpdatedAt > updatedTask.CreatedAt);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsTrue_AndRemovesTask_WhenTaskExists()
        {
            // Arrange
            var task = new TaskItem("Task 1", "Desc 1");
            await _dbContext.TaskItems.AddAsync(task);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.DeleteAsync(task.Id);
            var deletedTask = await _dbContext.TaskItems.FindAsync(task.Id);

            // Assert
            Assert.True(result);
            Assert.Null(deletedTask);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenTaskDoesNotExist()
        {
            // Act
            var result = await _repository.DeleteAsync(Guid.NewGuid());

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task Constructor_ThrowsArgumentNullException_WhenDbContextIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new TaskItemRepository(null));
        }
    }
}