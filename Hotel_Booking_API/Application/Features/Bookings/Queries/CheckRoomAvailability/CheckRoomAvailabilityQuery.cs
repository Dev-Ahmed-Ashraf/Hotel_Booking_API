using Hotel_Booking_API.Application.Common;
using MediatR;

namespace Hotel_Booking_API.Application.Features.Bookings.Queries.CheckRoomAvailability
{
    public class CheckRoomAvailabilityQuery : IRequest<ApiResponse<bool>>
    {
        public int RoomId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
    }
}
