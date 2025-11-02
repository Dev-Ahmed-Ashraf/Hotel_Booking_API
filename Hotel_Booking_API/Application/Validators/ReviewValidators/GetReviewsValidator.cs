using FluentValidation;
using Hotel_Booking_API.Application.Features.Reviews.Queries.GetReviews;

namespace Hotel_Booking_API.Application.Validators.ReviewValidators
{
    /// <summary>
    /// Validator for GetReviewsQuery to ensure all search parameters are valid.
    /// Validates pagination and search criteria.
    /// </summary>
    public class GetReviewsValidator : AbstractValidator<GetReviewsQuery>
    {
        public GetReviewsValidator()
        {
            // Validate pagination parameters
            RuleFor(x => x.Pagination.PageNumber)
                .GreaterThan(0).WithMessage("Page number must be greater than 0")
                .LessThanOrEqualTo(1000).WithMessage("Page number cannot exceed 1000");

            RuleFor(x => x.Pagination.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100");

            // Validate search criteria when provided
            When(x => x.Search != null, () =>
            {
                RuleFor(x => x.Search!.HotelId)
                    .GreaterThan(0).WithMessage("Hotel ID must be greater than 0")
                    .When(x => x.Search!.HotelId.HasValue);

                RuleFor(x => x.Search!.UserId)
                    .GreaterThan(0).WithMessage("User ID must be greater than 0")
                    .When(x => x.Search!.UserId.HasValue);

                RuleFor(x => x.Search!.MinRating)
                    .GreaterThanOrEqualTo(1).WithMessage("Minimum rating must be at least 1")
                    .LessThanOrEqualTo(5).WithMessage("Minimum rating cannot exceed 5")
                    .When(x => x.Search!.MinRating.HasValue);

                RuleFor(x => x.Search!.MaxRating)
                    .GreaterThanOrEqualTo(1).WithMessage("Maximum rating must be at least 1")
                    .LessThanOrEqualTo(5).WithMessage("Maximum rating cannot exceed 5")
                    .When(x => x.Search!.MaxRating.HasValue);

                RuleFor(x => x.Search!.MinRating)
                    .LessThanOrEqualTo(x => x.Search!.MaxRating).WithMessage("Minimum rating must be less than or equal to maximum rating")
                    .When(x => x.Search!.MinRating.HasValue && x.Search!.MaxRating.HasValue);
            });
        }
    }
}
