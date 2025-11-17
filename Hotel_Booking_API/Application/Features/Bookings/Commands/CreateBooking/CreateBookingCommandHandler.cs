using AutoMapper;
using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.Common.Exceptions;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Domain.Entities;
using Hotel_Booking_API.Domain.Enums;
using Hotel_Booking_API.Domain.Interfaces;
using MediatR;
using Serilog;
using System.ComponentModel.DataAnnotations;

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
                    excludeBookingId: null,
                    cancellationToken
                );

                if (!isAvailable)
                {
                    Log.Warning("Room not available for booking: {RoomId}", room.Id);
                    throw new ConflictException($"Room {room.RoomNumber} is not available for the selected dates.");
                }

                // Calculate total price
                int days = (int)(request.CreateBookingDto.CheckOutDate.Date - request.CreateBookingDto.CheckInDate.Date).TotalDays;
                if (days <= 0)
                    throw new ValidationException("Check-out must be at least 1 day after check-in.");
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
