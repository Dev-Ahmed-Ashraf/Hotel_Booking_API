using AutoMapper;
using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Domain.Entities;
using Hotel_Booking_API.Domain.Enums;
using Hotel_Booking_API.Domain.Interfaces;
using MediatR;
using Serilog;

namespace Hotel_Booking_API.Application.Features.Bookings.Commands.CreateBooking
{
    /// <summary>
    /// Handler for creating a new booking in the system.
    /// Validates business rules and creates the booking if all conditions are met.
    /// </summary>
    public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, ApiResponse<BookingDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateBookingCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Handles the booking creation request by validating business rules and persisting the booking.
        /// </summary>
        /// <param name="request">The create booking command containing booking details</param>
        /// <param name="cancellationToken">Cancellation token for async operations</param>
        /// <returns>ApiResponse containing the created booking details or error message</returns>
        public async Task<ApiResponse<BookingDto>> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
        {
            Log.Information("Starting {HandlerName} with request {@Request}", nameof(CreateBookingCommandHandler), request);

            try
            {
                // Validate that the user exists
                var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
                if (user == null || user.IsDeleted)
                {
                    Log.Warning("User not found or deleted: {UserId}", request.UserId);
                    return ApiResponse<BookingDto>.ErrorResponse($"User with ID {request.UserId} not found or is deleted.");
                }

                // Validate that the room exists and is not deleted
                var room = await _unitOfWork.Rooms.GetByIdAsync(
                    request.CreateBookingDto.RoomId, 
                    cancellationToken,
                    r => r.Hotel
                );
                if (room == null || room.IsDeleted)
                {
                    Log.Warning("Room not found or deleted: {RoomId}", request.CreateBookingDto.RoomId);
                    return ApiResponse<BookingDto>.ErrorResponse($"Room with ID {request.CreateBookingDto.RoomId} not found or is deleted.");
                }

                // Check if room is available
                if (!room.IsAvailable)
                {
                    Log.Warning("Room not available: {RoomId}", request.CreateBookingDto.RoomId);
                    return ApiResponse<BookingDto>.ErrorResponse($"Room {room.RoomNumber} is not available.");
                }

                // Check for conflicting bookings
                var conflictingBookings = await _unitOfWork.Bookings.FindAsync(b =>
                    b.RoomId == request.CreateBookingDto.RoomId &&
                    !b.IsDeleted &&
                    b.Status != BookingStatus.Cancelled &&
                    b.CheckInDate < request.CreateBookingDto.CheckOutDate &&
                    b.CheckOutDate > request.CreateBookingDto.CheckInDate
                );

                if (conflictingBookings.Any())
                {
                    Log.Warning("Room has conflicting bookings: {RoomId}, CheckIn: {CheckInDate}, CheckOut: {CheckOutDate}", 
                        request.CreateBookingDto.RoomId, request.CreateBookingDto.CheckInDate, request.CreateBookingDto.CheckOutDate);
                    return ApiResponse<BookingDto>.ErrorResponse("Room is not available for the specified date range.");
                }

                // Calculate total price
                var days = (request.CreateBookingDto.CheckOutDate - request.CreateBookingDto.CheckInDate).Days;
                var totalPrice = days * room.Price;

                // Map DTO to entity and set values
                var booking = _mapper.Map<Booking>(request.CreateBookingDto);
                booking.UserId = request.UserId;
                booking.TotalPrice = totalPrice;
                booking.Status = BookingStatus.Pending;
                booking.CreatedAt = DateTime.UtcNow;
                booking.UpdatedAt = DateTime.UtcNow;
                booking.IsDeleted = false;

                // Set room as unavailable
                room.IsAvailable = false;
                await _unitOfWork.Rooms.UpdateAsync(room);

                // Add booking to repository and save changes
                await _unitOfWork.Bookings.AddAsync(booking, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Map entity back to DTO for response
                var bookingDto = _mapper.Map<BookingDto>(booking);
                bookingDto.UserName = $"{user.FirstName} {user.LastName}";
                bookingDto.RoomNumber = room.RoomNumber;
                bookingDto.HotelName = room.Hotel.Name;

                Log.Information("Booking created successfully with ID {BookingId} for user {UserId} in room {RoomId}", 
                    booking.Id, booking.UserId, booking.RoomId);
                Log.Information("Completed {HandlerName} successfully", nameof(CreateBookingCommandHandler));

                return ApiResponse<BookingDto>.SuccessResponse(bookingDto, "Booking created successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while processing {HandlerName}", nameof(CreateBookingCommandHandler));
                throw;
            }
        }
    }
}
