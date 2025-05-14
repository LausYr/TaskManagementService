using FluentValidation;
using TaskManagementService.Infrastructure.Messaging.Events;

namespace TaskManagementService.Listener.Validators;

public class TaskDeletedEventValidator : AbstractValidator<TaskDeletedEvent>
{
    public TaskDeletedEventValidator()
    {
        RuleFor(x => x.EventId).NotEmpty().WithMessage("Идентификатор события обязателен.");
        RuleFor(x => x.TaskId).NotEmpty().WithMessage("Идентификатор задачи обязателен.");
        RuleFor(x => x.Timestamp).NotEmpty().WithMessage("Время события обязательно.");
    }
}