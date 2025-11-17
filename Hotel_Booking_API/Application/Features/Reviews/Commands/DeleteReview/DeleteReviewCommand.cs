using Hotel_Booking_API.Application.Common;
using MediatR;

namespace Hotel_Booking_API.Application.Features.Reviews.Commands.DeleteReview
{
    /// <summary>
    /// Command for deleting an existing review in the system.
    /// Performs soft delete by default with optional hard delete.
    /// </summary>
    public class DeleteReviewCommand : IRequest<ApiResponse<string>>
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public bool IsSoft { get; set; } = true;
    }
}
