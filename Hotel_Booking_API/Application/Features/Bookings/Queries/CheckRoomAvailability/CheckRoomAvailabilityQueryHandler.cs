using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.Common.Exceptions;
using Hotel_Booking_API.Domain.Entities;
using Hotel_Booking_API.Domain.Enums;
using Hotel_Booking_API.Domain.Interfaces;
using MediatR;
using Serilog;

namespace Hotel_Booking_API.Application.Features.Bookings.Queries.CheckRoomAvailability
{
    public class CheckRoomAvailabilityQueryHandler : IRequestHandler<CheckRoomAvailabilityQuery, ApiResponse<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CheckRoomAvailabilityQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<ApiResponse<bool>> Handle(CheckRoomAvailabilityQuery request, CancellationToken cancellationToken)
        {
            Log.Information("Starting {HandlerName} with request {@Request}",
                nameof(CheckRoomAvailabilityQueryHandler), request);

            try
            {
                // Validate room exists
                var room = await _unitOfWork.Rooms.GetByIdAsync(
                    request.RoomId,
                    cancellationToken
                );

                if (room is null || room.IsDeleted)
                {
                    Log.Warning("Room not found or deleted: {RoomId}", request.RoomId);
                    throw new NotFoundException(nameof(Room), request.RoomId);
                }

                // Call your repository function
                var isAvailable = await _unitOfWork.Rooms.IsRoomAvailableAsync(
                    request.RoomId,
                    request.CheckInDate,
                    request.CheckOutDate,
                    cancellationToken
                );

                Log.Information(
                    "Room availability check: RoomId={RoomId}, Start={Start}, End={End}, Available={Available}",
                    request.RoomId, request.CheckInDate, request.CheckOutDate, isAvailable
                );

                return ApiResponse<bool>.SuccessResponse(
                    isAvailable,
                    isAvailable ? "Room is available for booking." : "Room is not available for the selected dates."
                );
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while processing {HandlerName} for RoomId {RoomId}",
                    nameof(CheckRoomAvailabilityQueryHandler), request.RoomId);
                throw;
            }
        }
    }
}
