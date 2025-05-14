using FluentValidation;
using TaskManagementService.Application.DTOs;

namespace TaskManagementService.Application.Validators
{
    public class UpdateTaskDtoValidator : AbstractValidator<UpdateTaskDto>
    {
        public UpdateTaskDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Название задачи обязательно")
                .MaximumLength(200).WithMessage("Название задачи не должно превышать 200 символов");

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Описание задачи не должно превышать 2000 символов")
                .When(x => x.Description != null);

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Недопустимый статус задачи");
        }
    }
}