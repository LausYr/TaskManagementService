using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
using TaskManagementService.Infrastructure.Data;

namespace TaskManagementService.Infrastructure
{
    /// <summary>
    /// Фабрика для создания контекста базы данных во время выполнения миграций
    /// </summary>
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        /// <summary>
        /// Создает новый экземпляр контекста базы данных
        /// </summary>
        /// <param name="args">Аргументы командной строки</param>
        /// <returns>Контекст базы данных</returns>
        public AppDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Не найдена строка подключения 'DefaultConnection'");
            }

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(connectionString, options =>
            {
                options.MigrationsAssembly("TaskManagementService.Infrastructure");
            });

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}