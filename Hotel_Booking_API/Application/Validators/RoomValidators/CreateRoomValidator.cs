using FluentValidation;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Application.Features.Rooms.Commands.CreateRoom;
using Hotel_Booking_API.Domain.Enums;

namespace Hotel_Booking_API.Application.Validators.RoomValidators
{
    /// <summary>
    /// Validator for CreateRoomCommand to ensure all business rules and constraints are met.
    /// Validates room data before creation to maintain data integrity.
    /// </summary>
    public class CreateRoomValidator : AbstractValidator<CreateRoomCommand>
    {
        public CreateRoomValidator()
        {
            // Validate hotel ID is provided and greater than 0
            RuleFor(x => x.CreateRoomDto.HotelId)
                .GreaterThan(0).WithMessage("Hotel ID must be greater than 0");

            // Validate room number is required and within length limits
            RuleFor(x => x.CreateRoomDto.RoomNumber)
                .NotEmpty().WithMessage("Room number is required")
                .MaximumLength(50).WithMessage("Room number cannot exceed 50 characters")
                .Matches(@"^[A-Za-z0-9\-\s]+$").WithMessage("Room number can only contain letters, numbers, hyphens, and spaces");

            // Validate room type is a valid enum value
            RuleFor(x => x.CreateRoomDto.Type)
                .IsInEnum().WithMessage("Invalid room type");

            // Validate price is positive
            RuleFor(x => x.CreateRoomDto.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0")
                .LessThanOrEqualTo(10000).WithMessage("Price cannot exceed $10,000 per night");

            // Validate description length if provided
            RuleFor(x => x.CreateRoomDto.Description)
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

            RuleFor(x => x.CreateRoomDto.Type)
                .NotNull().WithMessage("Room type is required.");

            // Validate capacity is positive and reasonable
            RuleFor(x => x.CreateRoomDto.Capacity)
                .GreaterThan(0).WithMessage("Capacity must be greater than 0")
                .LessThanOrEqualTo(10).WithMessage("Capacity cannot exceed 10 guests");

            When(x => x.CreateRoomDto.Type == RoomType.Standard, () =>
            {
                RuleFor(x => x.CreateRoomDto.Capacity)
                    .LessThanOrEqualTo(2).WithMessage("Standard rooms can hold up to 2 people only");
            });

            When(x => x.CreateRoomDto.Type == RoomType.Deluxe, () =>
            {
                RuleFor(x => x.CreateRoomDto.Capacity)
                    .LessThanOrEqualTo(3).WithMessage("Deluxe rooms can hold up to 3 people only");
            });

            When(x => x.CreateRoomDto.Type == RoomType.Suite, () =>
            {
                RuleFor(x => x.CreateRoomDto.Capacity)
                    .LessThanOrEqualTo(4).WithMessage("Suites can hold up to 4 people only");
            });

            When(x => x.CreateRoomDto.Type == RoomType.Presidential, () =>
            {
                RuleFor(x => x.CreateRoomDto.Capacity)
                    .LessThanOrEqualTo(6).WithMessage("Presidential suites can hold up to 6 people only");
            });
        }
    }
}
