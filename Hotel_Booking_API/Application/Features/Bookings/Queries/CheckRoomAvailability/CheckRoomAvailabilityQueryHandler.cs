using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.Common.Exceptions;
using Hotel_Booking_API.Application.Features.Bookings.Queries.CheckRoomAvailability;
using Hotel_Booking_API.Domain.Entities;
using Hotel_Booking_API.Domain.Enums;
using Hotel_Booking_API.Domain.Interfaces;
using MediatR;
using Serilog;

namespace Hotel_Booking.Application.Features.Bookings.Queries.CheckRoomAvailability
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
            Log.Information("Starting {HandlerName} with request {@Request}", nameof(CheckRoomAvailabilityQueryHandler), request);

            try
            {
                // التحقق من وجود الغرفة
                var room = await _unitOfWork.Rooms.GetByIdAsync(request.RoomId, cancellationToken);

                if (room is null || room.IsDeleted)
                {
                    Log.Warning("Room not found or deleted: {RoomId}", request.RoomId);
                    throw new NotFoundException(nameof(Room), request.RoomId);
                }

                // البحث عن الحجوزات المتداخلة
                var overlappingBookings = await _unitOfWork.Bookings.FindAsync(
                    b => b.RoomId == request.RoomId &&
                         !b.IsDeleted &&
                         b.Status != BookingStatus.Cancelled &&
                         (request.ExcludeBookingId == null || b.Id != request.ExcludeBookingId) &&
                         (
                             (request.CheckInDate >= b.CheckInDate && request.CheckInDate < b.CheckOutDate) ||   // بداية متداخلة
                             (request.CheckOutDate > b.CheckInDate && request.CheckOutDate <= b.CheckOutDate) || // نهاية متداخلة
                             (request.CheckInDate <= b.CheckInDate && request.CheckOutDate >= b.CheckOutDate)    // يغطي المدى كله
                         )              
                );

                bool isAvailable = !overlappingBookings.Any();

                Log.Information("Room {RoomId} availability between {CheckInDate} and {CheckOutDate}: {IsAvailable}",
                    request.RoomId, request.CheckInDate, request.CheckOutDate, isAvailable);

                // إرجاع النتيجة
                return ApiResponse<bool>.SuccessResponse(
                    isAvailable,
                    isAvailable ? "Room is available for booking." : "Room is not available for the selected dates."
                );
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while processing {HandlerName} for RoomId: {RoomId}",
                    nameof(CheckRoomAvailabilityQueryHandler), request.RoomId);
                throw;
            }
        
        }
    }
}
