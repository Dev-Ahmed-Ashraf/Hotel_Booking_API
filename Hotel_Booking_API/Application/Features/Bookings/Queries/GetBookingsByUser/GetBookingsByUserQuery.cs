using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.DTOs;
using MediatR;

namespace Hotel_Booking_API.Application.Features.Bookings.Queries.GetBookingsByUser
{
    /// <summary>
    /// Query for retrieving all bookings for a specific user.
    /// Returns paginated list of user's bookings.
    /// </summary>
    public class GetBookingsByUserQuery : IRequest<ApiResponse<PagedList<BookingsForUserDto>>>
    {
        public int UserId { get; set; }
        public PaginationParameters Pagination { get; set; } = null!;
    }
}
