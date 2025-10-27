using AutoMapper;
using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Domain.Entities;
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

        /// <summary>
        /// Handles the booking update request by validating business rules and persisting changes.
        /// </summary>
        /// <param name="request">The update booking command containing booking ID and update details</param>
        /// <param name="cancellationToken">Cancellation token for async operations</param>
        /// <returns>ApiResponse containing the updated booking details or error message</returns>
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

                if (booking == null || booking.IsDeleted)
                {
                    Log.Warning("Booking not found or deleted: {BookingId}", request.Id);
                    return ApiResponse<BookingDto>.ErrorResponse($"Booking with ID {request.Id} not found or is deleted.");
                }

                // Block updates if booking is cancelled or completed
                if (booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Completed)
                {
                    Log.Warning("Cannot update cancelled or completed booking: {BookingId}, Status: {Status}", request.Id, booking.Status);
                    return ApiResponse<BookingDto>.ErrorResponse($"Cannot update booking with status '{booking.Status}'.");
                }

                var dto = request.UpdateBookingDto;
                bool datesChanged = false;

                // Apply partial updates - only update fields that are provided (not null)
                if (dto.CheckInDate.HasValue)
                {
                    if (dto.CheckInDate.Value != booking.CheckInDate)
                    {
                        booking.CheckInDate = dto.CheckInDate.Value;
                        datesChanged = true;
                    }
                }

                if (dto.CheckOutDate.HasValue)
                {
                    if (dto.CheckOutDate.Value != booking.CheckOutDate)
                    {
                        booking.CheckOutDate = dto.CheckOutDate.Value;
                        datesChanged = true;
                    }
                }

                if (dto.Status.HasValue)
                {
                    booking.Status = dto.Status.Value;
                }

                // If dates changed, validate and recalculate price
                if (datesChanged)
                {
                    // Validate new date range
                    if (booking.CheckInDate >= booking.CheckOutDate)
                    {
                        Log.Warning("Invalid date range in update: CheckIn {CheckInDate} >= CheckOut {CheckOutDate}", 
                            booking.CheckInDate, booking.CheckOutDate);
                        return ApiResponse<BookingDto>.ErrorResponse("Check-out date must be after check-in date.");
                    }

                    // Check for conflicting bookings (excluding current booking)
                    var conflictingBookings = await _unitOfWork.Bookings.FindAsync(b =>
                        b.RoomId == booking.RoomId &&
                        b.Id != booking.Id &&
                        !b.IsDeleted &&
                        b.Status != BookingStatus.Cancelled &&
                        b.CheckInDate < booking.CheckOutDate &&
                        b.CheckOutDate > booking.CheckInDate
                    );

                    if (conflictingBookings.Any())
                    {
                        Log.Warning("Room has conflicting bookings for new dates: {RoomId}, BookingId: {BookingId}", 
                            booking.RoomId, request.Id);
                        return ApiResponse<BookingDto>.ErrorResponse("Room is not available for the new date range.");
                    }

                    // Recalculate total price
                    var days = (booking.CheckOutDate - booking.CheckInDate).Days;
                    booking.TotalPrice = days * booking.Room!.Price;
                }

                // Update timestamp
                booking.UpdatedAt = DateTime.UtcNow;

                // Save changes
                await _unitOfWork.Bookings.UpdateAsync(booking);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Map entity back to DTO for response
                var bookingDto = _mapper.Map<BookingDto>(booking);
                bookingDto.UserName = $"{booking.User!.FirstName} {booking.User.LastName}";
                bookingDto.RoomNumber = booking.Room!.RoomNumber;
                bookingDto.HotelName = booking.Room.Hotel!.Name;

                Log.Information("Booking updated successfully with ID {BookingId}", booking.Id);
                Log.Information("Completed {HandlerName} successfully", nameof(UpdateBookingCommandHandler));

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
