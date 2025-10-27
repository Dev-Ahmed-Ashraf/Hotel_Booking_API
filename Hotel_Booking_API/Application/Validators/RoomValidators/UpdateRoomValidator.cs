using FluentValidation;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Application.Features.Rooms.Commands.UpdateRoom;

namespace Hotel_Booking_API.Application.Validators.RoomValidators
{
    /// <summary>
    /// Validator for UpdateRoomCommand to ensure all business rules and constraints are met.
    /// Validates room data before update to maintain data integrity.
    /// </summary>
    public class UpdateRoomValidator : AbstractValidator<UpdateRoomCommand>
    {
        public UpdateRoomValidator()
        {
            // Validate room ID is provided and greater than 0
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Room ID must be greater than 0");

            // Validate room number if provided
            RuleFor(x => x.UpdateRoomDto.RoomNumber)
                .MaximumLength(50).WithMessage("Room number cannot exceed 50 characters")
                .Matches(@"^[A-Za-z0-9\-\s]+$").WithMessage("Room number can only contain letters, numbers, hyphens, and spaces")
                .When(x => !string.IsNullOrWhiteSpace(x.UpdateRoomDto.RoomNumber));

            // Validate room type if provided
            RuleFor(x => x.UpdateRoomDto.Type)
                .NotNull().WithMessage("Type is Required")
                .IsInEnum().WithMessage("Invalid room type")
                .When(x => x.UpdateRoomDto.Type != default);

            // Validate price if provided
            RuleFor(x => x.UpdateRoomDto.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0")
                .LessThanOrEqualTo(10000).WithMessage("Price cannot exceed $10,000 per night")
                .When(x => x.UpdateRoomDto.Price > 0);

            // Validate capacity if provided
            RuleFor(x => x.UpdateRoomDto.Capacity)
                .GreaterThan(0).WithMessage("Capacity must be greater than 0")
                .LessThanOrEqualTo(10).WithMessage("Capacity cannot exceed 10 guests")
                .When(x => x.UpdateRoomDto.Capacity > 0);

            // Validate description length if provided
            RuleFor(x => x.UpdateRoomDto.Description)
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.UpdateRoomDto.Description));
        }
    }
}
