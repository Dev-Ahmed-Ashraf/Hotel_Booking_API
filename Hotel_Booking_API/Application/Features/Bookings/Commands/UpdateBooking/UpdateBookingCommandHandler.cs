using AutoMapper;
using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.Common.Exceptions;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Domain.Enums;
using Hotel_Booking_API.Domain.Interfaces;
using MediatR;
using Serilog;

namespace Hotel_Booking_API.Application.Features.Bookings.Commands.UpdateBooking
{
    /// <summary>
    /// Handler for updating an existing booking in the system.
    /// Validates business rules and updates the booking if all conditions are met.
    /// </summary>
    public class UpdateBookingCommandHandler : IRequestHandler<UpdateBookingCommand, ApiResponse<BookingDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateBookingCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResponse<BookingDto>> Handle(UpdateBookingCommand request, CancellationToken cancellationToken)
        {
            Log.Information("Starting {HandlerName} with request {@Request}", nameof(UpdateBookingCommandHandler), request);

            try
            {
                // Get the existing booking with related entities
                var booking = await _unitOfWork.Bookings.GetByIdAsync(
                    request.Id,
                    cancellationToken,
                    b => b.User,
                    b => b.Room,
                    b => b.Room!.Hotel
                );

                if (booking is null || booking.IsDeleted)
                {
                    Log.Warning("Booking not found or deleted: {BookingId}", request.Id);
                    throw new NotFoundException("Booking", request.Id);
                }

                // Block updates if booking is cancelled or completed
                if (booking.Status is BookingStatus.Cancelled or BookingStatus.Completed)
                {
                    Log.Warning("Cannot update cancelled or completed booking: {BookingId}, Status: {Status}", request.Id, booking.Status);
                    throw new BadRequestException($"Cannot update booking with status '{booking.Status}'.");
                }

                var dto = request.UpdateBookingDto;
                bool datesChanged = false;

                // Apply partial updates - only update fields that are provided (not null)
                if (dto.CheckInDate.HasValue && dto.CheckInDate.Value != booking.CheckInDate)
                {
                    booking.CheckInDate = dto.CheckInDate.Value;
                    datesChanged = true;
                }

                if (dto.CheckOutDate.HasValue && dto.CheckOutDate.Value != booking.CheckOutDate)
                {
                    booking.CheckOutDate = dto.CheckOutDate.Value;
                    datesChanged = true;
                }

                // If dates changed, validate and recalculate price
                if (datesChanged)
                {
                    // Check for conflicting bookings (excluding current booking)
                    bool isAvailable = await _unitOfWork.Rooms.IsRoomAvailableAsync(
                        booking.RoomId,
                        booking.CheckInDate,
                        booking.CheckOutDate,
                        cancellationToken
                );

                    if (!isAvailable)
                    {
                        Log.Warning("Room has conflicting bookings for new dates: {RoomId}, BookingId: {BookingId}",
                            booking.RoomId, request.Id);
                        throw new ConflictException("Room is not available for the new date range.");
                    }

                    // Recalculate total price
                    int days = (int)(booking.CheckOutDate.Date - booking.CheckInDate.Date).TotalDays;
                    booking.TotalPrice = days * booking.Room!.Price;
                }

                // Update timestamp
                booking.UpdatedAt = DateTime.UtcNow;

                // Save changes
                await _unitOfWork.Bookings.UpdateAsync(booking);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Map entity back to DTO for response
                var bookingDto = _mapper.Map<BookingDto>(booking);

                Log.Information("Booking updated successfully with ID {BookingId}", booking.Id);

                return ApiResponse<BookingDto>.SuccessResponse(bookingDto, "Booking updated successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while processing {HandlerName}", nameof(UpdateBookingCommandHandler));
                throw;
            }
        }
    }
}
