using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.DTOs;
using MediatR;

namespace Hotel_Booking_API.Application.Features.Reviews.Commands.CreateReview
{
    /// <summary>
    /// Command for creating a new review in the system.
    /// Validates user, hotel existence and rating constraints.
    /// </summary>
    public class CreateReviewCommand : IRequest<ApiResponse<ReviewDto>>
    {
        public CreateReviewDto CreateReviewDto { get; set; } = null!;
        public int UserId { get; set; }
    }
}
