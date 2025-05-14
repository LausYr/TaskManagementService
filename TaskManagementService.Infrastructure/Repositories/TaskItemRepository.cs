using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManagementService.Domain.Entities;
using TaskManagementService.Domain.Repositories;
using TaskManagementService.Infrastructure.Data;

namespace TaskManagementService.Infrastructure.Repositories
{
    /// <summary>
    /// Репозиторий для работы с задачами
    /// </summary>
    public class TaskItemRepository : ITaskItemRepository
    {
        private readonly AppDbContext _dbContext;

        /// <summary>
        /// Создает новый экземпляр репозитория
        /// </summary>
        /// <param name="dbContext">Контекст базы данных</param>
        public TaskItemRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TaskItem>> GetAllAsync(int skip, int take, CancellationToken cancellationToken = default)
        {
            return await _dbContext.TaskItems
                .OrderByDescending(t => t.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.TaskItems.CountAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<TaskItem> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.TaskItems
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task AddAsync(TaskItem taskItem, CancellationToken cancellationToken = default)
        {
            await _dbContext.TaskItems.AddAsync(taskItem, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(TaskItem taskItem, CancellationToken cancellationToken = default)
        {
            _dbContext.TaskItems.Update(taskItem);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var taskItem = await _dbContext.TaskItems.FindAsync(new object[] { id }, cancellationToken);
            if (taskItem == null)
                return false;

            _dbContext.TaskItems.Remove(taskItem);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}