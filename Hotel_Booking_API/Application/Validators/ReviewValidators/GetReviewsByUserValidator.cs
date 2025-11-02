using FluentValidation;
using Hotel_Booking_API.Application.Features.Reviews.Queries.GetReviewsByUser;

namespace Hotel_Booking_API.Application.Validators.ReviewValidators
{
    /// <summary>
    /// Validator for GetReviewsByUserQuery to ensure all parameters are valid.
    /// Validates user ID and pagination.
    /// </summary>
    public class GetReviewsByUserValidator : AbstractValidator<GetReviewsByUserQuery>
    {
        public GetReviewsByUserValidator()
        {
            // Validate user ID
            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("User ID must be greater than 0");

            // Validate pagination parameters
            RuleFor(x => x.Pagination.PageNumber)
                .GreaterThan(0).WithMessage("Page number must be greater than 0")
                .LessThanOrEqualTo(1000).WithMessage("Page number cannot exceed 1000");

            RuleFor(x => x.Pagination.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100");
        }
    }
}
