using AutoMapper;
using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.Common.Exceptions;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Domain.Enums;
using Hotel_Booking_API.Domain.Interfaces;
using MediatR;
using Serilog;
using static System.Net.Mime.MediaTypeNames;

namespace Hotel_Booking_API.Application.Features.Bookings.Commands.ChangeBookingStatus
{
    /// <summary>
    /// Handler for changing the status of an existing booking in the system.
    /// Validates status transitions and updates the booking status.
    /// </summary>
    public class ChangeBookingStatusCommandHandler : IRequestHandler<ChangeBookingStatusCommand, ApiResponse<BookingDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ChangeBookingStatusCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResponse<BookingDto>> Handle(ChangeBookingStatusCommand request, CancellationToken cancellationToken)
        {
            Log.Information("Starting {HandlerName} with request {@Request}", nameof(ChangeBookingStatusCommandHandler), request);

            try
            {
                // Get the existing booking with related entities
                var booking = await _unitOfWork.Bookings.GetByIdAsync(
                    request.Id,
                    cancellationToken,
                    b => b.User,
                    b => b.Room,
                    b => b.Room!.Hotel,
                    b => b.Payment
                );

                if (booking is null || booking.IsDeleted)
                {
                    throw new NotFoundException("Booking", request.Id);
                }

                var newStatus = request.ChangeBookingStatusDto.Status;
                var currentStatus = booking.Status;

                // Validate status transition
                if (!IsValidStatusTransition(currentStatus, newStatus))
                {
                    Log.Warning("Invalid status transition: {CurrentStatus} to {NewStatus} for booking {BookingId}",
                        currentStatus, newStatus, request.Id);
                    throw new BadRequestException($"Invalid status transition from '{currentStatus}' to '{newStatus}'.");
                }

                #region Additional business rules on transitions
                // Prevent Confirm unless room still available
                if (newStatus == BookingStatus.Confirmed)
                {
                    bool available = await _unitOfWork.Rooms.IsRoomAvailableAsync(
                        booking.RoomId,
                        booking.CheckInDate,
                        booking.CheckOutDate,
                        cancellationToken
                    );

                    if (!available)
                        throw new ConflictException("Room is not available to confirm this booking.");
                }

                // Completed → Cannot complete before check-in
                if (newStatus == BookingStatus.Completed &&
                    DateTime.UtcNow < booking.CheckInDate)
                {
                    throw new BadRequestException("Cannot complete booking before the check-in date.");
                }

                // NoShow → Cannot mark before the stay period has passed
                if (newStatus == BookingStatus.NoShow &&
                    DateTime.UtcNow < booking.CheckOutDate)
                {
                    throw new BadRequestException("Cannot mark booking as NoShow before check-out date.");
                }

                // Cancelled → Save cancellation reason
                if (newStatus == BookingStatus.Cancelled)
                {
                    booking.CancellationReason = request.ChangeBookingStatusDto.Notes;
                }
                #endregion

                // Update booking status
                booking.Status = newStatus;
                booking.UpdatedAt = DateTime.UtcNow;

                // Save changes
                await _unitOfWork.Bookings.UpdateAsync(booking);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Map entity back to DTO for response
                var bookingDto = _mapper.Map<BookingDto>(booking);

                Log.Information("Booking status changed successfully from {CurrentStatus} to {NewStatus} for booking {BookingId}",
                    currentStatus, newStatus, booking.Id);

                return ApiResponse<BookingDto>.SuccessResponse(bookingDto, $"Booking status changed to '{newStatus}' successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while processing {HandlerName}", nameof(ChangeBookingStatusCommandHandler));
                throw;
            }
        }

        /// <summary>
        /// Validates if the status transition is allowed based on business rules.
        /// </summary>
        /// <param name="currentStatus">Current booking status</param>
        /// <param name="newStatus">New booking status</param>
        /// <returns>True if transition is valid, false otherwise</returns>
        private static bool IsValidStatusTransition(BookingStatus currentStatus, BookingStatus newStatus)
        {
            return currentStatus switch
            {
                BookingStatus.Pending =>
                newStatus is BookingStatus.Confirmed or BookingStatus.Cancelled,
                BookingStatus.Confirmed =>
                newStatus is BookingStatus.Completed or BookingStatus.Cancelled or BookingStatus.NoShow,

                BookingStatus.Cancelled => false, // Cannot change from cancelled
                BookingStatus.Completed => false, // Cannot change from completed
                BookingStatus.NoShow => false, // Cannot change from no-show

                _ => false
            };
        }
    }
}
