using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.DTOs;
using MediatR;

namespace Hotel_Booking_API.Application.Features.Bookings.Commands.CancelBooking
{
    /// <summary>
    /// Command for cancelling an existing booking in the system.
    /// Changes booking status to Cancelled and restores room availability.
    /// </summary>
    public class CancelBookingCommand : IRequest<ApiResponse<string>>
    {
        public int Id { get; set; }
        public CancelBookingDto CancelBookingDto { get; set; } = null!;
    }
}
