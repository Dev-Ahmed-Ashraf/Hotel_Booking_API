using Hotel_Booking_API.Application.Common;
using MediatR;

namespace Hotel_Booking_API.Application.Features.Bookings.Queries.CheckRoomAvailability
{
    /// <summary>
    /// Query for checking if a room is available for a specific date range.
    /// Returns boolean result indicating availability.
    /// </summary>
    public class CheckRoomAvailabilityQuery : IRequest<ApiResponse<bool>>
    {
        public int RoomId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int? ExcludeBookingId { get; set; } // For updates
    }
}
