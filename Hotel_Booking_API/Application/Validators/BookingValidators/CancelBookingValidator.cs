using FluentValidation;
using Hotel_Booking_API.Application.Features.Bookings.Commands.CancelBooking;

namespace Hotel_Booking_API.Application.Validators.BookingValidators
{
    /// <summary>
    /// Validator for CancelBookingCommand to ensure cancellation parameters are valid.
    /// Validates booking ID and cancellation details.
    /// </summary>
    public class CancelBookingValidator : AbstractValidator<CancelBookingCommand>
    {
        public CancelBookingValidator()
        {
            // Validate booking ID
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Booking ID must be greater than 0");

            // Validate cancellation DTO
            RuleFor(x => x.CancelBookingDto)
                .NotNull().WithMessage("Cancellation details are required");

            When(x => x.CancelBookingDto != null, () =>
            {
                // Validate reason length if provided
                RuleFor(x => x.CancelBookingDto!.Reason)
                    .MaximumLength(500).WithMessage("Cancellation reason cannot exceed 500 characters")
                    .When(x => !string.IsNullOrWhiteSpace(x.CancelBookingDto!.Reason));
            });
        }
    }
}
