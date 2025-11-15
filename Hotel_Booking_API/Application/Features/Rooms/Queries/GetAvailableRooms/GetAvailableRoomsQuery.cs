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
        public AvailableRoomsDto? filter { get; set; }
    }
}
