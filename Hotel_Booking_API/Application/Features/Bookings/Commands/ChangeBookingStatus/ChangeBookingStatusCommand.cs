using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.DTOs;
using MediatR;

namespace Hotel_Booking_API.Application.Features.Bookings.Commands.ChangeBookingStatus
{
    /// <summary>
    /// Command for changing the status of an existing booking in the system.
    /// Updates booking status with validation of status transitions.
    /// </summary>
    public class ChangeBookingStatusCommand : IRequest<ApiResponse<BookingDto>>
    {
        public int Id { get; set; }
        public ChangeBookingStatusDto ChangeBookingStatusDto { get; set; } = null!;
    }
}
