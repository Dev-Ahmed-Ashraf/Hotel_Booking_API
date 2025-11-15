using AutoMapper;
using AutoMapper.QueryableExtensions;
using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Domain.Entities;
using Hotel_Booking_API.Domain.Enums;
using Hotel_Booking_API.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Hotel_Booking_API.Application.Features.Rooms.Queries.GetAvailableRooms
{
    /// <summary>
    /// Handler for retrieving available rooms for a specific date range.
    /// Implements complex availability logic by checking existing bookings.
    /// </summary>
    public class GetAvailableRoomsQueryHandler : IRequestHandler<GetAvailableRoomsQuery, ApiResponse<List<RoomDto>>>
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetAvailableRoomsQueryHandler(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ApiResponse<List<RoomDto>>> Handle(GetAvailableRoomsQuery request, CancellationToken cancellationToken)
        {
            Log.Information("Starting {HandlerName} with request {@Request}", nameof(GetAvailableRoomsQueryHandler), request);

            try
            {
                // Start with base query for rooms
                IQueryable<Room> query = _context.Rooms
                    .AsNoTracking()
                    .Include(r => r.Hotel)
                    .Include(r => r.Bookings)
                    .Where(r => !r.IsDeleted && !r.Hotel.IsDeleted);

                // Filter by hotel
                if (request.filter!.HotelId.HasValue)
                    query = query.Where(r => r.HotelId == request.filter.HotelId.Value);

                // Filter by type
                if (request.filter.Type.HasValue)
                    query = query.Where(r => r.Type == request.filter.Type.Value);

                // Filter by capacity
                if (request.filter.MinCapacity.HasValue)
                    query = query.Where(r => r.Capacity >= request.filter.MinCapacity.Value);

                // Filter by price
                if (request.filter.MaxPrice.HasValue)
                    query = query.Where(r => r.Price <= request.filter.MaxPrice.Value);

                // ===== Core Availability Logic (Very Efficient SQL) =====
                var availableRooms = await query
                    .Where(room =>
                        !room.Bookings.Any(b =>
                            !b.IsDeleted &&
                            b.Status != BookingStatus.Cancelled &&
                            b.Status != BookingStatus.Completed &&
                            b.CheckInDate < request.filter.CheckOutDate &&
                            b.CheckOutDate > request.filter.CheckInDate
                        )
                    )
                    .ProjectTo<RoomDto>(_mapper.ConfigurationProvider)
                    .OrderBy(r => r.Price)
                    .ToListAsync(cancellationToken);

                Log.Information(
                    "Available rooms retrieved successfully: {Count} rooms found between {CheckIn} and {CheckOut}",
                    availableRooms.Count,
                    request.filter.CheckInDate,
                    request.filter.CheckOutDate
                );
                return ApiResponse<List<RoomDto>>.SuccessResponse(availableRooms, $"Found {availableRooms.Count} available rooms for the specified date range.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while processing {HandlerName}", nameof(GetAvailableRoomsQueryHandler));
                throw;
            }
        }
    }
}
