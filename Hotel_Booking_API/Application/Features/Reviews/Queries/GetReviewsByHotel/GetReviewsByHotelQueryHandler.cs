using AutoMapper;
using AutoMapper.QueryableExtensions;
using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.Common.Exceptions;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Domain.Entities;
using Hotel_Booking_API.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Hotel_Booking_API.Application.Features.Reviews.Queries.GetReviewsByHotel
{
    /// <summary>
    /// Handler for retrieving all reviews for a specific hotel.
    /// Returns paginated list of reviews with related information.
    /// </summary>
    public class GetReviewsByHotelQueryHandler : IRequestHandler<GetReviewsByHotelQuery, ApiResponse<PagedList<ReviewDto>>>
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetReviewsByHotelQueryHandler(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Handles the get reviews by hotel query by validating hotel exists and retrieving reviews.
        /// </summary>
        /// <param name="request">The query containing hotel ID and pagination parameters</param>
        /// <param name="cancellationToken">Cancellation token for async operations</param>
        /// <returns>ApiResponse containing paginated list of reviews or error message</returns>
        public async Task<ApiResponse<PagedList<ReviewDto>>> Handle(GetReviewsByHotelQuery request, CancellationToken cancellationToken)
        {
            Log.Information("Starting {HandlerName} with request {@Request}", nameof(GetReviewsByHotelQueryHandler), request);

            try
            {
                // Validate that the hotel exists
                var hotel = await _context.Hotels
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(h => h.Id == request.HotelId && !h.IsDeleted, cancellationToken);

                if (hotel == null)
                {
                    Log.Warning("Hotel not found or deleted: {HotelId}", request.HotelId);
                    throw new NotFoundException("Hotel", request.HotelId);
                }

                // Query reviews filtered by hotel ID
                IQueryable<Review> query = _context.Reviews
                    .IgnoreQueryFilters()
                    .Include(r => r.User)
                    .Include(r => r.Hotel)
                    .Where(r => r.HotelId == request.HotelId && !r.IsDeleted)
                    .AsNoTracking()
                    .AsQueryable();

                // Get total count for pagination
                var totalCount = await query.CountAsync(cancellationToken);

                // Apply pagination and ordering
                var reviews = await query
                    .OrderByDescending(r => r.CreatedAt)
                    .Skip((request.Pagination.PageNumber - 1) * request.Pagination.PageSize)
                    .Take(request.Pagination.PageSize)
                    .ProjectTo<ReviewDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                // Create paginated result
                var pagedList = new PagedList<ReviewDto>(
                    reviews,
                    request.Pagination.PageNumber,
                    request.Pagination.PageSize,
                    totalCount
                );

                Log.Information("Reviews retrieved successfully for hotel {HotelId}: {TotalCount} reviews found for page {PageNumber}",
                    request.HotelId, totalCount, request.Pagination.PageNumber);

                return ApiResponse<PagedList<ReviewDto>>.SuccessResponse(pagedList, "Reviews retrieved successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while processing {HandlerName}", nameof(GetReviewsByHotelQueryHandler));
                throw;
            }
        }
    }
}
