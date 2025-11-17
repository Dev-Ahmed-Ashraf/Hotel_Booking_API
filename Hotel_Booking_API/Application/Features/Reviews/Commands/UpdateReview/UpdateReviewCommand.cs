using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.DTOs;
using MediatR;

namespace Hotel_Booking_API.Application.Features.Reviews.Commands.UpdateReview
{
    /// <summary>
    /// Command for updating an existing review in the system.
    /// Allows partial updates of review details.
    /// </summary>
    public class UpdateReviewCommand : IRequest<ApiResponse<ReviewDto>>
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public UpdateReviewDto UpdateReviewDto { get; set; } = null!;
    }
}
