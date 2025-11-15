using AutoMapper;
using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.Common.Exceptions;
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
                if (user is null || user.IsDeleted)
                {
                    Log.Warning("User not found or deleted: {UserId}", request.UserId);
                    throw new NotFoundException("User", request.UserId);
                }

                // Validate that the room exists and is not deleted
                var room = await _unitOfWork.Rooms.GetByIdAsync(
                    request.CreateBookingDto.RoomId,
                    cancellationToken,
                    r => r.Hotel
                );

                if (room is null || room.IsDeleted)
                {
                    Log.Warning("Room not found or deleted: {RoomId}", request.CreateBookingDto.RoomId);
                    throw new NotFoundException("Room", request.CreateBookingDto.RoomId);
                }

                // Check Room Availability (excluding Cancelled & Completed)
                var isAvailable = await _unitOfWork.Rooms.IsRoomAvailableAsync(
                    room.Id,
                    request.CreateBookingDto.CheckInDate,
                    request.CreateBookingDto.CheckOutDate,
                    cancellationToken
                );

                if (!isAvailable)
                {
                    Log.Warning("Room not available for booking: {RoomId}", room.Id);
                    throw new ConflictException($"Room {room.RoomNumber} is not available for the selected dates.");
                }

                // Double-check overlapping active bookings
                var conflictingBookings = await _unitOfWork.Bookings.FindAsync(b =>
                    b.RoomId == room.Id &&
                    !b.IsDeleted &&
                    b.Status != BookingStatus.Cancelled &&
                    b.Status != BookingStatus.Completed &&
                    b.CheckInDate < request.CreateBookingDto.CheckOutDate &&
                    b.CheckOutDate > request.CreateBookingDto.CheckInDate
                );

                if (conflictingBookings.Any())
                {
                    Log.Warning("Room {RoomId} has conflicting bookings between {Start} and {End}",
                    room.Id, request.CreateBookingDto.CheckInDate, request.CreateBookingDto.CheckOutDate);
                    throw new ConflictException("Room is not available for the specified date range.");
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
