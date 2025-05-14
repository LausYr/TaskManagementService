using FluentValidation.TestHelper;
using TaskManagementService.Infrastructure.Messaging.Events;
using TaskManagementService.Domain.Enums;
using System.Linq;
using System;
using TaskManagementService.Application.Validators;

namespace TaskManagementService.Listener.Tests.Validators
{
    public class TaskCreatedEventValidatorTests
    {
        private readonly TaskCreatedEventValidator _validator;

        public TaskCreatedEventValidatorTests()
        {
            _validator = new TaskCreatedEventValidator();
        }

        [Fact]
        public void Should_Have_Error_When_EventId_Is_Empty()
        {
            var validEvent = new TaskCreatedEvent(Guid.NewGuid(), "Valid Title", "Valid Description", TaskState.New);
            var model = validEvent with { EventId = Guid.Empty };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.EventId)
                  .WithErrorMessage("Идентификатор события обязательно.");
        }

        [Fact]
        public void Should_Have_Error_When_TaskId_Is_Empty()
        {
            var model = new TaskCreatedEvent(Guid.Empty, "Valid Title", "Valid Description", TaskState.New);
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.TaskId)
                  .WithErrorMessage("Идентификатор задачи обязательно.");
        }

        [Fact]
        public void Should_Have_Error_When_Title_Is_Null()
        {
            var validEvent = new TaskCreatedEvent(Guid.NewGuid(), "Valid Title", "Valid Description", TaskState.New);
            var model = validEvent with { Title = null! };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Title)
                  .WithErrorMessage("Название задачи обязательно.");
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
            var model = new TaskCreatedEvent(Guid.NewGuid(), string.Empty, "Valid Description", TaskState.New);
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Title)
                  .WithErrorMessage("Название задачи обязательно.");

            if (!result.IsValid)
            {
                var errors = string.Join("; ", result.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
                System.Diagnostics.Debug.WriteLine($"Validation errors: {errors}");
            }
        }

        [Fact]
        public void Should_Have_Error_When_Title_Exceeds_MaxLength()
        {
            var model = new TaskCreatedEvent(Guid.NewGuid(), new string('A', 201), "Valid Description", TaskState.New);
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Title)
                  .WithErrorMessage("Название задачи не должно превышать 200 символов.");
        }

        [Fact]
        public void Should_Have_Error_When_Description_Exceeds_MaxLength()
        {
            var model = new TaskCreatedEvent(Guid.NewGuid(), "Valid Title", new string('A', 2001), TaskState.New);
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Description)
                  .WithErrorMessage("Описание задачи не должно превышать 2000 символов.");
        }

        [Fact]
        public void Should_Have_Error_When_Timestamp_Is_Empty()
        {
            var validEvent = new TaskCreatedEvent(Guid.NewGuid(), "Valid Title", "Valid Description", TaskState.New);
            var model = validEvent with { Timestamp = default };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Timestamp)
                  .WithErrorMessage("Время события обязательно.");
        }

        [Fact]
        public void Should_Have_Error_When_Status_Is_Invalid()
        {
            var validEvent = new TaskCreatedEvent(Guid.NewGuid(), "Valid Title", "Valid Description", TaskState.New);
            var model = validEvent with { Status = (TaskState)999 };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Status)
                  .WithErrorMessage("Недопустимый статус задачи.");
        }

        [Fact]
        public void Should_Not_Have_Errors_When_Model_Is_Valid()
        {
            var model = new TaskCreatedEvent(Guid.NewGuid(), "Valid Title", "Valid Description", TaskState.New);
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Not_Have_Errors_When_Status_Is_InProgress()
        {
            var model = new TaskCreatedEvent(Guid.NewGuid(), "Valid Title", "Valid Description", TaskState.InProgress);
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Not_Have_Errors_When_Status_Is_Completed()
        {
            var model = new TaskCreatedEvent(Guid.NewGuid(), "Valid Title", "Valid Description", TaskState.Completed);
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Not_Have_Error_When_Title_Is_Exactly_MaxLength()
        {
            var model = new TaskCreatedEvent(Guid.NewGuid(), new string('A', 200), "Valid Description", TaskState.New);
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Description_Is_Exactly_MaxLength()
        {
            var model = new TaskCreatedEvent(Guid.NewGuid(), "Valid Title", new string('A', 2000), TaskState.New);
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Description_Is_Null()
        {
            var validEvent = new TaskCreatedEvent(Guid.NewGuid(), "Valid Title", "Valid Description", TaskState.New);
            var model = validEvent with { Description = null! };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }
    }
}