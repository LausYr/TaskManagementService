using FluentValidation;
using TaskManagementService.Application.DTOs;

namespace TaskManagementService.Application.Validators
{
    public class CreateTaskDtoValidator : AbstractValidator<CreateTaskDto>
    {
        public CreateTaskDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Название задачи обязательно")
                .MaximumLength(200).WithMessage("Название задачи не должно превышать 200 символов");

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Описание задачи не должно превышать 2000 символов")
                .When(x => x.Description != null);
        }
    }
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
    public class PaginationParamsValidator : AbstractValidator<PaginationParams>
    {
        public PaginationParamsValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThanOrEqualTo(1).WithMessage("Номер страницы должен быть больше или равен 1");

            RuleFor(x => x.PageSize)
                .GreaterThanOrEqualTo(1).WithMessage("Размер страницы должен быть больше или равен 1")
                .LessThanOrEqualTo(50).WithMessage("Размер страницы не должен превышать 50");
        }
    }
}