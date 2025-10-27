using MediatR;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Application.Common;

namespace Hotel_Booking_API.Application.Features.Rooms.Commands.CreateRoom
{
    /// <summary>
    /// Command to create a new room in the system.
    /// This command encapsulates the data needed to create a room and returns the created room details.
    /// </summary>
    public class CreateRoomCommand : IRequest<ApiResponse<RoomDto>>
    {
        /// <summary>
        /// The room details to create, including hotel ID, room number, type, price, capacity, and description.
        /// </summary>
        public CreateRoomDto CreateRoomDto { get; set; } = null!;
    }
}
