using AutoMapper;
using AutoMapper.QueryableExtensions;
using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.Common.Exceptions;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Domain.Entities;
using Hotel_Booking_API.Domain.Enums;
using Hotel_Booking_API.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
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

        public async Task<ApiResponse<ReviewDto>> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
        {
            Log.Information("Starting {HandlerName} with request {@Request}", nameof(CreateReviewCommandHandler), request);

            try
            {
                // ensure user exists
                var userExists = await _unitOfWork.Users
                    .Query()
                    .AnyAsync(u => u.Id == request.UserId && !u.IsDeleted, cancellationToken);

                if (!userExists)
                {
                    Log.Warning("User not found or deleted: {UserId}", request.UserId);
                    throw new NotFoundException("User", request.UserId);
                }

                // ensure hotel exists
                var hotelExists = await _unitOfWork.Hotels
                    .Query()
                    .AnyAsync(h => h.Id == request.CreateReviewDto.HotelId && !h.IsDeleted, cancellationToken);

                if (!hotelExists)
                {
                    Log.Warning("Hotel not found or deleted: {HotelId}", request.CreateReviewDto.HotelId);
                    throw new NotFoundException("Hotel", request.CreateReviewDto.HotelId);
                }

                // Prevent duplicate reviews by same user
                var alreadyReviewed = await _unitOfWork.Reviews
                    .Query()
                    .AnyAsync(r =>
                        r.UserId == request.UserId &&
                        r.HotelId == request.CreateReviewDto.HotelId &&
                        !r.IsDeleted,
                        cancellationToken);

                if (alreadyReviewed)
                {
                    Log.Warning("User {UserId} already reviewed hotel {HotelId}", request.UserId, request.CreateReviewDto.HotelId);
                    throw new BadRequestException("You have already reviewed this hotel.");
                }

                // Ensure user has stayed at the hotel (booking completed)
                var userHasBooking = await _unitOfWork.Bookings
                    .Query()
                    .AnyAsync(b =>
                        b.UserId == request.UserId &&
                        b.Room.HotelId == request.CreateReviewDto.HotelId &&
                        b.Status == BookingStatus.Completed,
                        cancellationToken);

                if (!userHasBooking)
                {
                    Log.Warning("User {UserId} attempted to review hotel {HotelId} without staying", request.UserId, request.CreateReviewDto.HotelId);
                    throw new BadRequestException("You cannot review a hotel unless you have completed a stay.");
                }

                // Map DTO to entity and set values
                var review = _mapper.Map<Review>(request.CreateReviewDto);
                review.UserId = request.UserId;
                review.CreatedAt = DateTime.UtcNow;
                review.UpdatedAt = DateTime.UtcNow;

                // Add review to repository and save changes
                await _unitOfWork.Reviews.AddAsync(review, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Retrieve DTO using ProjectTo
                var reviewDto = await _unitOfWork.Reviews
                    .Query()
                    .Where(r => r.Id == review.Id)
                    .ProjectTo<ReviewDto>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(cancellationToken);

                Log.Information("Review created successfully with ID {ReviewId} for user {UserId} on hotel {HotelId}",
                    review.Id, review.UserId, review.HotelId);

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
