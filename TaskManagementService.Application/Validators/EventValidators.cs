using FluentValidation;
using TaskManagementService.Infrastructure.Messaging.Events;

namespace TaskManagementService.Application.Validators
{

    public class TaskCreatedEventValidator : AbstractValidator<TaskCreatedEvent>
    {
        public TaskCreatedEventValidator()
        {
            RuleFor(x => x.EventId)
                .NotEmpty()
                .WithMessage("Идентификатор события обязательно.");

            RuleFor(x => x.TaskId)
                .NotEmpty()
                .WithMessage("Идентификатор задачи обязательно.");

            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Название задачи обязательно.")
                .MaximumLength(200)
                .WithMessage("Название задачи не должно превышать 200 символов.");

            RuleFor(x => x.Description)
                .MaximumLength(2000)
                .WithMessage("Описание задачи не должно превышать 2000 символов.")
                .When(x => x.Description != null);

            RuleFor(x => x.Timestamp)
                .NotEmpty()
                .WithMessage("Время события обязательно.");

            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Недопустимый статус задачи.");
        }
    }
    public class TaskDeletedEventValidator : AbstractValidator<TaskDeletedEvent>
    {
        public TaskDeletedEventValidator()
        {
            RuleFor(x => x.EventId).NotEmpty().WithMessage("Идентификатор события обязателен.");
            RuleFor(x => x.TaskId).NotEmpty().WithMessage("Идентификатор задачи обязателен.");
            RuleFor(x => x.Timestamp).NotEmpty().WithMessage("Время события обязательно.");
        }

    }

    public class TaskUpdatedEventValidator : AbstractValidator<TaskUpdatedEvent>
    {
        public TaskUpdatedEventValidator()
        {
            RuleFor(x => x.EventId)
                .NotEmpty()
                .WithMessage("Идентификатор события обязательно.");

            RuleFor(x => x.TaskId)
                .NotEmpty()
                .WithMessage("Идентификатор задачи обязательно.");

            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Название задачи обязательно.")
                .MaximumLength(200)
                .WithMessage("Название задачи не должно превышать 200 символов.");

            RuleFor(x => x.Description)
                .MaximumLength(2000)
                .WithMessage("Описание задачи не должно превышать 2000 символов.")
                .When(x => x.Description != null);

            RuleFor(x => x.Timestamp)
                .NotEmpty()
                .WithMessage("Время события обязательно.");

            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Недопустимый статус задачи.");
        }
    }
}