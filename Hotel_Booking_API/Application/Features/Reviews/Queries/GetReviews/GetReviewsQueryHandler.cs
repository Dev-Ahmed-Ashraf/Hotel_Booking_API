using AutoMapper;
using AutoMapper.QueryableExtensions;
using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Domain.Entities;
using Hotel_Booking_API.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Hotel_Booking_API.Application.Features.Reviews.Queries.GetReviews
{
    /// <summary>
    /// Handler for retrieving a paginated list of reviews with optional filtering.
    /// Implements complex filtering logic and returns reviews with related information.
    /// </summary>
    public class GetReviewsQueryHandler : IRequestHandler<GetReviewsQuery, ApiResponse<PagedList<ReviewDto>>>
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetReviewsQueryHandler(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ApiResponse<PagedList<ReviewDto>>> Handle(GetReviewsQuery request, CancellationToken cancellationToken)
        {
            Log.Information("Starting {HandlerName} with request {@Request}", nameof(GetReviewsQueryHandler), request);

            try
            {
                // Start with base query including related entities
                IQueryable<Review> query = _context.Reviews
                    .AsNoTracking()
                    .AsQueryable();

                // Filter out soft-deleted reviews unless explicitly requested
                if (request.IncludeDeleted)
                    query = query.IgnoreQueryFilters();

                // Apply search filters if provided
                if (request.Search is not null)
                {
                    var search = request.Search;

                    // Filter by hotel ID
                    if (search.HotelId.HasValue)
                        query = query.Where(r => r.HotelId == search.HotelId.Value);

                    // Filter by user ID
                    if (search.UserId.HasValue)
                        query = query.Where(r => r.UserId == search.UserId.Value);

                    // Filter by minimum rating
                    if (search.MinRating.HasValue)
                        query = query.Where(r => r.Rating >= search.MinRating.Value);

                    // Filter by maximum rating
                    if (search.MaxRating.HasValue)
                        query = query.Where(r => r.Rating <= search.MaxRating.Value);
                }

                // Get total count for pagination
                var totalCount = await query.CountAsync(cancellationToken);

                // Projection BEFORE pagination
                var projected = query
                    .OrderByDescending(r => r.CreatedAt)
                    .ProjectTo<ReviewDto>(_mapper.ConfigurationProvider);

                // Pagination
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

                Log.Information("Reviews retrieved successfully: {TotalCount} reviews found for page {PageNumber}",
                totalCount, request.Pagination.PageNumber);

                return ApiResponse<PagedList<ReviewDto>>.SuccessResponse(pagedList, "Reviews retrieved successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while processing {HandlerName}", nameof(GetReviewsQueryHandler));
                throw;
            }
        }
    }
}
