using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaskManagementService.Application.Extensions;
using TaskManagementService.Infrastructure;
using TaskManagementService.Infrastructure.Messaging;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureLogging();

builder.ConfigureServices((context, services) =>
{
    services.AddOpenTelemetry(context.Configuration, context.Configuration["ServiceName"] ?? "TaskManagementService.EventConsumer");
    services.AddRabbitMQ(context.Configuration);
    services.AddHostedService<RabbitMqConsumer>();
});

var host = builder.Build();
await host.RunAsync();