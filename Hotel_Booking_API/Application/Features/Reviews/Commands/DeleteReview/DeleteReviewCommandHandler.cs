using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.Common.Exceptions;
using Hotel_Booking_API.Domain.Interfaces;
using MediatR;
using Serilog;

namespace Hotel_Booking_API.Application.Features.Reviews.Commands.DeleteReview
{
    /// <summary>
    /// Handler for deleting an existing review from the system.
    /// Validates business rules and performs soft or hard delete based on request.
    /// </summary>
    public class DeleteReviewCommandHandler : IRequestHandler<DeleteReviewCommand, ApiResponse<string>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteReviewCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Handles the review deletion request by validating business rules and removing the review.
        /// </summary>
        /// <param name="request">The delete review command containing review ID and deletion options</param>
        /// <param name="cancellationToken">Cancellation token for async operations</param>
        /// <returns>ApiResponse containing success message or error message</returns>
        public async Task<ApiResponse<string>> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
        {
            Log.Information("Starting {HandlerName} with request {@Request}", nameof(DeleteReviewCommandHandler), request);

            try
            {
                // Get the existing review
                var review = await _unitOfWork.Reviews.GetByIdAsync(request.Id, cancellationToken);

                if (review == null)
                {
                    Log.Warning("Review not found: {ReviewId}", request.Id);
                    throw new NotFoundException("Review", request.Id);
                }

                if (review.IsDeleted)
                {
                    Log.Warning("Review already deleted: {ReviewId}", request.Id);
                    throw new BadRequestException($"Review with ID {request.Id} is already deleted.");
                }

                // Perform deletion based on request
                if (request.IsSoft)
                {
                    // Soft delete: mark as deleted but keep the record
                    review.IsDeleted = true;
                    review.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.Reviews.UpdateAsync(review);
                    Log.Information("Review soft deleted successfully with ID {ReviewId}", review.Id);
                }
                else
                {
                    // Hard delete: permanently remove the record
                    await _unitOfWork.Reviews.DeleteAsync(review);
                    Log.Information("Review permanently deleted with ID {ReviewId}", review.Id);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var message = request.IsSoft
                    ? $"Review {request.Id} soft deleted successfully."
                    : $"Review {request.Id} permanently deleted.";

                Log.Information("Completed {HandlerName} successfully", nameof(DeleteReviewCommandHandler));

                return ApiResponse<string>.SuccessResponse(message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while processing {HandlerName}", nameof(DeleteReviewCommandHandler));
                throw;
            }
        }
    }
}
