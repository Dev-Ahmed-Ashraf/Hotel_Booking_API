using FluentValidation;
using Hotel_Booking_API.Application.Features.Reviews.Commands.DeleteReview;

namespace Hotel_Booking_API.Application.Validators.ReviewValidators
{
    /// <summary>
    /// Validator for DeleteReviewCommand to ensure all deletion parameters are valid.
    /// Validates review ID.
    /// </summary>
    public class DeleteReviewValidator : AbstractValidator<DeleteReviewCommand>
    {
        public DeleteReviewValidator()
        {
            // Validate review ID
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Review ID must be greater than 0");
        }
    }
}
