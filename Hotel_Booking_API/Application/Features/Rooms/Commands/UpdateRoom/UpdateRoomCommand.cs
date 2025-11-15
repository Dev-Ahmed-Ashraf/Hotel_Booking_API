using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.DTOs;
using MediatR;

namespace Hotel_Booking_API.Application.Features.Rooms.Commands.UpdateRoom
{
    /// <summary>
    /// Command to update an existing room in the system.
    /// Supports partial updates where only provided fields are updated.
    /// </summary>
    public class UpdateRoomCommand : IRequest<ApiResponse<RoomDto>>
    {

        public int Id { get; set; }
        public UpdateRoomDto UpdateRoomDto { get; set; } = null!;
    }
}
