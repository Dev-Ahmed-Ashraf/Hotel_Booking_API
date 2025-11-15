using Hotel_Booking_API.Application.Common;
using MediatR;

namespace Hotel_Booking_API.Application.Features.Rooms.Commands.DeleteRoom
{
    /// <summary>
    /// Command to delete a room from the system.
    /// Supports both soft delete (default) and hard delete options.
    /// </summary>
    public class DeleteRoomCommand : IRequest<ApiResponse<string>>
    {

        public int Id { get; set; }
        public bool IsSoft { get; set; } = true;
        public bool ForceDelete { get; set; } = false;
    }
}
