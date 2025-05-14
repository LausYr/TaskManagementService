using Microsoft.AspNetCore.Mvc;
using TaskManagementService.Infrastructure.Messaging.Events;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TaskManagementService.Listener.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/events")]
    public class EventsController : ControllerBase
    {
        private readonly ILogger<EventsController> _logger;

        public EventsController(ILogger<EventsController> logger)
        {
            _logger = logger;
        }

        [HttpPost("created")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Created([FromBody] TaskCreatedEvent taskEvent)
        {
            _logger.LogInformation("Получено событие создания задачи: EventId={EventId}, TaskId={TaskId}, Title={Title}, Status={Status}",
                taskEvent.EventId, taskEvent.TaskId, taskEvent.Title, taskEvent.Status);
            return Ok(new { Message = "Событие создания задачи обработано" });
        }

        [HttpPost("updated")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Updated([FromBody] TaskUpdatedEvent taskEvent)
        {
            _logger.LogInformation("Получено событие обновления задачи: EventId={EventId}, TaskId={TaskId}, Title={Title}, Status={Status}",
                taskEvent.EventId, taskEvent.TaskId, taskEvent.Title, taskEvent.Status);
            return Ok(new { Message = "Событие обновления задачи обработано" });
        }

        [HttpPost("deleted")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Deleted([FromBody] TaskDeletedEvent taskEvent)
        {
            _logger.LogInformation("Получено событие удаления задачи: EventId={EventId}, TaskId={TaskId}",
                taskEvent.EventId, taskEvent.TaskId);
            return Ok(new { Message = "Событие удаления задачи обработано" });
        }
    }
}