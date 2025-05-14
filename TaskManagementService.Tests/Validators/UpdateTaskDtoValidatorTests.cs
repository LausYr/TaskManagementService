using FluentValidation.TestHelper;
using TaskManagementService.Application.DTOs;
using TaskManagementService.Application.Validators;
using TaskManagementService.Domain.Enums;
using Xunit;
using System.Linq;

namespace TaskManagementService.Tests.Validators
{
    public class UpdateTaskDtoValidatorTests
    {
        private readonly UpdateTaskDtoValidator _validator;

        public UpdateTaskDtoValidatorTests()
        {
            _validator = new UpdateTaskDtoValidator();
        }

        [Fact]
        public void Should_Have_Error_When_Title_Is_Null()
        {
            var model = new UpdateTaskDto { Title = null, Description = "Valid Description", Status = TaskState.New };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Title)
                  .WithErrorMessage("Название задачи обязательно");
            // Для диагностики
            if (!result.IsValid)
            {
                var errors = string.Join("; ", result.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
                System.Diagnostics.Debug.WriteLine($"Validation errors: {errors}");
            }
        }

        [Fact]
        public void Should_Have_Error_When_Title_Is_Empty()
        {
            var model = new UpdateTaskDto { Title = string.Empty, Description = "Valid Description", Status = TaskState.New };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Title)
                  .WithErrorMessage("Название задачи обязательно");
            // Для диагностики
            if (!result.IsValid)
            {
                var errors = string.Join("; ", result.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
                System.Diagnostics.Debug.WriteLine($"Validation errors: {errors}");
            }
        }

        [Fact]
        public void Should_Have_Error_When_Title_Exceeds_MaxLength()
        {
            var model = new UpdateTaskDto { Title = new string('A', 201), Description = "Valid Description", Status = TaskState.New };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Title)
                  .WithErrorMessage("Название задачи не должно превышать 200 символов");
        }

        [Fact]
        public void Should_Have_Error_When_Description_Exceeds_MaxLength()
        {
            var model = new UpdateTaskDto { Title = "Valid Title", Description = new string('A', 2001), Status = TaskState.New };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Description)
                  .WithErrorMessage("Описание задачи не должно превышать 2000 символов");
        }

        [Fact]
        public void Should_Have_Error_When_Status_Is_Invalid()
        {
            var model = new UpdateTaskDto { Title = "Valid Title", Description = "Valid Description", Status = (TaskState)999 };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Status)
                  .WithErrorMessage("Недопустимый статус задачи");
        }

        [Fact]
        public void Should_Not_Have_Error_When_Description_Is_Null()
        {
            var model = new UpdateTaskDto { Title = "Valid Title", Description = null, Status = TaskState.New };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void Should_Not_Have_Errors_When_Model_Is_Valid()
        {
            var model = new UpdateTaskDto
            {
                Title = "Valid Title",
                Description = "Valid Description",
                Status = TaskState.New
            };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Not_Have_Errors_When_Status_Is_InProgress()
        {
            var model = new UpdateTaskDto
            {
                Title = "Valid Title",
                Description = "Valid Description",
                Status = TaskState.InProgress
            };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Not_Have_Errors_When_Status_Is_Completed()
        {
            var model = new UpdateTaskDto
            {
                Title = "Valid Title",
                Description = "Valid Description",
                Status = TaskState.Completed
            };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Not_Have_Error_When_Title_Is_Exactly_MaxLength()
        {
            var model = new UpdateTaskDto
            {
                Title = new string('A', 200),
                Description = "Valid Description",
                Status = TaskState.New
            };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Description_Is_Exactly_MaxLength()
        {
            var model = new UpdateTaskDto
            {
                Title = "Valid Title",
                Description = new string('A', 2000),
                Status = TaskState.New
            };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }
    }
}