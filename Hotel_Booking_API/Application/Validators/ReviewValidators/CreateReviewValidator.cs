using FluentValidation;
using Hotel_Booking_API.Application.Features.Reviews.Commands.CreateReview;

namespace Hotel_Booking_API.Application.Validators.ReviewValidators
{
    /// <summary>
    /// Validator for CreateReviewCommand to ensure all review parameters are valid.
    /// Validates user ID, hotel ID, rating, and comment.
    /// </summary>
    public class CreateReviewValidator : AbstractValidator<CreateReviewCommand>
    {
        public CreateReviewValidator()
        {
            // Validate user ID
            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("User ID must be greater than 0");

            // Validate review DTO
            RuleFor(x => x.CreateReviewDto)
                .NotNull().WithMessage("Review details are required");

            When(x => x.CreateReviewDto != null, () =>
            {
                // Validate hotel ID
                RuleFor(x => x.CreateReviewDto!.HotelId)
                    .GreaterThan(0).WithMessage("Hotel ID must be greater than 0");

                // Validate rating
                RuleFor(x => x.CreateReviewDto!.Rating)
                    .GreaterThanOrEqualTo(1).WithMessage("Rating must be at least 1")
                    .LessThanOrEqualTo(5).WithMessage("Rating cannot exceed 5");

                // Validate comment
                RuleFor(x => x.CreateReviewDto!.Comment)
                    .MaximumLength(1000).WithMessage("Comment cannot exceed 1000 characters");
            });
        }
    }
}
