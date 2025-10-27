using FluentValidation;
using Hotel_Booking_API.Application.Features.Bookings.Commands.CreateBooking;

namespace Hotel_Booking_API.Application.Validators.BookingValidators
{
    /// <summary>
    /// Validator for CreateBookingCommand to ensure all booking parameters are valid.
    /// Validates dates, room ID, and business rules.
    /// </summary>
    public class CreateBookingValidator : AbstractValidator<CreateBookingCommand>
    {
        public CreateBookingValidator()
        {
            // Validate user ID
            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("User ID must be greater than 0");

            // Validate booking DTO
            RuleFor(x => x.CreateBookingDto)
                .NotNull().WithMessage("Booking details are required");

            When(x => x.CreateBookingDto != null, () =>
            {
                // Validate room ID
                RuleFor(x => x.CreateBookingDto!.RoomId)
                    .GreaterThan(0).WithMessage("Room ID must be greater than 0");

                // Validate check-in date
                RuleFor(x => x.CreateBookingDto!.CheckInDate)
                    .NotEmpty().WithMessage("Check-in date is required")
                    .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Check-in date cannot be in the past");

                // Validate check-out date
                RuleFor(x => x.CreateBookingDto!.CheckOutDate)
                    .NotEmpty().WithMessage("Check-out date is required")
                    .GreaterThan(x => x.CreateBookingDto!.CheckInDate).WithMessage("Check-out date must be after check-in date");

                // Validate booking duration
                RuleFor(x => x.CreateBookingDto!.CheckOutDate)
                    .LessThanOrEqualTo(x => x.CreateBookingDto!.CheckInDate.AddDays(30))
                    .WithMessage("Booking duration cannot exceed 30 days")
                    .When(x => x.CreateBookingDto!.CheckInDate != default && x.CreateBookingDto!.CheckOutDate != default);
            });
        }
    }
}
