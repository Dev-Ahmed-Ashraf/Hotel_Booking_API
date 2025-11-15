using FluentValidation;
using Hotel_Booking_API.Application.Features.Bookings.Queries.CheckRoomAvailability;

namespace Hotel_Booking_API.Application.Validators.BookingValidators
{
    public class CheckRoomAvailabilityValidator : AbstractValidator<CheckRoomAvailabilityQuery>
    {
        public CheckRoomAvailabilityValidator()
        {
            // RoomId must be positive
            RuleFor(x => x.RoomId)
                .GreaterThan(0)
                .WithMessage("RoomId must be a positive number.");

            // Check-in date must be today or later
            RuleFor(x => x.CheckInDate)
                .GreaterThanOrEqualTo(DateTime.Today)
                .WithMessage("Check-in date cannot be in the past.");

            // Check-out date must be after check-in date
            RuleFor(x => x.CheckOutDate)
                .GreaterThan(x => x.CheckInDate)
                .WithMessage("Check-out date must be after check-in date.");
        }
    }
}
