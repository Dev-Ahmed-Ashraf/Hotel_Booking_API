using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.DTOs;
using MediatR;

namespace Hotel_Booking_API.Application.Features.Bookings.Queries.GetBookings
{
    /// <summary>
    /// Query for retrieving a paginated list of bookings with optional filtering.
    /// Supports filtering by hotel, user, status, and date range.
    /// </summary>
    public class GetBookingsQuery : IRequest<ApiResponse<PagedList<BookingDto>>>
    {
        public PaginationParameters Pagination { get; set; } = null!;
        public SearchBookingsDto? Search { get; set; }
        public bool IncludeDeleted { get; set; } = false;
    }
}
