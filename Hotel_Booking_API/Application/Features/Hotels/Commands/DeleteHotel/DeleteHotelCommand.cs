using Hotel_Booking_API.Application.Common;
using MediatR;

namespace Hotel_Booking_API.Application.Features.Hotels.Commands.DeleteHotel
{
    /// <summary>
    /// Command to delete a hotel from the system.
    /// Supports both soft delete (default) and hard delete options.
    /// </summary>
    public class DeleteHotelCommand : IRequest<ApiResponse<string>>
    {
        /// <summary>
        /// The ID of the hotel to delete.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// If true, performs soft delete (marks as deleted). If false, permanently removes the hotel.
        /// Default is true for soft delete.
        /// </summary>
        public bool IsSoft { get; set; } = true;

        /// <summary>
        /// If true, forces deletion even if hotel has active bookings (use with caution).
        /// Default is false.
        /// </summary>
        public bool ForceDelete { get; set; } = false;
    }
}
