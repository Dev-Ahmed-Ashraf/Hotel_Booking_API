using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.DTOs;
using MediatR;

namespace Hotel_Booking_API.Application.Features.Bookings.Commands.CreateBooking
{
    /// <summary>
    /// Command for creating a new booking in the system.
    /// Validates room availability and calculates total price.
    /// </summary>
    public class CreateBookingCommand : IRequest<ApiResponse<BookingDto>>
    {
        public CreateBookingDto CreateBookingDto { get; set; } = null!;
        public int UserId { get; set; }
    }
}
