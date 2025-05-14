using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskManagementService.Application;
using TaskManagementService.Application.Extensions;
using TaskManagementService.Application.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureLogging();

builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>())
    .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase);

builder.Services.AddApiVersioningConfiguration();
builder.Services.AddSwaggerDocumentation(
    builder.Configuration["ServiceName"] ?? "TaskManagementService.Listener",
    builder.Configuration["ContactEmail"] ?? "listener@taskmanagement.com",
    builder.Services.BuildServiceProvider().GetRequiredService<ILoggerFactory>(),
    builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecific", policy =>
        policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? ["http://localhost:5000"])
              .AllowAnyMethod()
              .AllowAnyHeader());
});

builder.Services.AddBaseHealthChecks(); // Используем базовые health checks
builder.Services.AddOpenTelemetry(builder.Configuration, builder.Configuration["ServiceName"] ?? "TaskManagementService.Listener");
builder.Services.AddListenerCore();

var app = builder.Build();

app.UseCommonMiddleware(app.Environment, builder.Configuration["ServiceName"] ?? "TaskManagementService.Listener");
app.MapControllers();

await app.RunAsync();

public partial class Program { }