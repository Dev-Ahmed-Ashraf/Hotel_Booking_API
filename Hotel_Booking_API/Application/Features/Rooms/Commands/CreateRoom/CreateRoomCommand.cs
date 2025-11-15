using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.DTOs;
using MediatR;

namespace Hotel_Booking_API.Application.Features.Rooms.Commands.CreateRoom
{
    /// <summary>
    /// Command to create a new room in the system.
    /// This command encapsulates the data needed to create a room and returns the created room details.
    /// </summary>
    public class CreateRoomCommand : IRequest<ApiResponse<RoomDto>>
    {
        public CreateRoomDto CreateRoomDto { get; set; } = null!;
    }
}
