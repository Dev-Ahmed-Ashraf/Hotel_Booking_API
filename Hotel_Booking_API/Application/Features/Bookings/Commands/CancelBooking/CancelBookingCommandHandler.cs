using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.Common.Exceptions;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Domain.Entities;
using Hotel_Booking_API.Domain.Enums;
using Hotel_Booking_API.Domain.Interfaces;
using MediatR;
using Serilog;

namespace Hotel_Booking_API.Application.Features.Bookings.Commands.CancelBooking
{
    /// <summary>
    /// Handler for cancelling an existing booking in the system.
    /// Validates business rules and cancels the booking if all conditions are met.
    /// </summary>
    public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, ApiResponse<string>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CancelBookingCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Handles the booking cancellation request by validating business rules and cancelling the booking.
        /// </summary>
        /// <param name="request">The cancel booking command containing booking ID and cancellation details</param>
        /// <param name="cancellationToken">Cancellation token for async operations</param>
        /// <returns>ApiResponse containing success message or error message</returns>
        public async Task<ApiResponse<string>> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
        {
            Log.Information("Starting {HandlerName} with request {@Request}", nameof(CancelBookingCommandHandler), request);

            try
            {
                // Get the existing booking with room information
                var booking = await _unitOfWork.Bookings.GetByIdAsync(
                    request.Id,
                    cancellationToken,
                    b => b.Room
                );

                if (booking == null)
                {
                    Log.Warning("Booking not found: {BookingId}", request.Id);
                    throw new NotFoundException("Booking", request.Id);
                }

                if (booking.IsDeleted)
                {
                    Log.Warning("Booking already deleted: {BookingId}", request.Id);
                    throw new BadRequestException($"Booking with ID {request.Id} is already deleted.");
                }

                // Block cancellation if already cancelled or completed
                if (booking.Status == BookingStatus.Cancelled)
                {
                    Log.Warning("Booking already cancelled: {BookingId}", request.Id);
                    throw new BadRequestException($"Booking with ID {request.Id} is already cancelled.");
                }

                if (booking.Status == BookingStatus.Completed)
                {
                    Log.Warning("Cannot cancel completed booking: {BookingId}", request.Id);
                    throw new BadRequestException($"Cannot cancel booking with status '{BookingStatus.Completed}'.");
                }

                // Update booking status to Cancelled
                booking.Status = BookingStatus.Cancelled;
                booking.UpdatedAt = DateTime.UtcNow;

                // Save changes
                await _unitOfWork.Bookings.UpdateAsync(booking);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var message = $"Booking {request.Id} cancelled successfully.";
                if (!string.IsNullOrWhiteSpace(request.CancelBookingDto.Reason))
                {
                    message += $" Reason: {request.CancelBookingDto.Reason}";
                }

                Log.Information("Booking cancelled successfully with ID {BookingId}", booking.Id);
                Log.Information("Completed {HandlerName} successfully", nameof(CancelBookingCommandHandler));

                return ApiResponse<string>.SuccessResponse(message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while processing {HandlerName}", nameof(CancelBookingCommandHandler));
                throw;
            }
        }
    }
}
