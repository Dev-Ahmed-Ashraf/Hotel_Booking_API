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
        /// <summary>
        /// The ID of the room to delete.
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// If true, performs soft delete (marks as deleted). If false, permanently removes the room.
        /// Default is true for soft delete.
        /// </summary>
        public bool IsSoft { get; set; } = true;
        
        /// <summary>
        /// If true, forces deletion even if room has active bookings (use with caution).
        /// Default is false.
        /// </summary>
        public bool ForceDelete { get; set; } = false;
    }
}
