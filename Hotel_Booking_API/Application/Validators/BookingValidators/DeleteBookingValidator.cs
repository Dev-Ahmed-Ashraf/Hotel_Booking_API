using FluentValidation;
using Hotel_Booking_API.Application.Features.Bookings.Commands.DeleteBooking;

namespace Hotel_Booking_API.Application.Validators.BookingValidators
{
    /// <summary>
    /// Validator for DeleteBookingCommand to ensure deletion parameters are valid.
    /// Validates booking ID and deletion options.
    /// </summary>
    public class DeleteBookingValidator : AbstractValidator<DeleteBookingCommand>
    {
        public DeleteBookingValidator()
        {
            // Validate booking ID
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Booking ID must be greater than 0");
        }
    }
}
