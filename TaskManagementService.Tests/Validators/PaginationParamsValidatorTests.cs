using FluentValidation.TestHelper;
using TaskManagementService.Application.DTOs;
using TaskManagementService.Application.Validators;
using Xunit;
using System.Linq;

namespace TaskManagementService.Tests.Validators
{
    public class PaginationParamsValidatorTests
    {
        private readonly PaginationParamsValidator _validator;

        public PaginationParamsValidatorTests()
        {
            _validator = new PaginationParamsValidator();
        }

        [Fact]
        public void Should_Not_Have_Errors_When_Model_Is_Valid()
        {
            var model = new PaginationParams { Page = 1, PageSize = 10 };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Not_Have_Error_When_PageSize_Is_Exactly_Maximum()
        {
            var model = new PaginationParams { Page = 1, PageSize = 50 };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.PageSize);
        }

        [Fact]
        public void Should_Not_Have_Error_When_PageSize_Is_Minimum()
        {
            var model = new PaginationParams { Page = 1, PageSize = 1 };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.PageSize);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Page_Is_Large()
        {
            var model = new PaginationParams { Page = 100, PageSize = 10 };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Page);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Page_Is_Minimum()
        {
            var model = new PaginationParams { Page = 1, PageSize = 10 };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Page);
        }
    }
}