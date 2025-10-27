using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.DTOs;
using MediatR;

namespace Hotel_Booking_API.Application.Features.Bookings.Queries.GetBookingById
{
    /// <summary>
    /// Query for retrieving a specific booking by its ID.
    /// Returns full booking details including hotel, room, and user info.
    /// </summary>
    public class GetBookingByIdQuery : IRequest<ApiResponse<BookingDto>>
    {
        public int Id { get; set; }
    }
}
