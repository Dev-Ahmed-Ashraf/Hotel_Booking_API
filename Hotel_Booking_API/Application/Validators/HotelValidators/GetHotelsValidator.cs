using FluentValidation;
using Hotel_Booking_API.Application.Features.Hotels.Queries.GetHotels;

namespace Hotel_Booking_API.Application.Validators.HotelValidators
{
    public class GetHotelsValidator : AbstractValidator<GetHotelsQuery>
    {
        public GetHotelsValidator()
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
                RuleFor(x => x.Search!.City)
                    .MaximumLength(100).WithMessage("City name cannot exceed 100 characters")
                    .Matches(@"^[A-Za-z\s\-\.']+$").WithMessage("City name can only contain letters, spaces, hyphens, dots, and apostrophes")
                    .When(x => !string.IsNullOrWhiteSpace(x.Search!.City));

                RuleFor(x => x.Search!.Country)
                    .MaximumLength(100).WithMessage("Country name cannot exceed 100 characters")
                    .Matches(@"^[A-Za-z\s\-\.']+$").WithMessage("Country name can only contain letters, spaces, hyphens, dots, and apostrophes")
                    .When(x => !string.IsNullOrWhiteSpace(x.Search!.Country));

                RuleFor(x => x.Search!.MinRating)
                    .GreaterThanOrEqualTo(0).WithMessage("Minimum rating cannot be negative")
                    .LessThanOrEqualTo(5).WithMessage("Minimum rating cannot exceed 5")
                    .When(x => x.Search!.MinRating.HasValue);

                RuleFor(x => x.Search!.MaxRating)
                    .GreaterThanOrEqualTo(0).WithMessage("Maximum rating cannot be negative")
                    .LessThanOrEqualTo(5).WithMessage("Maximum rating cannot exceed 5")
                    .When(x => x.Search!.MaxRating.HasValue);

                // Ensure max rating is greater than min rating when both are provided
                RuleFor(x => x.Search!.MaxRating)
                    .GreaterThan(x => x.Search!.MinRating).WithMessage("Maximum rating must be greater than minimum rating")
                    .When(x => x.Search!.MinRating.HasValue && x.Search!.MaxRating.HasValue);
            });
        }
    }
}
