using Hotel_Booking_API.Domain.Entities;
using Hotel_Booking_API.Domain.Interfaces;

namespace Hotel_Booking.Domain.Interfaces
{
    public interface IRoomRepository
    {
        Task<bool> IsRoomAvailableAsync(int roomId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    }
}
