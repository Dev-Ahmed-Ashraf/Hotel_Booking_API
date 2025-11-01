using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.Common.Exceptions;
using Hotel_Booking_API.Domain.Entities;
using Hotel_Booking_API.Domain.Enums;
using Hotel_Booking_API.Domain.Interfaces;
using MediatR;
using Serilog;

namespace Hotel_Booking_API.Application.Features.Bookings.Commands.DeleteBooking
{
    /// <summary>
    /// Handler for deleting an existing booking from the system.
    /// Validates business rules and performs soft or hard delete based on request.
    /// </summary>
    public class DeleteBookingCommandHandler : IRequestHandler<DeleteBookingCommand, ApiResponse<string>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteBookingCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Handles the booking deletion request by validating business rules and removing the booking.
        /// </summary>
        /// <param name="request">The delete booking command containing booking ID and deletion options</param>
        /// <param name="cancellationToken">Cancellation token for async operations</param>
        /// <returns>ApiResponse containing success message or error message</returns>
        public async Task<ApiResponse<string>> Handle(DeleteBookingCommand request, CancellationToken cancellationToken)
        {
            Log.Information("Starting {HandlerName} with request {@Request}", nameof(DeleteBookingCommandHandler), request);

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

                // Check if booking has active status (not cancelled or completed)
                var hasActiveStatus = booking.Status != BookingStatus.Cancelled && booking.Status != BookingStatus.Completed;

                // If not forcing deletion and booking is active, restore room availability
                if (!request.ForceDelete && hasActiveStatus && booking.Room != null)
                {
                    Log.Warning("Cannot delete active booking without force option: {BookingId}", request.Id);
                    throw new BadRequestException("Cannot delete an active booking unless force delete is enabled.");
                }

                // Perform deletion based on request
                if (request.IsSoft)
                {
                    // Soft delete: mark as deleted but keep the record
                    booking.IsDeleted = true;
                    booking.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.Bookings.UpdateAsync(booking);
                    Log.Information("Booking soft deleted successfully with ID {BookingId}", booking.Id);
                }
                else
                {
                    // Hard delete: permanently remove the record
                    await _unitOfWork.Bookings.DeleteAsync(booking);
                    Log.Information("Booking permanently deleted with ID {BookingId}", booking.Id);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var message = request.IsSoft
                    ? $"Booking {request.Id} soft deleted successfully."
                    : $"Booking {request.Id} permanently deleted.";

                Log.Information("Completed {HandlerName} successfully", nameof(DeleteBookingCommandHandler));

                return ApiResponse<string>.SuccessResponse(message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while processing {HandlerName}", nameof(DeleteBookingCommandHandler));
                throw;
            }
        }
    }
}
