using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.DTOs;
using MediatR;

namespace Hotel_Booking_API.Application.Features.Reviews.Queries.GetReviewsByUser
{
    /// <summary>
    /// Query for retrieving all reviews for a specific user.
    /// Returns paginated list of reviews by the user.
    /// </summary>
    public class GetReviewsByUserQuery : IRequest<ApiResponse<PagedList<ReviewDto>>>
    {
        public int UserId { get; set; }
        public PaginationParameters Pagination { get; set; } = null!;
    }
}
