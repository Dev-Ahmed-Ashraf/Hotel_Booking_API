using AutoMapper;
using AutoMapper.QueryableExtensions;
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

        public async Task<ApiResponse<PagedList<RoomDto>>> Handle(GetRoomsQuery request, CancellationToken cancellationToken)
        {
            Log.Information("Starting {Handler} with request {@Request}", nameof(GetRoomsQueryHandler), request);

            try
            {
                IQueryable<Room> query = _context.Rooms
                    .IgnoreQueryFilters()                  .AsNoTracking()
                    .Include(r => r.Hotel)
                    .AsQueryable();

                // Exclude soft deleted rooms unless explicitly included
                if (!request.IncludeDeleted)
                    query = query.Where(r => !r.IsDeleted);

                // Exclude rooms of deleted hotels
                query = query.Where(r => r.Hotel != null && !r.Hotel.IsDeleted);

                // Apply filters
                if (request.Search is not null)
                {
                    var s = request.Search;

                    if (s.HotelId.HasValue)
                        query = query.Where(r => r.HotelId == s.HotelId.Value);

                    if (s.Type.HasValue)
                        query = query.Where(r => r.Type == s.Type.Value);

                    if (s.MinPrice.HasValue)
                        query = query.Where(r => r.Price >= s.MinPrice.Value);

                    if (s.MaxPrice.HasValue)
                        query = query.Where(r => r.Price <= s.MaxPrice.Value);

                    if (s.Capacity.HasValue)
                        query = query.Where(r => r.Capacity >= s.Capacity.Value);

                    if (!string.IsNullOrWhiteSpace(s.HotelName))
                        query = query.Where(r =>
                            EF.Functions.Like(r.Hotel.Name, $"%{s.HotelName}%"));

                    if (!string.IsNullOrWhiteSpace(s.RoomNumber))
                        query = query.Where(r =>
                            EF.Functions.Like(r.RoomNumber, $"%{s.RoomNumber}%"));
                }

                var totalCount = await query.CountAsync(cancellationToken);

                var rooms = await query
                    .OrderByDescending(r => r.Id)
                    .Skip((request.Pagination.PageNumber - 1) * request.Pagination.PageSize)
                    .Take(request.Pagination.PageSize)
                    .ProjectTo<RoomDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                var pagedList = new PagedList<RoomDto>(
                    rooms,
                    request.Pagination.PageNumber,
                    request.Pagination.PageSize,
                    totalCount
                    );

                Log.Information("Rooms retrieved successfully: {TotalCount} Rooms found for page {PageNumber}", totalCount, request.Pagination.PageNumber);

                return ApiResponse<PagedList<RoomDto>>.SuccessResponse(pagedList, "Rooms retrieved successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in {Handler}", nameof(GetRoomsQueryHandler));
                throw;
            }
        }
    }
}
