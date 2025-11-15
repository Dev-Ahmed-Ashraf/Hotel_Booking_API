using AutoMapper;
using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.Common.Exceptions;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Domain.Entities;
using Hotel_Booking_API.Domain.Interfaces;
using MediatR;
using Serilog;

namespace Hotel_Booking_API.Application.Features.Reviews.Commands.CreateReview
{
    /// <summary>
    /// Handler for creating a new review in the system.
    /// Validates business rules and creates the review if all conditions are met.
    /// </summary>
    public class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, ApiResponse<ReviewDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateReviewCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Handles the review creation request by validating business rules and persisting the review.
        /// </summary>
        /// <param name="request">The create review command containing review details</param>
        /// <param name="cancellationToken">Cancellation token for async operations</param>
        /// <returns>ApiResponse containing the created review details or error message</returns>
        public async Task<ApiResponse<ReviewDto>> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
        {
            Log.Information("Starting {HandlerName} with request {@Request}", nameof(CreateReviewCommandHandler), request);

            try
            {
                // Validate that the user exists
                var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
                if (user is null || user.IsDeleted)
                {
                    Log.Warning("User not found or deleted: {UserId}", request.UserId);
                    throw new NotFoundException("User", request.UserId);
                }

                // Validate that the hotel exists
                var hotel = await _unitOfWork.Hotels.GetByIdAsync(request.CreateReviewDto.HotelId, cancellationToken);
                if (hotel is null || hotel.IsDeleted)
                {
                    Log.Warning("Hotel not found or deleted: {HotelId}", request.CreateReviewDto.HotelId);
                    throw new NotFoundException("Hotel", request.CreateReviewDto.HotelId);
                }

                // Validate rating is between 1-5 (also enforced by DB constraint)
                if (request.CreateReviewDto.Rating < 1 || request.CreateReviewDto.Rating > 5)
                {
                    Log.Warning("Invalid rating provided: {Rating}", request.CreateReviewDto.Rating);
                    throw new BadRequestException("Rating must be between 1 and 5.");
                }

                // Map DTO to entity and set values
                var review = _mapper.Map<Review>(request.CreateReviewDto);
                review.UserId = request.UserId;
                review.CreatedAt = DateTime.UtcNow;
                review.UpdatedAt = DateTime.UtcNow;

                // Add review to repository and save changes
                await _unitOfWork.Reviews.AddAsync(review, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Get the review with includes for mapping
                var createdReview = await _unitOfWork.Reviews.GetByIdAsync(
                    review.Id,
                    cancellationToken,
                    r => r.User,
                    r => r.Hotel
                );

                // Map entity back to DTO for response
                var reviewDto = _mapper.Map<ReviewDto>(createdReview);

                Log.Information("Review created successfully with ID {ReviewId} for user {UserId} on hotel {HotelId}",
                    review.Id, review.UserId, review.HotelId);
                Log.Information("Completed {HandlerName} successfully", nameof(CreateReviewCommandHandler));

                return ApiResponse<ReviewDto>.SuccessResponse(reviewDto, "Review created successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while processing {HandlerName}", nameof(CreateReviewCommandHandler));
                throw;
            }
        }
    }
}
