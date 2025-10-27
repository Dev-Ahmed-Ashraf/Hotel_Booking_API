using FluentValidation;
using Hotel_Booking_API.Application.Features.Rooms.Queries.GetAvailableRooms;

namespace Hotel_Booking_API.Application.Validators.RoomValidators
{
    /// <summary>
    /// Validator for GetAvailableRoomsQuery to ensure all search parameters are valid.
    /// Validates date ranges, hotel ID, and other search criteria.
    /// </summary>
    public class GetAvailableRoomsValidator : AbstractValidator<GetAvailableRoomsQuery>
    {
        public GetAvailableRoomsValidator()
        {
            // Validate hotel ID if provided
            RuleFor(x => x.HotelId)
                .GreaterThan(0).WithMessage("Hotel ID must be greater than 0")
                .When(x => x.HotelId.HasValue);

            // Validate check-in date
            RuleFor(x => x.CheckInDate)
                .NotEmpty().WithMessage("Check-in date is required")
                .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Check-in date cannot be in the past")
                .LessThan(DateTime.Today.AddYears(2)).WithMessage("Check-in date cannot be more than 2 years in the future");

            // Validate check-out date
            RuleFor(x => x.CheckOutDate)
                .NotEmpty().WithMessage("Check-out date is required")
                .GreaterThan(x => x.CheckInDate).WithMessage("Check-out date must be after check-in date")
                .LessThan(DateTime.Today.AddYears(2)).WithMessage("Check-out date cannot be more than 2 years in the future");

            // Validate room type if provided
            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("Invalid room type")
                .When(x => x.Type.HasValue);

            // Validate minimum capacity if provided
            RuleFor(x => x.MinCapacity)
                .GreaterThan(0).WithMessage("Minimum capacity must be greater than 0")
                .LessThanOrEqualTo(10).WithMessage("Minimum capacity cannot exceed 10 guests")
                .When(x => x.MinCapacity.HasValue);

            // Validate maximum price if provided
            RuleFor(x => x.MaxPrice)
                .GreaterThan(0).WithMessage("Maximum price must be greater than 0")
                .LessThanOrEqualTo(10000).WithMessage("Maximum price cannot exceed $10,000")
                .When(x => x.MaxPrice.HasValue);

            // Validate date range duration (not too long)
            RuleFor(x => x.CheckOutDate)
                .LessThanOrEqualTo(x => x.CheckInDate.AddDays(30))
                .WithMessage("Booking duration cannot exceed 30 days")
                .When(x => x.CheckInDate != default && x.CheckOutDate != default);
        }
    }
}
