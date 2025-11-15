using AutoMapper;
using AutoMapper.QueryableExtensions;
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
        private readonly IMapper _mapper;

        public GetHotelsQueryHandler(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

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
                    .ProjectTo<HotelDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);
                
                // Create paginated result
                var pagedList = new PagedList<HotelDto>(
                    hotels,
                    request.Pagination.PageNumber,
                    request.Pagination.PageSize,
                    totalCount
                );

                Log.Information("Hotels retrieved successfully: {TotalCount} hotels found for page {PageNumber}", totalCount, request.Pagination.PageNumber);

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
