using AutoMapper;
using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.Common.Exceptions;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Domain.Interfaces;
using MediatR;
using Serilog;

namespace Hotel_Booking_API.Application.Features.Reviews.Commands.UpdateReview
{
    /// <summary>
    /// Handler for updating an existing review in the system.
    /// Validates business rules and updates the review if all conditions are met.
    /// </summary>
    public class UpdateReviewCommandHandler : IRequestHandler<UpdateReviewCommand, ApiResponse<ReviewDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateReviewCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Handles the review update request by validating business rules and persisting changes.
        /// </summary>
        /// <param name="request">The update review command containing review ID and update details</param>
        /// <param name="cancellationToken">Cancellation token for async operations</param>
        /// <returns>ApiResponse containing the updated review details or error message</returns>
        public async Task<ApiResponse<ReviewDto>> Handle(UpdateReviewCommand request, CancellationToken cancellationToken)
        {
            Log.Information("Starting {HandlerName} with request {@Request}", nameof(UpdateReviewCommandHandler), request);

            try
            {
                // Get the existing review with related entities
                var review = await _unitOfWork.Reviews.GetByIdAsync(
                    request.Id,
                    cancellationToken,
                    r => r.User,
                    r => r.Hotel
                );

                if (review is null || review.IsDeleted)
                {
                    Log.Warning("Review not found or deleted: {ReviewId}", request.Id);
                    throw new NotFoundException("Review", request.Id);
                }

                var dto = request.UpdateReviewDto;

                // Validate rating if provided
                if (dto.Rating.HasValue && (dto.Rating.Value < 1 || dto.Rating.Value > 5))
                {
                    Log.Warning("Invalid rating provided: {Rating}", dto.Rating.Value);
                    throw new BadRequestException("Rating must be between 1 and 5.");
                }

                // Apply partial updates - only update fields that are provided
                if (dto.Rating.HasValue)
                {
                    review.Rating = dto.Rating.Value;
                }

                if (!string.IsNullOrWhiteSpace(dto.Comment))
                {
                    review.Comment = dto.Comment;
                }

                // Update timestamp
                review.UpdatedAt = DateTime.UtcNow;

                // Save changes
                await _unitOfWork.Reviews.UpdateAsync(review);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Map entity back to DTO for response
                var reviewDto = _mapper.Map<ReviewDto>(review);

                Log.Information("Review updated successfully with ID {ReviewId}", review.Id);
                Log.Information("Completed {HandlerName} successfully", nameof(UpdateReviewCommandHandler));

                return ApiResponse<ReviewDto>.SuccessResponse(reviewDto, "Review updated successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while processing {HandlerName}", nameof(UpdateReviewCommandHandler));
                throw;
            }
        }
    }
}
