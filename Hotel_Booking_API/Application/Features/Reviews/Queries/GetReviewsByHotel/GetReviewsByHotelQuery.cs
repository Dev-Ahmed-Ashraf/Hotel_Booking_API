using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.DTOs;
using MediatR;

namespace Hotel_Booking_API.Application.Features.Reviews.Queries.GetReviewsByHotel
{
    /// <summary>
    /// Query for retrieving all reviews for a specific hotel.
    /// Returns paginated list of reviews for the hotel.
    /// </summary>
    public class GetReviewsByHotelQuery : IRequest<ApiResponse<PagedList<ReviewDto>>>
    {
        public int HotelId { get; set; }
        public PaginationParameters Pagination { get; set; } = null!;
    }
}
