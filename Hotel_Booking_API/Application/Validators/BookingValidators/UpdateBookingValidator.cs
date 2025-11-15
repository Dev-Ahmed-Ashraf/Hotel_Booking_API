using FluentValidation;
using Hotel_Booking_API.Application.Features.Bookings.Commands.UpdateBooking;

namespace Hotel_Booking_API.Application.Validators.BookingValidators
{
    public class UpdateBookingValidator : AbstractValidator<UpdateBookingCommand>
    {
        public UpdateBookingValidator()
        {
            // Validate booking ID
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Booking ID must be greater than 0");

            // Validate booking DTO
            RuleFor(x => x.UpdateBookingDto)
                .NotNull().WithMessage("Booking update details are required");

            When(x => x.UpdateBookingDto != null, () =>
            {
                // Validate check-in date if provided
                RuleFor(x => x.UpdateBookingDto!.CheckInDate)
                    .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Check-in date cannot be in the past")
                    .When(x => x.UpdateBookingDto!.CheckInDate.HasValue);

                // Validate check-out date if provided
                RuleFor(x => x.UpdateBookingDto!.CheckOutDate)
                    .GreaterThan(x => x.UpdateBookingDto!.CheckInDate).WithMessage("Check-out date must be after check-in date")
                    .When(x => x.UpdateBookingDto!.CheckInDate.HasValue && x.UpdateBookingDto!.CheckOutDate.HasValue);

                // Validate booking duration if both dates are provided
                RuleFor(x => x.UpdateBookingDto!.CheckOutDate)
                    .LessThanOrEqualTo(x => x.UpdateBookingDto!.CheckInDate!.Value.AddDays(30))
                    .WithMessage("Booking duration cannot exceed 30 days")
                    .When(x => x.UpdateBookingDto!.CheckInDate.HasValue && x.UpdateBookingDto!.CheckOutDate.HasValue);
            });
        }
    }
}
