using Hotel_Booking.Domain.Interfaces;
using Hotel_Booking_API.Domain.Enums;
using Hotel_Booking_API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Hotel_Booking.Infrastructure.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public RoomRepository(ApplicationDbContext dbContext) 
        {
            _dbContext = dbContext;
        }

        public async Task<bool> IsRoomAvailableAsync(
            int roomId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default)
        {
            var overlappingBookings = await _dbContext.Bookings
                .Where(b => b.RoomId == roomId &&
                            b.CheckInDate < endDate &&
                            b.CheckOutDate > startDate &&
                            b.Status != BookingStatus.Cancelled &&
                            b.Status != BookingStatus.Completed)
                .AnyAsync(cancellationToken);

            return !overlappingBookings;
        }
    }
}
