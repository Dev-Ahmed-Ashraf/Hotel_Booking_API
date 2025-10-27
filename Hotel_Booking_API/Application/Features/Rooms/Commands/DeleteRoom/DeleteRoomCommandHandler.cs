using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Domain.Interfaces;
using MediatR;
using Serilog;

namespace Hotel_Booking_API.Application.Features.Rooms.Commands.DeleteRoom
{
    /// <summary>
    /// Handler for deleting a room from the system.
    /// Validates business rules and performs soft or hard delete based on request.
    /// </summary>
    public class DeleteRoomCommandHandler : IRequestHandler<DeleteRoomCommand, ApiResponse<string>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteRoomCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Handles the room deletion request by validating business rules and removing the room.
        /// </summary>
        /// <param name="request">The delete room command containing room ID and deletion options</param>
        /// <param name="cancellationToken">Cancellation token for async operations</param>
        /// <returns>ApiResponse containing success message or error message</returns>
        public async Task<ApiResponse<string>> Handle(DeleteRoomCommand request, CancellationToken cancellationToken)
        {
            Log.Information("Starting {HandlerName} with request {@Request}", nameof(DeleteRoomCommandHandler), request);

            try
            {
                // Get the room with related bookings to check for active reservations
                var room = await _unitOfWork.Rooms.GetByIdAsync(
                request.Id, 
                cancellationToken, 
                r => r.Bookings
            );

                if (room == null)
                {
                    Log.Warning("Room not found: {RoomId}", request.Id);
                    return ApiResponse<string>.ErrorResponse($"Room with ID {request.Id} not found.");
                }

                if (room.IsDeleted)
                {
                    Log.Warning("Room already deleted: {RoomId}", request.Id);
                    return ApiResponse<string>.ErrorResponse($"Room with ID {request.Id} is already deleted.");
                }

            // Check for active bookings if not forcing deletion
            if (!request.ForceDelete)
            {
                var activeBookings = room.Bookings.Where(b => 
                    !b.IsDeleted && 
                    b.Status != Domain.Enums.BookingStatus.Cancelled &&
                    b.CheckOutDate > DateTime.UtcNow
                ).ToList();

                if (activeBookings.Any())
                {
                    var bookingIds = string.Join(", ", activeBookings.Select(b => b.Id));
                    Log.Warning("Cannot delete room with active bookings: {RoomId}, RoomNumber: {RoomNumber}, BookingIds: {BookingIds}", request.Id, room.RoomNumber, bookingIds);
                    return ApiResponse<string>.ErrorResponse(
                        $"Cannot delete room '{room.RoomNumber}' because it has active bookings (IDs: {bookingIds}). " +
                        "Use ForceDelete=true to override this check, but this may cause data integrity issues.");
                }
            }

                // Perform deletion based on request
                if (request.IsSoft)
                {
                    // Soft delete: mark as deleted but keep the record
                    room.IsDeleted = true;
                    room.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.Rooms.UpdateAsync(room);
                    Log.Information("Room soft deleted successfully with ID {RoomId} and number {RoomNumber}", room.Id, room.RoomNumber);
                }
                else
                {
                    // Hard delete: permanently remove the record
                    await _unitOfWork.Rooms.DeleteAsync(room);
                    Log.Information("Room permanently deleted with ID {RoomId} and number {RoomNumber}", room.Id, room.RoomNumber);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var message = request.IsSoft
                    ? $"Room '{room.RoomNumber}' soft deleted successfully."
                    : $"Room '{room.RoomNumber}' permanently deleted.";

                Log.Information("Completed {HandlerName} successfully", nameof(DeleteRoomCommandHandler));

                return ApiResponse<string>.SuccessResponse(message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while processing {HandlerName}", nameof(DeleteRoomCommandHandler));
                throw;
            }
        }
    }
}
