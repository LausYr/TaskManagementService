using FluentValidation;
using TaskManagementService.Application.DTOs;

namespace TaskManagementService.Application.Validators
{
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