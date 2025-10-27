using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Domain.Enums;
using MediatR;

namespace Hotel_Booking_API.Application.Features.Rooms.Queries.GetAvailableRooms
{
    /// <summary>
    /// Query to retrieve available rooms for a specific date range.
    /// Checks room availability by examining existing bookings and room status.
    /// </summary>
    public class GetAvailableRoomsQuery : IRequest<ApiResponse<List<RoomDto>>>
    {
        /// <summary>
        /// The hotel ID to search for available rooms. If null, searches all hotels.
        /// </summary>
        public int? HotelId { get; set; }
        
        /// <summary>
        /// The check-in date for the booking.
        /// </summary>
        public DateTime CheckInDate { get; set; }
        
        /// <summary>
        /// The check-out date for the booking.
        /// </summary>
        public DateTime CheckOutDate { get; set; }
        
        /// <summary>
        /// Optional room type filter.
        /// </summary>
        public RoomType? Type { get; set; }
        
        /// <summary>
        /// Optional minimum capacity filter.
        /// </summary>
        public int? MinCapacity { get; set; }
        
        /// <summary>
        /// Optional maximum price filter.
        /// </summary>
        public decimal? MaxPrice { get; set; }
    }
}
