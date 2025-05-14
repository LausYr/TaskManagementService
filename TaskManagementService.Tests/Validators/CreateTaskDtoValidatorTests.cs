using FluentValidation.TestHelper;
using TaskManagementService.Application.DTOs;
using TaskManagementService.Application.Validators;
using Xunit;
using System.Linq;

namespace TaskManagementService.Tests.Validators
{
    public class CreateTaskDtoValidatorTests
    {
        private readonly CreateTaskDtoValidator _validator;

        public CreateTaskDtoValidatorTests()
        {
            _validator = new CreateTaskDtoValidator();
        }

        [Fact]
        public void Should_Have_Error_When_Title_Is_Null()
        {
            var model = new CreateTaskDto { Title = null, Description = "Valid Description" };
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
            var model = new CreateTaskDto { Title = string.Empty, Description = "Valid Description" };
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
            var model = new CreateTaskDto { Title = new string('A', 201), Description = "Valid Description" };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Title)
                  .WithErrorMessage("Название задачи не должно превышать 200 символов");
        }

        [Fact]
        public void Should_Have_Error_When_Description_Exceeds_MaxLength()
        {
            var model = new CreateTaskDto { Title = "Valid Title", Description = new string('A', 2001) };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Description)
                  .WithErrorMessage("Описание задачи не должно превышать 2000 символов");
        }

        [Fact]
        public void Should_Not_Have_Error_When_Description_Is_Null()
        {
            var model = new CreateTaskDto { Title = "Valid Title", Description = null };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void Should_Not_Have_Errors_When_Model_Is_Valid()
        {
            var model = new CreateTaskDto
            {
                Title = "Valid Title",
                Description = "Valid Description"
            };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Not_Have_Error_When_Title_Is_Exactly_MaxLength()
        {
            var model = new CreateTaskDto
            {
                Title = new string('A', 200),
                Description = "Valid Description"
            };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Description_Is_Exactly_MaxLength()
        {
            var model = new CreateTaskDto
            {
                Title = "Valid Title",
                Description = new string('A', 2000)
            };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }
    }
}