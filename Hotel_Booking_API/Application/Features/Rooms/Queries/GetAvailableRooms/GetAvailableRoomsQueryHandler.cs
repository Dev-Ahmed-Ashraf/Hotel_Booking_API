using AutoMapper;
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

        /// <summary>
        /// Handles the get available rooms query by checking room availability for the specified date range.
        /// Includes comprehensive validation for hotel existence, date ranges, and business rules.
        /// </summary>
        /// <param name="request">The query containing date range and filter criteria</param>
        /// <param name="cancellationToken">Cancellation token for async operations</param>
        /// <returns>ApiResponse containing list of available rooms or error message</returns>
        public async Task<ApiResponse<List<RoomDto>>> Handle(GetAvailableRoomsQuery request, CancellationToken cancellationToken)
        {
            Log.Information("Starting {HandlerName} with request {@Request}", nameof(GetAvailableRoomsQueryHandler), request);

            try
            {
                // Validate date range
                if (request.CheckInDate >= request.CheckOutDate)
                {
                    Log.Warning("Invalid date range: CheckIn {CheckInDate} >= CheckOut {CheckOutDate}", request.CheckInDate, request.CheckOutDate);
                    return ApiResponse<List<RoomDto>>.ErrorResponse("Check-in date must be before check-out date.");
                }

                if (request.CheckInDate < DateTime.Today)
                {
                    Log.Warning("Check-in date in the past: {CheckInDate}", request.CheckInDate);
                    return ApiResponse<List<RoomDto>>.ErrorResponse("Check-in date cannot be in the past.");
                }

                // Validate date range duration (not too long)
                var duration = request.CheckOutDate - request.CheckInDate;
                if (duration.TotalDays > 30)
                {
                    Log.Warning("Booking duration too long: {DurationDays} days", duration.TotalDays);
                    return ApiResponse<List<RoomDto>>.ErrorResponse("Booking duration cannot exceed 30 days.");
                }

            // Validate hotel existence if hotel ID is provided
            if (request.HotelId.HasValue)
            {
                var hotelExists = await _context.Hotels
                    .IgnoreQueryFilters()
                    .AnyAsync(h => h.Id == request.HotelId.Value && !h.IsDeleted, cancellationToken);
                
                if (!hotelExists)
                {
                    Log.Warning("Hotel not found or deleted: {HotelId}", request.HotelId.Value);
                    return ApiResponse<List<RoomDto>>.ErrorResponse($"Hotel with ID {request.HotelId.Value} not found or is deleted.");
                }
            }

                // Validate room type if provided
                if (request.Type.HasValue && !Enum.IsDefined(typeof(RoomType), request.Type.Value))
                {
                    Log.Warning("Invalid room type specified: {RoomType}", request.Type.Value);
                    return ApiResponse<List<RoomDto>>.ErrorResponse("Invalid room type specified.");
                }

                // Validate capacity if provided
                if (request.MinCapacity.HasValue)
                {
                    if (request.MinCapacity.Value <= 0)
                    {
                        Log.Warning("Invalid minimum capacity: {MinCapacity}", request.MinCapacity.Value);
                        return ApiResponse<List<RoomDto>>.ErrorResponse("Minimum capacity must be greater than 0.");
                    }
                    if (request.MinCapacity.Value > 10)
                    {
                        Log.Warning("Minimum capacity too high: {MinCapacity}", request.MinCapacity.Value);
                        return ApiResponse<List<RoomDto>>.ErrorResponse("Minimum capacity cannot exceed 10 guests.");
                    }
                }

                // Validate price if provided
                if (request.MaxPrice.HasValue)
                {
                    if (request.MaxPrice.Value <= 0)
                    {
                        Log.Warning("Invalid maximum price: {MaxPrice}", request.MaxPrice.Value);
                        return ApiResponse<List<RoomDto>>.ErrorResponse("Maximum price must be greater than 0.");
                    }
                    if (request.MaxPrice.Value > 10000)
                    {
                        Log.Warning("Maximum price too high: {MaxPrice}", request.MaxPrice.Value);
                        return ApiResponse<List<RoomDto>>.ErrorResponse("Maximum price cannot exceed $10,000.");
                    }
                }

            // Start with base query for rooms
            IQueryable<Room> roomsQuery = _context.Rooms
                .Include(r => r.Hotel)
                .Include(r => r.Bookings)
                .AsNoTracking()
                .Where(r => !r.IsDeleted && r.IsAvailable);

            // Apply hotel filter if specified
            if (request.HotelId.HasValue)
                roomsQuery = roomsQuery.Where(r => r.HotelId == request.HotelId.Value);

            // Apply room type filter if specified
            if (request.Type.HasValue)
                roomsQuery = roomsQuery.Where(r => r.Type == request.Type.Value);

            // Apply capacity filter if specified
            if (request.MinCapacity.HasValue)
                roomsQuery = roomsQuery.Where(r => r.Capacity >= request.MinCapacity.Value);

            // Apply price filter if specified
            if (request.MaxPrice.HasValue)
                roomsQuery = roomsQuery.Where(r => r.Price <= request.MaxPrice.Value);

            // Get all rooms matching the criteria
            var allRooms = await roomsQuery.ToListAsync(cancellationToken);

            // Filter out rooms with conflicting bookings
            var availableRooms = new List<Room>();

            foreach (var room in allRooms)
            {
                // Check if room has any active bookings that conflict with the requested dates
                var hasConflict = room.Bookings.Any(booking =>
                    !booking.IsDeleted &&
                    booking.Status != BookingStatus.Cancelled &&
                    booking.CheckInDate < request.CheckOutDate &&
                    booking.CheckOutDate > request.CheckInDate
                );

                // If no conflicts, room is available
                if (!hasConflict)
                {
                    availableRooms.Add(room);
                }
            }

            // Map entities to DTOs
            var roomDtos = availableRooms.Select(room => new RoomDto
            {
                Id = room.Id,
                HotelId = room.HotelId,
                HotelName = room.Hotel.Name,
                RoomNumber = room.RoomNumber,
                Type = room.Type,
                Price = room.Price,
                IsAvailable = room.IsAvailable,
                Capacity = room.Capacity,
                Description = room.Description,
                CreatedAt = room.CreatedAt
            }).ToList();

                Log.Information("Available rooms retrieved successfully: {RoomCount} rooms found for date range {CheckInDate} to {CheckOutDate}", roomDtos.Count, request.CheckInDate, request.CheckOutDate);
                Log.Information("Completed {HandlerName} successfully", nameof(GetAvailableRoomsQueryHandler));

                return ApiResponse<List<RoomDto>>.SuccessResponse(roomDtos, $"Found {roomDtos.Count} available rooms for the specified date range.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while processing {HandlerName}", nameof(GetAvailableRoomsQueryHandler));
                throw;
            }
        }
    }
}
