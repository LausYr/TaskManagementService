using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace TaskManagementService.Application.Extensions
{
    /// <summary>
    /// Предоставляет методы расширения для общей конфигурации.
    /// </summary>
    public static class CommonExtensions
    {
        /// <summary>
        /// Настраивает логирование Serilog.
        /// </summary>
        public static IHostBuilder ConfigureLogging(this IHostBuilder builder)
        {
            return builder.UseSerilog((context, configuration) =>
            {
                configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .Enrich.FromLogContext()
                    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");
            });
        }

        /// <summary>
        /// Настраивает общие промежуточные программные компоненты.
        /// </summary>
        public static IApplicationBuilder UseCommonMiddleware(this IApplicationBuilder app, IHostEnvironment env, string serviceName)
        {
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var errorFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (errorFeature != null)
                    {
                        var loggerFactory = context.RequestServices.GetRequiredService<ILoggerFactory>();
                        var logger = loggerFactory.CreateLogger("CommonConfiguration");
                        logger.LogError(errorFeature.Error, "An unhandled exception occurred.");
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        context.Response.ContentType = "application/problem+json";
                        await context.Response.WriteAsJsonAsync(new ProblemDetails
                        {
                            Title = "Server Error",
                            Status = StatusCodes.Status500InternalServerError,
                            Detail = "An unexpected error occurred."
                        });
                    }
                });
            });

            if (env.IsDevelopment())
            {
                app.UseSwaggerDocumentation(serviceName);

                app.Use(async (context, next) =>
                {
                    if (context.Request.Path.Value == "/")
                    {
                        context.Response.Redirect("/swagger");
                        return;
                    }
                    await next();
                });

            }

            app.UseHttpsRedirection();
            app.UseCors("AllowSpecific");
            app.UseHealthChecks();
            return app;
        }
    }
}