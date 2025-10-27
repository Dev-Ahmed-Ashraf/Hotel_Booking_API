using FluentValidation;
using Hotel_Booking_API.Application.Features.Bookings.Queries.GetBookingsByUser;

namespace Hotel_Booking_API.Application.Validators.BookingValidators
{
    /// <summary>
    /// Validator for GetBookingsByUserQuery to ensure query parameters are valid.
    /// Validates user ID and pagination parameters.
    /// </summary>
    public class GetBookingsByUserValidator : AbstractValidator<GetBookingsByUserQuery>
    {
        public GetBookingsByUserValidator()
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
