using FluentValidation;
using Hotel_Booking_API.Application.Features.Rooms.Queries.GetRoomById;

namespace Hotel_Booking_API.Application.Validators.RoomValidators
{
    /// <summary>
    /// Validator for GetRoomByIdQuery to ensure the room ID is valid.
    /// Validates the room ID parameter for proper format and range.
    /// </summary>
    public class GetRoomByIdValidator : AbstractValidator<GetRoomByIdQuery>
    {
        public GetRoomByIdValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Room ID must be greater than 0")
                .LessThanOrEqualTo(int.MaxValue).WithMessage("Room ID is invalid");
        }
    }
}
