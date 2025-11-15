using FluentValidation;
using Hotel_Booking_API.Application.Features.Bookings.Queries.CalculateBookingPrice;

namespace Hotel_Booking_API.Application.Validators.BookingValidators
{
    public class CalculateBookingPriceValidator : AbstractValidator<CalculateBookingPriceQuery>
    {
        public CalculateBookingPriceValidator()
        {
            RuleFor(x => x.RoomId)
            .GreaterThan(0)
            .WithMessage("RoomId must be a positive number.");

            RuleFor(x => x.CheckInDate)
                .GreaterThanOrEqualTo(DateTime.Today)
                .WithMessage("Check-in date cannot be in the past.");

            RuleFor(x => x.CheckOutDate)
                .GreaterThan(x => x.CheckInDate)
                .WithMessage("Check-out date must be after check-in date.");
        }
    }
}
