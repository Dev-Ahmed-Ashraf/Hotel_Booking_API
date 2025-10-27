using Hotel_Booking_API.Application.Common;
using MediatR;

namespace Hotel_Booking_API.Application.Features.Bookings.Commands.DeleteBooking
{
    /// <summary>
    /// Command for deleting an existing booking in the system.
    /// Performs soft delete by default with optional force delete.
    /// </summary>
    public class DeleteBookingCommand : IRequest<ApiResponse<string>>
    {
        public int Id { get; set; }
        public bool IsSoft { get; set; } = true;
        public bool ForceDelete { get; set; } = false;
    }
}
