using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.Common.Exceptions;
using Hotel_Booking_API.Domain.Enums;
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

        public async Task<ApiResponse<string>> Handle(DeleteRoomCommand request, CancellationToken cancellationToken)
        {
            Log.Information("Starting {Handler} with request {@Request}", nameof(DeleteRoomCommandHandler), request);

            try
            {
                var room = await _unitOfWork.Rooms.GetByIdAsync(
                    request.Id,
                    cancellationToken,
                    r => r.Bookings
                );

                if (room == null)
                    throw new NotFoundException("Room", request.Id);

                if (room.IsDeleted)
                    throw new BadRequestException($"Room with ID {request.Id} is already deleted.");

                // Check active bookings if not forcing deletion
                if (!request.ForceDelete)
                {
                    var activeBookings = room.Bookings.Where(b =>
                        !b.IsDeleted &&
                        b.Status != BookingStatus.Cancelled &&
                        b.CheckOutDate > DateTime.UtcNow
                    ).ToList();

                    if (activeBookings.Any())
                    {
                        var bookingIds = string.Join(", ", activeBookings.Select(b => b.Id));
                        throw new BadRequestException(
                            $"Cannot delete room '{room.RoomNumber}' because it has active bookings (IDs: {bookingIds})."
                        );
                    }
                }

                if (request.IsSoft)
                {
                    room.IsDeleted = true;
                    room.UpdatedAt = DateTime.UtcNow;

                    // Soft delete all related bookings
                    foreach (var booking in room.Bookings)
                    {
                        booking.IsDeleted = true;
                        booking.UpdatedAt = DateTime.UtcNow;
                    }

                }
                else
                {
                    // Hard delete (remove bookings first if cascade is not enabled)
                    foreach (var booking in room.Bookings)
                        await _unitOfWork.Bookings.DeleteAsync(booking);

                    await _unitOfWork.Rooms.DeleteAsync(room);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var msg = request.IsSoft
                    ? $"Room '{room.RoomNumber}' soft deleted successfully."
                    : $"Room '{room.RoomNumber}' permanently deleted.";

                return ApiResponse<string>.SuccessResponse(msg);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in {Handler}", nameof(DeleteRoomCommandHandler));
                throw;
            }
        }
    }
}
