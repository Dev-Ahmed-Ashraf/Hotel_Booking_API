using FluentValidation;
using Hotel_Booking_API.Application.Features.Reviews.Queries.GetReviewById;

namespace Hotel_Booking_API.Application.Validators.ReviewValidators
{
    /// <summary>
    /// Validator for GetReviewByIdQuery to ensure the review ID parameter is valid.
    /// </summary>
    public class GetReviewByIdValidator : AbstractValidator<GetReviewByIdQuery>
    {
        public GetReviewByIdValidator()
        {
            // Validate review ID
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Review ID must be greater than 0");
        }
    }
}
