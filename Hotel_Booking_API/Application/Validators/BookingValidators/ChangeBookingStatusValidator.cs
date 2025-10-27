using FluentValidation;
using Hotel_Booking_API.Application.Features.Bookings.Commands.ChangeBookingStatus;

namespace Hotel_Booking_API.Application.Validators.BookingValidators
{
    /// <summary>
    /// Validator for ChangeBookingStatusCommand to ensure status change parameters are valid.
    /// Validates booking ID and new status.
    /// </summary>
    public class ChangeBookingStatusValidator : AbstractValidator<ChangeBookingStatusCommand>
    {
        public ChangeBookingStatusValidator()
        {
            // Validate booking ID
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Booking ID must be greater than 0");

            // Validate status change DTO
            RuleFor(x => x.ChangeBookingStatusDto)
                .NotNull().WithMessage("Status change details are required");

            When(x => x.ChangeBookingStatusDto != null, () =>
            {
                // Validate new status
                RuleFor(x => x.ChangeBookingStatusDto!.Status)
                    .IsInEnum().WithMessage("Invalid booking status");

                // Validate notes length if provided
                RuleFor(x => x.ChangeBookingStatusDto!.Notes)
                    .MaximumLength(500).WithMessage("Status change notes cannot exceed 500 characters")
                    .When(x => !string.IsNullOrWhiteSpace(x.ChangeBookingStatusDto!.Notes));
            });
        }
    }
}
