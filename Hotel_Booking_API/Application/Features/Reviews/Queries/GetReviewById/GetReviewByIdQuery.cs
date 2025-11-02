using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.DTOs;
using MediatR;

namespace Hotel_Booking_API.Application.Features.Reviews.Queries.GetReviewById
{
    /// <summary>
    /// Query for retrieving a specific review by its ID.
    /// Returns full review details including hotel and user info.
    /// </summary>
    public class GetReviewByIdQuery : IRequest<ApiResponse<ReviewDto>>
    {
        public int Id { get; set; }
    }
}
