using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using TaskManagementService.Application.DTOs;
using TaskManagementService.Application.Services;
using TaskManagementService.Common;
using TaskManagementService.Domain.Enums;
using TaskManagementService.Infrastructure.Messaging;
using TaskManagementService.Infrastructure.Messaging.Events;
using Xunit;

namespace TaskManagementService.Tests.Application.Services
{
    public class EventNotificationServiceTests
    {
        private readonly Mock<IMessagePublisher> _mockPublisher;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<EventNotificationService>> _mockLogger;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly EventNotificationService _service;

        public EventNotificationServiceTests()
        {
            _mockPublisher = new Mock<IMessagePublisher>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<EventNotificationService>>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            _mockConfiguration.Setup(c => c["ListenerUrl"]).Returns("http://localhost:5000");

            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);

            _service = new EventNotificationService(
                _mockPublisher.Object,
                _httpClient,
                _mockConfiguration.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task NotifyTaskCreatedAsync_ShouldPublishMessageAndCallApi()
        {
            // Arrange
            var taskDto = new TaskDto
            {
                Id = Guid.NewGuid(),
                Title = "Test Task",
                Description = "Test Description",
                Status = TaskState.New
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            // Act
            await _service.NotifyTaskCreatedAsync(taskDto);

            // Assert
            _mockPublisher.Verify(p => p.PublishAsync(
                It.Is<TaskCreatedEvent>(e =>
                    e.TaskId == taskDto.Id &&
                    e.Title == taskDto.Title &&
                    e.Description == taskDto.Description &&
                    e.Status == taskDto.Status
                ),
                MessageConstants.TaskCreatedRoutingKey
            ), Times.Once);

            _mockHttpMessageHandler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri.ToString().EndsWith("/api/v1/events/created")
                    ),
                    ItExpr.IsAny<CancellationToken>()
                );
        }

        [Fact]
        public async Task NotifyTaskUpdatedAsync_ShouldPublishMessageAndCallApi()
        {
            // Arrange
            var taskDto = new TaskDto
            {
                Id = Guid.NewGuid(),
                Title = "Updated Task",
                Description = "Updated Description",
                Status = TaskState.InProgress
            };

            // Настройка HTTP ответа
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            // Act
            await _service.NotifyTaskUpdatedAsync(taskDto);

            // Assert
            _mockPublisher.Verify(p => p.PublishAsync(
                It.Is<TaskUpdatedEvent>(e =>
                    e.TaskId == taskDto.Id &&
                    e.Title == taskDto.Title &&
                    e.Description == taskDto.Description &&
                    e.Status == taskDto.Status
                ),
                MessageConstants.TaskUpdatedRoutingKey
            ), Times.Once);

            _mockHttpMessageHandler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri.ToString().EndsWith("/api/v1/events/updated")
                    ),
                    ItExpr.IsAny<CancellationToken>()
                );
        }

        [Fact]
        public async Task NotifyTaskDeletedAsync_ShouldPublishMessageAndCallApi()
        {
            // Arrange
            var taskId = Guid.NewGuid();

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            // Act
            await _service.NotifyTaskDeletedAsync(taskId);

            // Assert
            _mockPublisher.Verify(p => p.PublishAsync(
                It.Is<TaskDeletedEvent>(e => e.TaskId == taskId),
                MessageConstants.TaskDeletedRoutingKey
            ), Times.Once);

            _mockHttpMessageHandler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri.ToString().EndsWith("/api/v1/events/deleted")
                    ),
                    ItExpr.IsAny<CancellationToken>()
                );
        }

        [Fact]
        public async Task NotifyTaskCreatedAsync_WhenPublisherThrowsException_ShouldLogError()
        {
            // Arrange
            var taskDto = new TaskDto
            {
                Id = Guid.NewGuid(),
                Title = "Test Task",
                Description = "Test Description",
                Status = TaskState.New
            };

            _mockPublisher
                .Setup(p => p.PublishAsync(It.IsAny<TaskCreatedEvent>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            await _service.NotifyTaskCreatedAsync(taskDto);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task NotifyTaskUpdatedAsync_WhenHttpClientThrowsException_ShouldLogError()
        {
            // Arrange
            var taskDto = new TaskDto
            {
                Id = Guid.NewGuid(),
                Title = "Updated Task",
                Description = "Updated Description",
                Status = TaskState.InProgress
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException("Test HTTP exception"));

            // Act
            await _service.NotifyTaskUpdatedAsync(taskDto);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task NotifyTaskDeletedAsync_WhenApiReturnsError_ShouldLogError()
        {
            // Arrange
            var taskId = Guid.NewGuid();

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            // Act
            await _service.NotifyTaskDeletedAsync(taskId);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void Constructor_WhenListenerUrlIsNull_ShouldThrowException()
        {
            // Arrange
            var mockEmptyConfig = new Mock<IConfiguration>();
            mockEmptyConfig.Setup(c => c["ListenerUrl"]).Returns((string)null);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => new EventNotificationService(
                _mockPublisher.Object,
                _httpClient,
                mockEmptyConfig.Object,
                _mockLogger.Object
            ));
        }
    }
}