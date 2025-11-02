using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.DTOs;
using MediatR;

namespace Hotel_Booking_API.Application.Features.Reviews.Queries.GetReviews
{
    /// <summary>
    /// Query for retrieving a paginated list of reviews with optional filtering.
    /// Supports filtering by hotel, user, and rating range.
    /// </summary>
    public class GetReviewsQuery : IRequest<ApiResponse<PagedList<ReviewDto>>>
    {
        public PaginationParameters Pagination { get; set; } = null!;
        public SearchReviewsDto? Search { get; set; }
        public bool IncludeDeleted { get; set; } = false;
    }
}
