using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.DTOs;
using MediatR;

namespace Hotel_Booking_API.Application.Features.Bookings.Queries.GetBookingsByHotel
{
    /// <summary>
    /// Query for retrieving all bookings for a specific hotel.
    /// Returns paginated list of hotel's bookings.
    /// </summary>
    public class GetBookingsByHotelQuery : IRequest<ApiResponse<PagedList<BookingDto>>>
    {
        public int HotelId { get; set; }
        public PaginationParameters Pagination { get; set; } = null!;
    }
}
