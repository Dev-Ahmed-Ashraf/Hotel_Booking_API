using FluentValidation;
using Hotel_Booking_API.Application.Features.Bookings.Queries.GetBookings;

namespace Hotel_Booking_API.Application.Validators.BookingValidators
{
    /// <summary>
    /// Validator for GetBookingsQuery to ensure all search parameters are valid.
    /// Validates pagination and search criteria.
    /// </summary>
    public class GetBookingsValidator : AbstractValidator<GetBookingsQuery>
    {
        public GetBookingsValidator()
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

                RuleFor(x => x.Search!.Status)
                    .IsInEnum().WithMessage("Invalid booking status")
                    .When(x => x.Search!.Status.HasValue);

                RuleFor(x => x.Search!.StartDate)
                    .LessThan(x => x.Search!.EndDate).WithMessage("Start date must be before end date")
                    .When(x => x.Search!.StartDate.HasValue && x.Search!.EndDate.HasValue);

                RuleFor(x => x.Search!.EndDate)
                    .GreaterThan(x => x.Search!.StartDate).WithMessage("End date must be after start date")
                    .When(x => x.Search!.StartDate.HasValue && x.Search!.EndDate.HasValue);
            });
        }
    }
}
