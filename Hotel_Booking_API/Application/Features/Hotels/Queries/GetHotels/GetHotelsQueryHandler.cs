using AutoMapper;
using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Domain.Entities;
using Hotel_Booking_API.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Hotel_Booking_API.Application.Features.Hotels.Queries.GetHotels
{
    public class GetHotelsQueryHandler : IRequestHandler<GetHotelsQuery, ApiResponse<PagedList<HotelDto>>>
    {
        private readonly ApplicationDbContext _context;
        public GetHotelsQueryHandler(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
        }

        /// <summary>
        /// Handles the get hotels query by applying filters and pagination.
        /// All validation is handled by the GetHotelsValidator through the MediatR pipeline.
        /// </summary>
        /// <param name="request">The query containing pagination and search parameters</param>
        /// <param name="cancellationToken">Cancellation token for async operations</param>
        /// <returns>ApiResponse containing paginated list of hotels or error message</returns>
        public async Task<ApiResponse<PagedList<HotelDto>>> Handle(GetHotelsQuery request, CancellationToken cancellationToken)
        {
            Log.Information("Starting {HandlerName} with request {@Request}", nameof(GetHotelsQueryHandler), request);

            try
            {
                IQueryable<Hotel> query = _context.Hotels
                .IgnoreQueryFilters()
                .Include(h => h.Rooms)
                .AsNoTracking() 
                .AsQueryable();

            // Filter out soft-deleted hotels unless explicitly requested
            if (!request.IncludeDeleted)
                query = query.Where(h => !h.IsDeleted);

            // Apply search filters if provided
            if (request.Search is not null)
            {
                var search = request.Search;

                // Filter by city
                if (!string.IsNullOrWhiteSpace(search.City))
                    query = query.Where(h => h.City.Contains(search.City));

                // Filter by country
                if (!string.IsNullOrWhiteSpace(search.Country))
                    query = query.Where(h => h.Country.Contains(search.Country));

                // Filter by rating range
                if (search.MinRating.HasValue)
                    query = query.Where(h => h.Rating >= search.MinRating.Value);

                if (search.MaxRating.HasValue)
                    query = query.Where(h => h.Rating <= search.MaxRating.Value);
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination and ordering
            var hotels = await query
                .OrderByDescending(h => h.CreatedAt) 
                .Skip((request.Pagination.PageNumber - 1) * request.Pagination.PageSize)
                .Take(request.Pagination.PageSize)
                .Select(h => new HotelDto
                {
                    Id = h.Id,
                    Name = h.Name,
                    Description = h.Description,
                    Address = h.Address,
                    City = h.City,
                    Country = h.Country,
                    Rating = h.Rating,
                    CreatedAt = h.CreatedAt,
                    TotalRooms = h.Rooms.Count(r => !r.IsDeleted),
                    AvailableRooms = h.Rooms.Count(r => !r.IsDeleted && r.IsAvailable),
                    IsDeleted = h.IsDeleted
                })
                .ToListAsync(cancellationToken);

            // Create paginated result
            var pagedList = new PagedList<HotelDto>(
                hotels,
                request.Pagination.PageNumber,
                request.Pagination.PageSize,
                totalCount
            );

                Log.Information("Hotels retrieved successfully: {TotalCount} hotels found for page {PageNumber}", totalCount, request.Pagination.PageNumber);
                Log.Information("Completed {HandlerName} successfully", nameof(GetHotelsQueryHandler));

                return ApiResponse<PagedList<HotelDto>>.SuccessResponse(pagedList, "Hotels retrieved successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while processing {HandlerName}", nameof(GetHotelsQueryHandler));
                throw;
            }
        }

    }
}
