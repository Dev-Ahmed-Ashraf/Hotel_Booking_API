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

namespace Hotel_Booking_API.Application.Features.Reviews.Queries.GetReviewsByUser
{
    /// <summary>
    /// Handler for retrieving all reviews for a specific user.
    /// Returns paginated list of reviews with related information.
    /// </summary>
    public class GetReviewsByUserQueryHandler : IRequestHandler<GetReviewsByUserQuery, ApiResponse<PagedList<ReviewDto>>>
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetReviewsByUserQueryHandler(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ApiResponse<PagedList<ReviewDto>>> Handle(GetReviewsByUserQuery request, CancellationToken cancellationToken)
        {
            Log.Information("Starting {HandlerName} with request {@Request}", nameof(GetReviewsByUserQueryHandler), request);

            try
            {
                // Validate user exists
                var userExists = await _context.Users
                    .AsNoTracking()
                    .AnyAsync(u => u.Id == request.UserId && !u.IsDeleted, cancellationToken);

                if (!userExists)
                {
                    Log.Warning("User not found or deleted: {UserId}", request.UserId);
                    throw new NotFoundException("User", request.UserId);
                }

                // Query only reviews for this user
                var query = _context.Reviews
                    .AsNoTracking()
                    .Where(r => r.UserId == request.UserId && !r.IsDeleted);

                // Get total count for pagination
                var totalCount = await query.CountAsync(cancellationToken);

                // Order + projection BEFORE pagination
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

                Log.Information("Reviews retrieved successfully for user {UserId}: {TotalCount} reviews found for page {PageNumber}",
                    request.UserId, totalCount, request.Pagination.PageNumber);

                return ApiResponse<PagedList<ReviewDto>>.SuccessResponse(pagedList, "Reviews retrieved successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while processing {HandlerName}", nameof(GetReviewsByUserQueryHandler));
                throw;
            }
        }
    }
}
