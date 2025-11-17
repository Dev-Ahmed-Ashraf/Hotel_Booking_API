using Hotel_Booking_API.Domain.Entities;

namespace Hotel_Booking_API.Domain.Interfaces
{
    public interface IRoomRepository : IRepository<Room>
    {
        Task<bool> IsRoomAvailableAsync(int roomId, DateTime startDate, DateTime endDate, int? excludeBookingId = null, CancellationToken cancellationToken = default);
    }
}
