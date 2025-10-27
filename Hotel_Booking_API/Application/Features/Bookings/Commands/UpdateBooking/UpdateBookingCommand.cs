using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.DTOs;
using MediatR;

namespace Hotel_Booking_API.Application.Features.Bookings.Commands.UpdateBooking
{
    /// <summary>
    /// Command for updating an existing booking in the system.
    /// Allows partial updates of booking details.
    /// </summary>
    public class UpdateBookingCommand : IRequest<ApiResponse<BookingDto>>
    {
        public int Id { get; set; }
        public UpdateBookingDto UpdateBookingDto { get; set; } = null!;
    }
}
