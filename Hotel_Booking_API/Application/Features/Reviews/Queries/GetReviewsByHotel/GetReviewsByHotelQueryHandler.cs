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

        public async Task<ApiResponse<PagedList<ReviewDto>>> Handle(GetReviewsByHotelQuery request, CancellationToken cancellationToken)
        {
            Log.Information("Starting {HandlerName} with request {@Request}", nameof(GetReviewsByHotelQueryHandler), request);

            try
            {
                // Check if hotel exists (without loading whole entity)
                var hotelExists = await _context.Hotels
                    .AsNoTracking()
                    .AnyAsync(h => h.Id == request.HotelId && !h.IsDeleted, cancellationToken);

                if (!hotelExists)
                {
                    Log.Warning("Hotel not found or deleted: {HotelId}", request.HotelId);
                    throw new NotFoundException("Hotel", request.HotelId);
                }

                // Base reviews query
                var query = _context.Reviews
                    .AsNoTracking()
                    .Where(r => r.HotelId == request.HotelId && !r.IsDeleted);

                // Get total count for pagination
                var totalCount = await query.CountAsync(cancellationToken);

                // Apply ordering + projection BEFORE pagination
                var projected = query
                    .OrderByDescending(r => r.CreatedAt)
                    .ProjectTo<ReviewDto>(_mapper.ConfigurationProvider);

                // Apply pagination
                var reviews = await projected
                    .Skip((request.Pagination.PageNumber - 1) * request.Pagination.PageSize)
                    .Take(request.Pagination.PageSize)
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
