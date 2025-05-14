using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using TaskManagementService.Application;
using TaskManagementService.Application.Extensions;
using TaskManagementService.Application.Filters;
using TaskManagementService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ��������� �����������
builder.Host.ConfigureLogging();

// ��������� ������������ � JSON ������������
builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>())
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase);

// API ��������������� � Swagger
builder.Services.AddApiVersioningConfiguration();
builder.Services.AddSwaggerDocumentation(
    builder.Configuration["ServiceName"] ?? "TaskManagementService.API",
    builder.Configuration["ContactEmail"] ?? "api@taskmanagement.com",
    builder.Services.BuildServiceProvider().GetRequiredService<ILoggerFactory>(),
    builder.Configuration);

// CORS ��������
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecific", policy =>
        policy.WithOrigins(
                builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ??
                ["http://localhost:5000"])
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// ����������� ��������
builder.Services.AddHealthChecksWithDbAndRabbitMQ(builder.Configuration);
builder.Services.AddOpenTelemetry(
    builder.Configuration,
    builder.Configuration["ServiceName"] ?? "TaskManagementService.API");
builder.Services.AddApplicationCore();
builder.Services.AddTaskManagement();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddRabbitMQ(builder.Configuration);

// ��������� ������ ������� ������� (10 MB)
builder.Services.Configure<KestrelServerOptions>(options =>
    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024);

var app = builder.Build();

// ��������� middleware � �������������
app.UseCommonMiddleware(
    app.Environment,
    builder.Configuration["ServiceName"] ?? "TaskManagementService.API");
app.MapControllers();

// ������������� �� � ������ ����������
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        await DbInitializer.InitializeAsync(scope.ServiceProvider, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "������ ��� ������������� ���� ������");
    }
}

await app.RunAsync();

public partial class Program { }