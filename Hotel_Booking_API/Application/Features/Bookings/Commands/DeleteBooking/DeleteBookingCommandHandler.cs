using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.Common.Exceptions;
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

                if (booking is null || booking.IsDeleted)
                    throw new NotFoundException("Booking", request.Id);

                // Prevent deleting completed bookings (hard delete)
                if (!request.IsSoft && booking.Status == BookingStatus.Completed)
                {
                    throw new BadRequestException("Completed bookings cannot be permanently deleted.");
                }

                // 3) Prevent soft deleting active bookings unless forced
                if (booking.Status != BookingStatus.Cancelled &&
                    booking.Status != BookingStatus.Completed &&
                    !request.ForceDelete)
                {
                    throw new BadRequestException("Cannot delete an active booking unless force delete is enabled.");
                }

                // Perform deletion based on request
                if (request.IsSoft)
                {
                    // Soft delete: mark as deleted but keep the record
                    booking.IsDeleted = true;
                    booking.Status = BookingStatus.Cancelled;
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
