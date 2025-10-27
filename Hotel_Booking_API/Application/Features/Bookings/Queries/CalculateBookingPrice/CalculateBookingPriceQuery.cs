using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.DTOs;
using MediatR;

namespace Hotel_Booking_API.Application.Features.Bookings.Queries.CalculateBookingPrice
{
    /// <summary>
    /// Query for calculating the total price for a booking.
    /// Returns price breakdown with room rate and total cost.
    /// </summary>
    public class CalculateBookingPriceQuery : IRequest<ApiResponse<BookingPriceResponseDto>>
    {
        public int RoomId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
    }
}
