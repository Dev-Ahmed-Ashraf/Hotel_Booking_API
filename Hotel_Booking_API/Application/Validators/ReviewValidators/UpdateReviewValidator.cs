using FluentValidation;
using Hotel_Booking_API.Application.Features.Reviews.Commands.UpdateReview;

namespace Hotel_Booking_API.Application.Validators.ReviewValidators
{
    /// <summary>
    /// Validator for UpdateReviewCommand to ensure all update parameters are valid.
    /// Validates review ID, rating, and comment.
    /// </summary>
    public class UpdateReviewValidator : AbstractValidator<UpdateReviewCommand>
    {
        public UpdateReviewValidator()
        {
            // Validate review ID
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Review ID must be greater than 0");

            // Validate review DTO
            RuleFor(x => x.UpdateReviewDto)
                .NotNull().WithMessage("Review update details are required");

            When(x => x.UpdateReviewDto != null, () =>
            {
                // Validate rating when provided
                RuleFor(x => x.UpdateReviewDto!.Rating)
                    .GreaterThanOrEqualTo(1).WithMessage("Rating must be at least 1")
                    .LessThanOrEqualTo(5).WithMessage("Rating cannot exceed 5")
                    .When(x => x.UpdateReviewDto!.Rating.HasValue);

                // Validate comment when provided (non-empty)
                RuleFor(x => x.UpdateReviewDto!.Comment)
                    .MaximumLength(1000).WithMessage("Comment cannot exceed 1000 characters")
                    .When(x => !string.IsNullOrWhiteSpace(x.UpdateReviewDto!.Comment));

                // Ensure at least one field is provided for update
                RuleFor(x => x)
                    .Must(x => x.UpdateReviewDto!.Rating.HasValue || !string.IsNullOrWhiteSpace(x.UpdateReviewDto!.Comment))
                    .WithMessage("At least one field (Rating or Comment) must be provided for update");
            });
        }
    }
}
