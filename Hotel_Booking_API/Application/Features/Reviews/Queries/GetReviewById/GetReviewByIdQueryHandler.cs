using AutoMapper;
using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.Common.Exceptions;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Domain.Interfaces;
using MediatR;
using Serilog;

namespace Hotel_Booking_API.Application.Features.Reviews.Queries.GetReviewById
{
    /// <summary>
    /// Handler for retrieving a specific review by its ID.
    /// Includes full review details with related entities.
    /// </summary>
    public class GetReviewByIdQueryHandler : IRequestHandler<GetReviewByIdQuery, ApiResponse<ReviewDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetReviewByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Handles the get review by ID query by retrieving the review with related entities.
        /// </summary>
        /// <param name="request">The query containing the review ID</param>
        /// <param name="cancellationToken">Cancellation token for async operations</param>
        /// <returns>ApiResponse containing the review details or error message</returns>
        public async Task<ApiResponse<ReviewDto>> Handle(GetReviewByIdQuery request, CancellationToken cancellationToken)
        {
            Log.Information("Starting {HandlerName} with request {@Request}", nameof(GetReviewByIdQueryHandler), request);

            try
            {
                // Get the review with all related entities included
                var review = await _unitOfWork.Reviews.GetByIdAsync(
                    request.Id,
                    cancellationToken,
                    r => r.User,
                    r => r.Hotel
                );

                // Check if review exists and is not deleted
                if (review is null || review.IsDeleted)
                {
                    Log.Warning("Review not found or deleted: {ReviewId}", request.Id);
                    throw new NotFoundException("Review", request.Id);
                }

                // Map entity to DTO for response
                var reviewDto = _mapper.Map<ReviewDto>(review);

                Log.Information("Review retrieved successfully with ID {ReviewId} for user {UserId}", review.Id, review.UserId);
                Log.Information("Completed {HandlerName} successfully", nameof(GetReviewByIdQueryHandler));

                return ApiResponse<ReviewDto>.SuccessResponse(reviewDto, "Review retrieved successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while processing {HandlerName}", nameof(GetReviewByIdQueryHandler));
                throw;
            }
        }
    }
}
