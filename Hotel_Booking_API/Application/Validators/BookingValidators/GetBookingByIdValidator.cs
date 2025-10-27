using FluentValidation;
using Hotel_Booking_API.Application.Features.Bookings.Queries.GetBookingById;

namespace Hotel_Booking_API.Application.Validators.BookingValidators
{
    /// <summary>
    /// Validator for GetBookingByIdQuery to ensure query parameters are valid.
    /// Validates booking ID.
    /// </summary>
    public class GetBookingByIdValidator : AbstractValidator<GetBookingByIdQuery>
    {
        public GetBookingByIdValidator()
        {
            // Validate booking ID
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Booking ID must be greater than 0");
        }
    }
}
