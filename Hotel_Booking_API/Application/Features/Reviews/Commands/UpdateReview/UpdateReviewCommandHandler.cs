using AutoMapper;
using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.Common.Exceptions;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
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

        public async Task<ApiResponse<ReviewDto>> Handle(UpdateReviewCommand request, CancellationToken cancellationToken)
        {
            Log.Information("Starting {HandlerName} with request {@Request}", nameof(UpdateReviewCommandHandler), request);

            try
            {
                // Get the existing review with related entities
                var review = await _unitOfWork.Reviews.GetByIdAsync(request.Id, cancellationToken);

                if (review is null || review.IsDeleted)
                {
                    Log.Warning("Review not found or deleted: {ReviewId}", request.Id);
                    throw new NotFoundException("Review", request.Id);
                }

                // Only review owner can update
                if (review.UserId != request.UserId)
                {
                    Log.Warning("Unauthorized update attempt: User {UserId} attempted to modify Review {ReviewId}", request.UserId, review.Id);
                    throw new ForbiddenException("You are not allowed to update this review.");
                }

                var dto = request.UpdateReviewDto;

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
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Re-load with includes for mapping
                var reviewWithIncludes = await _unitOfWork.Reviews.Query()
                    .Include(r => r.User)
                    .Include(r => r.Hotel)
                    .FirstOrDefaultAsync(r => r.Id == review.Id, cancellationToken);

                // Map entity back to DTO for response
                var reviewDto = _mapper.Map<ReviewDto>(reviewWithIncludes);

                Log.Information("Review updated successfully with ID {ReviewId}", review.Id);

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
