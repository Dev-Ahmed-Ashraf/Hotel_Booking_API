using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.Common.Exceptions;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Domain.Interfaces;
using MediatR;
using Serilog;

namespace Hotel_Booking_API.Application.Features.Bookings.Queries.CalculateBookingPrice
{
    public class CalculateBookingPriceHandler : IRequestHandler<CalculateBookingPriceQuery, ApiResponse<BookingPriceResponseDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CalculateBookingPriceHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<ApiResponse<BookingPriceResponseDto>> Handle(CalculateBookingPriceQuery request, CancellationToken cancellationToken)
        {
            Log.Information("Starting {HandlerName} with request {@Request}", nameof(CalculateBookingPriceHandler), request);

            try
            {
                // Retrieve room
                var room = await _unitOfWork.Rooms.GetByIdAsync(request.RoomId, cancellationToken);
                if (room is null || room.IsDeleted)
                {
                    Log.Warning("Room not found or deleted: {RoomId}", request.RoomId);
                    throw new NotFoundException(nameof(room), request.RoomId);
                }

                // Calculate nights
                int nights = (request.CheckOutDate - request.CheckInDate).Days;

                // 3?? Calculate total price
                decimal totalPrice = nights * room.Price;

                var result = new BookingPriceResponseDto
                {
                    RoomId = room.Id,
                    RoomNumber = room.RoomNumber,
                    RoomPrice = room.Price,
                    Days = nights,
                    TotalPrice = totalPrice
                };

                Log.Information("Calculated booking price for RoomId {RoomId}: {TotalPrice} for {Nights} nights", room.Id, totalPrice, nights);

                return ApiResponse<BookingPriceResponseDto>.SuccessResponse(result, "Booking price calculated successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in {HandlerName} for RoomId: {RoomId}", nameof(CalculateBookingPriceHandler), request.RoomId);
                throw;
            }
        }
    }
}
