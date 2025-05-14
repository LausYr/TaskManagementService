using FluentValidation.TestHelper;
using System;
using TaskManagementService.Infrastructure.Messaging.Events;
using TaskManagementService.Application.Validators;

namespace TaskManagementService.Tests.Validators
{
    public class TaskDeletedEventValidatorTests
    {
        private readonly TaskDeletedEventValidator _validator;

        public TaskDeletedEventValidatorTests()
        {
            _validator = new TaskDeletedEventValidator();
        }

        [Fact]
        public void Should_Have_Error_When_EventId_Is_Empty()
        {
            var validEvent = new TaskDeletedEvent(Guid.NewGuid());
            var model = validEvent with { EventId = Guid.Empty };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.EventId)
                  .WithErrorMessage("Идентификатор события обязателен.");
        }

        [Fact]
        public void Should_Have_Error_When_TaskId_Is_Empty()
        {
            var model = new TaskDeletedEvent(Guid.Empty);
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.TaskId)
                  .WithErrorMessage("Идентификатор задачи обязателен.");
        }

        [Fact]
        public void Should_Have_Error_When_Timestamp_Is_Empty()
        {
            var validEvent = new TaskDeletedEvent(Guid.NewGuid());
            var model = validEvent with { Timestamp = default };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Timestamp)
                  .WithErrorMessage("Время события обязательно.");
        }

        [Fact]
        public void Should_Not_Have_Errors_When_Model_Is_Valid()
        {
            var model = new TaskDeletedEvent(Guid.NewGuid());
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}