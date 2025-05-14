using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskManagementService.Infrastructure.Data;

namespace TaskManagementService.Infrastructure
{
    /// <summary>
    /// Класс для инициализации базы данных
    /// </summary>
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider, ILogger logger)
        {
            try
            {
                var dbContext = serviceProvider.GetRequiredService<AppDbContext>();
                logger.LogInformation("Строка подключения: {ConnectionString}", dbContext.Database.GetConnectionString());
                logger.LogInformation("Начало применения миграций...");
                await dbContext.Database.MigrateAsync();
                logger.LogInformation("Миграции успешно применены");

                // Логируем список таблиц после миграции
                var tables = await dbContext.Database
                    .SqlQueryRaw<string>("SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'")
                    .ToListAsync();
                logger.LogInformation("Таблицы в базе данных: {Tables}", string.Join(", ", tables));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при инициализации базы данных");
                throw;
            }
        }
    }
}