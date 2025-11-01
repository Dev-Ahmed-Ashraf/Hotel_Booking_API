using AutoMapper;
using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Domain.Entities;
using Hotel_Booking_API.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Hotel_Booking_API.Application.Features.Rooms.Queries.GetRooms
{
    /// <summary>
    /// Handler for retrieving a paginated list of rooms with optional filtering.
    /// Implements complex filtering logic and returns rooms with hotel information.
    /// </summary>
    public class GetRoomsQueryHandler : IRequestHandler<GetRoomsQuery, ApiResponse<PagedList<RoomDto>>>
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetRoomsQueryHandler(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Handles the get rooms query by applying filters and pagination.
        /// All validation is handled by the GetRoomsValidator through the MediatR pipeline.
        /// </summary>
        /// <param name="request">The query containing pagination and search parameters</param>
        /// <param name="cancellationToken">Cancellation token for async operations</param>
        /// <returns>ApiResponse containing paginated list of rooms or error message</returns>
        public async Task<ApiResponse<PagedList<RoomDto>>> Handle(GetRoomsQuery request, CancellationToken cancellationToken)
        {
            Log.Information("Starting {HandlerName} with request {@Request}", nameof(GetRoomsQueryHandler), request);

            try
            {
                // Start with base query including hotel information
                IQueryable<Room> query = _context.Rooms
                .IgnoreQueryFilters()
                .Include(r => r.Hotel)
                .AsNoTracking()
                .AsQueryable();

            // Filter out soft-deleted rooms unless explicitly requested
            if (!request.IncludeDeleted)
                query = query.Where(r => !r.IsDeleted);

            // Apply search filters if provided
            if (request.Search is not null)
            {
                var search = request.Search;

                // Filter by hotel ID
                if (search.HotelId.HasValue)
                    query = query.Where(r => r.HotelId == search.HotelId.Value);

                // Filter by room type
                if (search.Type.HasValue)
                    query = query.Where(r => r.Type == search.Type.Value);

                // Filter by availability
                //if (search.IsAvailable.HasValue)
                //    query = query.Where(r => r.IsAvailable == search.IsAvailable.Value);

                // Filter by price range
                if (search.MinPrice.HasValue)
                    query = query.Where(r => r.Price >= search.MinPrice.Value);

                if (search.MaxPrice.HasValue)
                    query = query.Where(r => r.Price <= search.MaxPrice.Value);

                // Filter by capacity
                if (search.Capacity.HasValue)
                    query = query.Where(r => r.Capacity >= search.Capacity.Value);

                // Filter by hotel name (case-insensitive partial match)
                if (!string.IsNullOrWhiteSpace(search.HotelName))
                    query = query.Where(r => r.Hotel.Name.Contains(search.HotelName));

                // Filter by room number (case-insensitive partial match)
                if (!string.IsNullOrWhiteSpace(search.RoomNumber))
                    query = query.Where(r => r.RoomNumber.Contains(search.RoomNumber));
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination and ordering
            var rooms = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((request.Pagination.PageNumber - 1) * request.Pagination.PageSize)
                .Take(request.Pagination.PageSize)
                .Select(r => new RoomDto
                {
                    Id = r.Id,
                    HotelId = r.HotelId,
                    HotelName = r.Hotel.Name,
                    RoomNumber = r.RoomNumber,
                    Type = r.Type,
                    Price = r.Price,
                    //IsAvailable = r.IsAvailable,
                    Capacity = r.Capacity,
                    Description = r.Description,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync(cancellationToken);

            // Create paginated result
            var pagedList = new PagedList<RoomDto>(
                rooms,
                request.Pagination.PageNumber,
                request.Pagination.PageSize,
                totalCount
            );

                Log.Information("Rooms retrieved successfully: {TotalCount} rooms found for page {PageNumber}", totalCount, request.Pagination.PageNumber);
                Log.Information("Completed {HandlerName} successfully", nameof(GetRoomsQueryHandler));

                return ApiResponse<PagedList<RoomDto>>.SuccessResponse(pagedList, "Rooms retrieved successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while processing {HandlerName}", nameof(GetRoomsQueryHandler));
                throw;
            }
        }
    }
}
